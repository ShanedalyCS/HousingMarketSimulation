public class HousingMarketSimulation
{
    public static void Main(string[] args)
    {
        if (args.Any(argument =>
            string.Equals(argument, "--scenarios", StringComparison.OrdinalIgnoreCase)))
        {
            string outputDirectory = Path.Combine(
                Environment.CurrentDirectory,
                "scenario-output");
            ScenarioRunner.RunAll(outputDirectory, Console.Out);
            return;
        }

        ConsoleInputReader input = new(Console.In, Console.Out);
        int numberOfBuyers = input.ReadNonNegativeInt("How many people? ");
        int numberOfHouses = input.ReadNonNegativeInt("How many houses? ");
        int? seed = input.ReadOptionalSeed(
            "Optional random seed (press Enter for a random run): ");
        int numberOfTicks = input.ReadNonNegativeInt("How many months to simulate? ");
        SimulationSettings settings = input.ReadSimulationSettings();

        Market market = new();
        Random random = seed.HasValue ? new Random(seed.Value) : new Random();
        DataGenerator dataGenerator = new(random);
        dataGenerator.GenerateData(numberOfBuyers, numberOfHouses, market);

        Simulation simulation = new(market, dataGenerator, settings);
        for (int i = 0; i < numberOfTicks; i++) simulation.RunTick();

        string reportPath = Path.Combine(
            Environment.CurrentDirectory,
            "monthly-market-reports.csv");
        MonthlyReportCsvExporter.Export(market.MonthlyReports, reportPath);
        Console.WriteLine($"Monthly reports exported to {reportPath}");
    }
}
