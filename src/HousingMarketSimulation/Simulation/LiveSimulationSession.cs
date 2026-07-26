public sealed record LiveSimulationConfiguration(
    int InitialBuyers,
    int InitialHouses,
    int DurationMonths,
    int Seed,
    int NewBuyersPerMonth = 1,
    int NewHousesPerMonth = 1)
{
    public void Validate()
    {
        if (InitialBuyers < 0)
            throw new ArgumentOutOfRangeException(nameof(InitialBuyers));
        if (InitialHouses < 0)
            throw new ArgumentOutOfRangeException(nameof(InitialHouses));
        if (DurationMonths < 1)
            throw new ArgumentOutOfRangeException(nameof(DurationMonths));
        if (NewBuyersPerMonth < 0)
            throw new ArgumentOutOfRangeException(nameof(NewBuyersPerMonth));
        if (NewHousesPerMonth < 0)
            throw new ArgumentOutOfRangeException(nameof(NewHousesPerMonth));
    }
}

public sealed class LiveSimulationSession
{
    private readonly Simulation simulation;

    public LiveSimulationSession(LiveSimulationConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        configuration.Validate();
        Configuration = configuration;

        Market = new Market();
        DataGenerator generator = new(new Random(configuration.Seed));
        generator.GenerateData(
            configuration.InitialBuyers,
            configuration.InitialHouses,
            Market);
        simulation = new Simulation(
            Market,
            generator,
            new SimulationSettings
            {
                NewBuyersPerMonth = configuration.NewBuyersPerMonth,
                NewHousesPerMonth = configuration.NewHousesPerMonth
            });
    }

    public LiveSimulationConfiguration Configuration { get; }
    public Market Market { get; }
    public int CurrentMonth => simulation.CurrentMonth;
    public bool IsComplete => CurrentMonth >= Configuration.DurationMonths;
    public MonthlyAnalyticsSnapshot? LatestSnapshot =>
        Market.AnalyticsSnapshots.LastOrDefault();

    public bool AdvanceOneMonth()
    {
        if (IsComplete) return false;
        simulation.RunTick();
        return true;
    }
}
