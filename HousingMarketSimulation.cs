public class HousingMarketSimulation
{

    public static void Main(string[] args)
    {
        Market market = new();

        Console.WriteLine("How many people?");
        int numberOfBuyers = int.Parse(Console.ReadLine()!);

        Console.WriteLine("How many houses?");
        int numberOfHouses = int.Parse(Console.ReadLine()!);

        Console.WriteLine("Use zero-price market discovery mode? (y/n)");
        bool usePriceDiscoveryMode = Console.ReadLine()?.Trim().ToLowerInvariant() == "y";

        Console.WriteLine("Optional random seed (press Enter for a random run):");
        string? seedInput = Console.ReadLine();
        Random random = int.TryParse(seedInput, out int seed)
            ? new Random(seed)
            : new Random();
        DataGenerator dataGenerator = new(random);

        dataGenerator.GenerateData(
            numberOfBuyers,
            numberOfHouses,
            market,
            usePriceDiscoveryMode);



        Simulation simulation = new(market, dataGenerator, usePriceDiscoveryMode);

        Console.WriteLine("How many months to simulate?");
        int numberOfTicks = int.Parse(Console.ReadLine()!);

        for (int i = 0; i < numberOfTicks; i++)
        {
            simulation.RunTick();
        }

        string reportPath = Path.Combine(
            Environment.CurrentDirectory,
            "monthly-market-reports.csv");
        MonthlyReportCsvExporter.Export(market.MonthlyReports, reportPath);
        Console.WriteLine($"Monthly reports exported to {reportPath}");
    }
}
