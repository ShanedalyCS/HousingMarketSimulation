public class RandomnessTests
{
    [Fact]
    public void IdenticalSeedsGenerateIdenticalInitialMarkets()
    {
        Market first = GenerateMarket(12345);
        Market second = GenerateMarket(12345);

        Assert.Equal(
            first.Buyers.Select(BuyerSnapshot),
            second.Buyers.Select(BuyerSnapshot));
        Assert.Equal(
            first.Houses.Select(HouseSnapshot),
            second.Houses.Select(HouseSnapshot));
    }

    [Fact]
    public void GeneratedBuyersHaveRealisticReproducibleVariation()
    {
        Market first = GenerateMarket(999, buyers: 50, houses: 0);
        Market second = GenerateMarket(999, buyers: 50, houses: 0);

        Assert.All(first.Buyers, buyer => Assert.InRange(buyer.Age, 18, 70));
        Assert.Contains(first.Buyers, buyer => buyer.HasFamily);
        Assert.Contains(first.Buyers, buyer => !buyer.HasFamily);
        Assert.True(first.Buyers.Select(buyer => buyer.Savings / buyer.Salary).Distinct().Count() > 1);
        Assert.Equal(
            first.Buyers.Select(BuyerSnapshot),
            second.Buyers.Select(BuyerSnapshot));
    }

    [Fact]
    public void CompleteSimulationIsReproducibleWithSameSeed()
    {
        string first = RunSimulation(1234);
        string second = RunSimulation(1234);

        Assert.Equal(first, second);
    }

    private static Market GenerateMarket(int seed, int buyers = 5, int houses = 5)
    {
        Market market = new();
        new DataGenerator(new Random(seed)).GenerateData(buyers, houses, market);
        return market;
    }

    private static string BuyerSnapshot(Buyer buyer)
    {
        BuyerPreferences p = buyer.Preferences;
        return $"{buyer.Name}|{buyer.Age}|{buyer.Salary}|{buyer.Motivation}|{buyer.Savings}|" +
            $"{buyer.HasFamily}|{p.LocationWeight}|{p.BuildQualityWeight}|" +
            $"{p.FloorAreaWeight}|{p.PlotSizeWeight}|{p.HouseAgeWeight}";
    }

    private static string HouseSnapshot(House house)
    {
        return $"{house.Name}|{house.BaseValue}|{house.AskingPrice}|" +
            $"{house.BuildQuality}|{house.Location}|{house.AgeYears}|" +
            $"{house.FloorAreaSquareMetres}|{house.PlotSizeSquareMetres}|" +
            $"{house.SellerPricingMultiplier}";
    }

    private static string RunSimulation(int seed)
    {
        Market market = GenerateMarket(seed, buyers: 20, houses: 15);
        DataGenerator generator = new(new Random(seed + 1));
        Simulation simulation = new(market, generator);
        for (int month = 0; month < 24; month++) simulation.RunTick();

        return string.Join(
            Environment.NewLine,
            market.Transactions.Select(transaction =>
                $"T|{transaction.Buyer.Name}|{transaction.House.Name}|{transaction.SalePrice}")
            .Concat(market.MonthlyReports.Select(report =>
                $"R|{report.Month}|{report.BidsPlaced}|{report.TransactionsCompleted}|" +
                $"{report.MedianAskingPrice}|{report.MedianSalePrice}|" +
                $"{report.TotalTransactionValue}|{report.MarketInventoryEnd}")));
    }
}
