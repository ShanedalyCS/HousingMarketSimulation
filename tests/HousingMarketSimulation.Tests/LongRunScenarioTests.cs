[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ConsoleSensitiveCollection
{
    public const string Name = "Console-sensitive scenarios";
}

[Collection(ConsoleSensitiveCollection.Name)]
public class LongRunScenarioTests
{
    [Fact]
    public void SeededSimulationRemainsSaneAndReproducibleFor120Months()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            SimulationScenario scenario = ScenarioRunner.Scenarios[0];
            ScenarioRunResult first = ScenarioRunner.Run(scenario, directory);
            string firstSnapshot = Snapshot(first);
            ScenarioRunResult second = ScenarioRunner.Run(scenario, directory);

            ValidateLongRun(first);
            ValidateLongRun(second);
            Assert.Equal(firstSnapshot, Snapshot(second));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void BalancedDemandAndSupplyScenariosCompleteWithoutInvalidState()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            IReadOnlyList<ScenarioRunResult> results =
                ScenarioRunner.RunAll(directory, TextWriter.Null);

            Assert.Equal(3, results.Count);
            Assert.Equal(1, results[0].Scenario.Settings.NewBuyersPerMonth);
            Assert.Equal(1, results[0].Scenario.Settings.NewHousesPerMonth);
            Assert.True(results[1].Scenario.InitialBuyers > results[1].Scenario.InitialHouses);
            Assert.True(results[1].Scenario.Settings.NewBuyersPerMonth
                > results[1].Scenario.Settings.NewHousesPerMonth);
            Assert.True(results[2].Scenario.InitialHouses > results[2].Scenario.InitialBuyers);
            Assert.True(results[2].Scenario.Settings.NewHousesPerMonth
                > results[2].Scenario.Settings.NewBuyersPerMonth);

            foreach (ScenarioRunResult result in results)
            {
                ValidateLongRun(result);
                Assert.True(File.Exists(result.CsvPath));
                Assert.True(File.Exists(result.AnalyticsCsvPath));
                Assert.True(File.Exists(result.DashboardJsonPath));
                string[] csvLines = File.ReadAllLines(result.CsvPath);
                Assert.Equal(121, csvLines.Length);
                Assert.DoesNotContain("MarketInventoryEnd", csvLines[0]);
                Assert.All(csvLines, line => Assert.Equal(19, line.Split(',').Length));
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void ValidateLongRun(ScenarioRunResult result)
    {
        Market market = result.Market;
        Assert.Equal(120, market.MonthlyReports.Count);
        Assert.Equal(120, market.AnalyticsSnapshots.Count);
        Assert.Equal(
            market.Transactions.Count,
            market.MonthlyReports.Sum(report => report.TransactionsCompleted));

        HashSet<House> soldHouses = [];
        HashSet<Buyer> successfulBuyers = [];
        Assert.All(market.Transactions, transaction =>
        {
            Assert.True(soldHouses.Add(transaction.House), "A house was sold more than once.");
            Assert.True(successfulBuyers.Add(transaction.Buyer), "A buyer purchased more than once.");
            AssertFiniteNonNegative(transaction.SalePrice);
            AssertFiniteNonNegative(transaction.ListPrice);
        });

        IEnumerable<House> allKnownHouses = market.Houses
            .Concat(market.Transactions.Select(transaction => transaction.House))
            .Distinct();
        Assert.All(allKnownHouses, house =>
        {
            AssertFiniteNonNegative(house.BaseValue);
            AssertFiniteNonNegative(house.EstimatedMarketValue);
            AssertFiniteNonNegative(house.AskingPrice);
        });

        int transactionOffset = 0;
        foreach (MonthlyMarketReport report in market.MonthlyReports)
        {
            Assert.True(report.TransactionsCompleted <= report.BuyersActiveDuringMonth);
            Assert.True(report.TransactionsCompleted <= report.HousesActiveDuringMonth);
            Assert.True(report.BuyersRemaining >= 0);
            Assert.True(report.HousesRemaining >= 0);
            Assert.True(report.PriceReductions + report.PriceIncreases
                <= report.HousesActiveDuringMonth);

            AssertFiniteNonNegative(report.AverageAskingPriceDuringMonth);
            AssertFiniteNonNegative(report.AverageSalePrice);
            AssertFiniteNonNegative(report.MedianAskingPrice);
            AssertFiniteNonNegative(report.MedianSalePrice);
            AssertFiniteNonNegative(report.AverageSaleToListRatio);
            AssertFiniteNonNegative(report.AverageTimeOnMarketForSoldHouses);
            AssertFiniteNonNegative(report.TotalTransactionValue);
            Assert.True(float.IsFinite(report.AskingPriceChange));
            Assert.True(float.IsFinite(report.AskingPricePercentageChange));

            Transaction[] monthlyTransactions = market.Transactions
                .Skip(transactionOffset)
                .Take(report.TransactionsCompleted)
                .ToArray();
            Assert.Equal(report.TransactionsCompleted, monthlyTransactions.Length);
            Assert.Equal(
                monthlyTransactions.Sum(transaction => transaction.SalePrice),
                report.TotalTransactionValue,
                precision: 2);
            transactionOffset += report.TransactionsCompleted;
        }
        Assert.Equal(market.Transactions.Count, transactionOffset);
        Assert.All(market.AnalyticsSnapshots, snapshot =>
        {
            Assert.Equal(snapshot.Month == 1
                ? result.Scenario.InitialHouses
                : result.Scenario.Settings.NewHousesPerMonth,
                snapshot.NewListings);
            Assert.True(snapshot.ActiveListings >= snapshot.Transactions);
            Assert.True(snapshot.EndingInventory >= 0);
            AssertNullableFinite(snapshot.ConstantQualityPriceIndex);
            AssertNullableFinite(snapshot.MonthsOfSupply);
            AssertNullableFinite(snapshot.MedianBuyerMaximumPurchasePrice);
            AssertNullableFinite(snapshot.PercentageBuyersCapableOfBidding);
        });

        ScenarioSummary summary = result.Summary;
        Assert.Equal(market.Transactions.Count, summary.TotalTransactions);
        Assert.Equal(market.Buyers.Count, summary.FinalActiveBuyers);
        Assert.Equal(market.Houses.Count, summary.FinalInventory);
        AssertFiniteNonNegative(summary.StartingAverageAskingPrice);
        AssertFiniteNonNegative(summary.EndingAverageAskingPrice);
        Assert.True(float.IsFinite(summary.AskingPricePercentageChange));
        AssertFiniteNonNegative(summary.AverageSaleToListRatio);
        AssertFiniteNonNegative(summary.AverageTimeOnMarket);
        AssertFiniteNonNegative(summary.TotalTransactionValue);
    }

    private static void AssertFiniteNonNegative(float value)
    {
        Assert.True(float.IsFinite(value), $"Expected a finite value, received {value}.");
        Assert.True(value >= 0, $"Expected a non-negative value, received {value}.");
    }

    private static void AssertNullableFinite(float? value)
    {
        if (value.HasValue) Assert.True(float.IsFinite(value.Value));
    }

    private static string Snapshot(ScenarioRunResult result)
    {
        IEnumerable<string> transactions = result.Market.Transactions.Select(transaction =>
            $"T|{transaction.House.Name}|{transaction.Buyer.Name}|{transaction.SalePrice}");
        IEnumerable<string> reports = result.Market.MonthlyReports.Select(report =>
            $"R|{report.Month}|{report.BidsPlaced}|{report.TransactionsCompleted}|" +
                $"{report.PriceReductions}|{report.PriceIncreases}|{report.HousesRemaining}|" +
                $"{report.AverageAskingPriceDuringMonth}|{report.MedianAskingPrice}|" +
                $"{report.AverageSalePrice}|{report.TotalTransactionValue}");
        IEnumerable<string> analytics = result.Market.AnalyticsSnapshots.Select(snapshot =>
            $"A|{snapshot.Month}|{snapshot.ConstantQualityPriceIndex}|" +
            $"{snapshot.ActiveBuyers}|{snapshot.ActiveListings}|" +
            $"{snapshot.EndingInventory}|{snapshot.MonthsOfSupply}");
        return string.Join(
            Environment.NewLine,
            transactions.Concat(reports).Concat(analytics));
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"housing-market-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
