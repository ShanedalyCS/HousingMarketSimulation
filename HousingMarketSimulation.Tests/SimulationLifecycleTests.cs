public class SimulationLifecycleTests
{
    [Fact]
    public void UnsoldHouseWithoutBidsReceivesPriceReduction()
    {
        Market market = new();
        House house = new("House", 100, 1, 1, 1, 1);
        market.Houses.Add(house);

        new Simulation(market, new DataGenerator(new Random(1))).RunTick();

        Assert.Equal(98f, house.Value);
        Assert.Equal(1, Assert.Single(market.MonthlyReports).PriceReductions);
    }

    [Fact]
    public void MonthlyReportUsesPreEntrantEndOfMonthSnapshot()
    {
        Market market = new();
        House house = new("House", 100, 1, 1, 1, 1);
        market.Houses.Add(house);
        Simulation simulation = new(market, new DataGenerator(new Random(2)));

        simulation.RunTick();

        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Assert.Equal(0, report.BuyersActiveDuringMonth);
        Assert.Equal(1, report.HousesActiveDuringMonth);
        Assert.Equal(0, report.BuyersRemaining);
        Assert.Equal(1, report.HousesRemaining);
        Assert.Equal(98f, report.AverageAskingPriceDuringMonth);
        Assert.Single(market.Buyers);
        Assert.Equal(2, market.Houses.Count);
    }

    [Fact]
    public void EntrantsParticipateStartingInFollowingMonth()
    {
        Market market = new();
        market.Houses.Add(new House("Affordable", 30, 20, 20, 20, 20));
        Simulation simulation = new(market, new DataGenerator(new Random(3)));

        simulation.RunTick();
        simulation.RunTick();

        Assert.Equal(0, market.MonthlyReports[0].BidsPlaced);
        Assert.Equal(0, market.MonthlyReports[0].BuyersActiveDuringMonth);
        Assert.Equal(1, market.MonthlyReports[1].BuyersActiveDuringMonth);
        Assert.True(market.MonthlyReports[1].BidsPlaced > 0);
    }

    [Fact]
    public void ZeroPriceDiscoveryIsReportedSeparately()
    {
        Market market = new();
        market.Houses.Add(new House("House", 0, 10, 10, 10, 10));
        market.Buyers.Add(CreateBuyer("A"));
        market.Buyers.Add(CreateBuyer("B"));
        market.Buyers.Add(CreateBuyer("C"));
        Simulation simulation = new(
            market,
            new DataGenerator(new Random(4)),
            usePriceDiscoveryMode: true);

        simulation.RunTick();

        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Assert.Equal(1, report.PriceDiscoveries);
        Assert.Equal(0, report.PriceIncreases);
        Assert.Equal(1, report.TransactionsCompleted);
    }

    private static Buyer CreateBuyer(string name)
    {
        return new Buyer(name, 30, 100, 5, 100, false);
    }
}
