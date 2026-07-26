public class SimulationLifecycleTests
{
    [Fact]
    public void UnsoldHouseWithoutBidsReceivesPriceReduction()
    {
        Market market = new();
        House house = TestHouseFactory.Create(100);
        market.Houses.Add(house);

        new Simulation(market, new DataGenerator(new Random(1))).RunTick();

        Assert.Equal(98f, house.AskingPrice);
        Assert.Equal(1, Assert.Single(market.MonthlyReports).PriceReductions);
    }

    [Fact]
    public void MonthlyReportUsesPreEntrantEndOfMonthSnapshot()
    {
        Market market = new();
        House house = TestHouseFactory.Create(100);
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
        market.Houses.Add(TestHouseFactory.Create(30, "Affordable"));
        Simulation simulation = new(market, new DataGenerator(new Random(3)));

        simulation.RunTick();
        simulation.RunTick();

        Assert.Equal(0, market.MonthlyReports[0].BidsPlaced);
        Assert.Equal(0, market.MonthlyReports[0].BuyersActiveDuringMonth);
        Assert.Equal(1, market.MonthlyReports[1].BuyersActiveDuringMonth);
        Assert.True(market.MonthlyReports[1].BidsPlaced > 0);
    }

    private static Buyer CreateBuyer(string name)
    {
        return new Buyer(name, 30, 100, 5, 100, false);
    }
}
