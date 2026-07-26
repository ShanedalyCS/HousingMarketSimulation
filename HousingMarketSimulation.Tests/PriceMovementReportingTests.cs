public class PriceMovementReportingTests
{
    private static readonly SimulationSettings NoEntrants = new()
    {
        NewBuyersPerMonth = 0,
        NewHousesPerMonth = 0
    };

    [Fact]
    public void HouseAdjustedTwiceDownwardCountsAsOneReduction()
    {
        Market market = new();
        market.Houses.Add(TestHouseFactory.Create(200));

        new Simulation(market, new DataGenerator(new Random(1)), NoEntrants).RunTick();

        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Assert.Equal(1, report.PriceReductions);
        Assert.Equal(0, report.PriceIncreases);
    }

    [Fact]
    public void UpThenDownCountsByFinalNetMovement()
    {
        Market market = new();
        market.Houses.Add(TestHouseFactory.Create(100));
        SimulationSettings settings = new()
        {
            NewBuyersPerMonth = 0,
            NewHousesPerMonth = 0,
            MaximumMonthlyMarketValueAdjustment = 0.03f,
            NoBidPriceReduction = 0.05f
        };

        new Simulation(market, new DataGenerator(new Random(2)), settings).RunTick();

        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Assert.Equal(1, report.PriceReductions);
        Assert.Equal(0, report.PriceIncreases);
    }

    [Fact]
    public void ListingCannotAppearInBothMovementCounts()
    {
        House first = TestHouseFactory.Create(100, "First");
        House second = TestHouseFactory.Create(100, "Second");
        Dictionary<House, float> starting = new()
        {
            [first] = 100,
            [second] = 100
        };
        first.AskingPrice = 90;
        second.AskingPrice = 110;

        PriceMovementSummary result = PriceMovementCounter.CountNetMovements(starting);

        Assert.Equal(1, result.Reductions);
        Assert.Equal(1, result.Increases);
        Assert.True(result.Reductions + result.Increases <= starting.Count);
    }

    [Fact]
    public void MovementCountsNeverExceedActiveListings()
    {
        Market market = new();
        market.Houses.AddRange(
            Enumerable.Range(0, 6).Select(index =>
                TestHouseFactory.Create(100 + index * 10, $"House {index}")));

        new Simulation(market, new DataGenerator(new Random(3)), NoEntrants).RunTick();

        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Assert.True(report.PriceReductions + report.PriceIncreases
            <= report.HousesActiveDuringMonth);
    }

    [Fact]
    public void SoldHouseIsIncludedInNetMovementCount()
    {
        Market market = new();
        market.Houses.Add(TestHouseFactory.Create(100));
        market.Buyers.Add(new Buyer("Buyer", 35, 200, 10, 200, false));

        new Simulation(market, new DataGenerator(new Random(4)), NoEntrants).RunTick();

        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Assert.Single(market.Transactions);
        Assert.Equal(1, report.PriceIncreases);
        Assert.Equal(0, report.PriceReductions);
    }

    [Fact]
    public void MovementCounterIgnoresDifferencesWithinTolerance()
    {
        House house = TestHouseFactory.Create(100);
        Dictionary<House, float> starting = new() { [house] = 100 };
        house.AskingPrice = 100.004f;

        Assert.Equal(
            new PriceMovementSummary(0, 0),
            PriceMovementCounter.CountNetMovements(starting));
    }
}
