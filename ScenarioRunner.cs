public sealed record SimulationScenario(
    string Name,
    string FileName,
    int Seed,
    int InitialBuyers,
    int InitialHouses,
    int Months,
    SimulationSettings Settings);

public sealed record ScenarioSummary(
    string Name,
    int TotalTransactions,
    int FinalActiveBuyers,
    int FinalInventory,
    float StartingAverageAskingPrice,
    float EndingAverageAskingPrice,
    float AskingPricePercentageChange,
    float AverageSaleToListRatio,
    float AverageTimeOnMarket,
    float TotalTransactionValue);

public sealed record ScenarioRunResult(
    SimulationScenario Scenario,
    ScenarioSummary Summary,
    Market Market,
    string CsvPath);

public static class ScenarioRunner
{
    private static readonly object ConsoleRedirectLock = new();

    public static IReadOnlyList<SimulationScenario> Scenarios { get; } =
    [
        new(
            "Balanced market",
            "balanced-market.csv",
            1101,
            40,
            40,
            120,
            new SimulationSettings
            {
                NewBuyersPerMonth = 1,
                NewHousesPerMonth = 1
            }),
        new(
            "Excess-demand market",
            "excess-demand-market.csv",
            2202,
            80,
            25,
            120,
            new SimulationSettings
            {
                NewBuyersPerMonth = 3,
                NewHousesPerMonth = 1
            }),
        new(
            "Excess-supply market",
            "excess-supply-market.csv",
            3303,
            25,
            80,
            120,
            new SimulationSettings
            {
                NewBuyersPerMonth = 1,
                NewHousesPerMonth = 3
            })
    ];

    public static IReadOnlyList<ScenarioRunResult> RunAll(
        string outputDirectory,
        TextWriter output)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(output);
        Directory.CreateDirectory(outputDirectory);

        List<ScenarioRunResult> results = [];
        foreach (SimulationScenario scenario in Scenarios)
        {
            ScenarioRunResult result = Run(scenario, outputDirectory);
            results.Add(result);
            PrintSummary(result.Summary, output);
        }

        output.WriteLine($"Scenario CSV files written to {Path.GetFullPath(outputDirectory)}");
        return results;
    }

    public static ScenarioRunResult Run(
        SimulationScenario scenario,
        string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        scenario.Settings.Validate();
        if (scenario.InitialBuyers < 0)
            throw new ArgumentOutOfRangeException(nameof(scenario.InitialBuyers));
        if (scenario.InitialHouses < 0)
            throw new ArgumentOutOfRangeException(nameof(scenario.InitialHouses));
        if (scenario.Months < 0)
            throw new ArgumentOutOfRangeException(nameof(scenario.Months));

        Market market = new();
        DataGenerator generator = new(new Random(scenario.Seed));
        generator.GenerateData(scenario.InitialBuyers, scenario.InitialHouses, market);
        float startingAverage = AverageAskingPrice(market.Houses);
        Simulation simulation = new(market, generator, scenario.Settings);

        lock (ConsoleRedirectLock)
        {
            TextWriter originalOutput = Console.Out;
            try
            {
                Console.SetOut(TextWriter.Null);
                for (int month = 0; month < scenario.Months; month++) simulation.RunTick();
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }

        float endingAverage = AverageAskingPrice(market.Houses);
        float askingPricePercentageChange = startingAverage == 0
            ? 0
            : (endingAverage - startingAverage) / startingAverage * 100f;
        ScenarioSummary summary = new(
            scenario.Name,
            market.Transactions.Count,
            market.Buyers.Count,
            market.Houses.Count,
            startingAverage,
            endingAverage,
            askingPricePercentageChange,
            Average(market.Transactions
                .Where(transaction => transaction.ListPrice > 0)
                .Select(transaction => transaction.SalePrice / transaction.ListPrice)),
            Average(market.Transactions.Select(
                transaction => (float)transaction.MonthsOnMarket)),
            market.Transactions.Sum(transaction => transaction.SalePrice));

        Directory.CreateDirectory(outputDirectory);
        string csvPath = Path.Combine(outputDirectory, scenario.FileName);
        MonthlyReportCsvExporter.Export(market.MonthlyReports, csvPath);
        return new ScenarioRunResult(scenario, summary, market, csvPath);
    }

    private static void PrintSummary(ScenarioSummary summary, TextWriter output)
    {
        output.WriteLine();
        output.WriteLine(summary.Name);
        output.WriteLine(
            $"  Transactions: {summary.TotalTransactions}; " +
            $"final buyers/inventory: {summary.FinalActiveBuyers}/{summary.FinalInventory}");
        output.WriteLine(
            $"  Average asking: {summary.StartingAverageAskingPrice:F2} K -> " +
            $"{summary.EndingAverageAskingPrice:F2} K " +
            $"({summary.AskingPricePercentageChange:+0.00;-0.00;0.00}%)");
        output.WriteLine(
            $"  Sale-to-list: {summary.AverageSaleToListRatio:P2}; " +
            $"average time on market: {summary.AverageTimeOnMarket:F2} months; " +
            $"transaction value: {summary.TotalTransactionValue:F2} K");
    }

    private static float AverageAskingPrice(IEnumerable<House> houses) =>
        Average(houses.Where(house => house.AskingPrice > 0)
            .Select(house => house.AskingPrice));

    private static float Average(IEnumerable<float> values)
    {
        float[] materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }
}
