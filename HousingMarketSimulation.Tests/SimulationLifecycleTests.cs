public class SimulationLifecycleTests
{
    private static readonly SimulationSettings NoEntrants = new()
    {
        NewBuyersPerMonth = 0,
        NewHousesPerMonth = 0
    };

    [Fact]
    public void UnsoldHouseWithoutBidsReceivesPriceReduction()
    {
        Market market = new();
        House house = TestHouseFactory.Create(askingPrice: 100);
        market.Houses.Add(house);

        new Simulation(market, new DataGenerator(new Random(1)), NoEntrants).RunTick();

        // Market-target movement is capped at 3%, followed by the configured 2% no-bid reduction.
        Assert.InRange(house.AskingPrice, 94.99f, 100.99f);
        Assert.True(Assert.Single(market.MonthlyReports).PriceReductions >= 1);
        Assert.Equal(1, house.MonthsOnMarket);
    }

    [Fact]
    public void MonthlyReportUsesEvaluationAndPreEntrantEndOfMonthSnapshots()
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
        Assert.Equal(report.HousesRemaining, report.MarketInventoryEnd);
        Assert.InRange(report.AverageAskingPriceDuringMonth, 97, 103);
        Assert.Equal(report.AverageAskingPriceDuringMonth, report.MedianAskingPrice);
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
    }

    [Fact]
    public void ReportCalculatesSaleMetricsFromAuctionSnapshot()
    {
        Market market = new();
        House house = TestHouseFactory.Create(100);
        Buyer buyer = new("Buyer", 35, 200, 10, 200, false);
        market.Houses.Add(house);
        market.Buyers.Add(buyer);

        new Simulation(market, new DataGenerator(new Random(4)), NoEntrants).RunTick();

        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Transaction transaction = Assert.Single(market.Transactions);
        Assert.Equal(transaction.SalePrice, report.MedianSalePrice);
        Assert.Equal(transaction.SalePrice, report.TotalTransactionValue);
        Assert.Equal(transaction.SalePrice / transaction.ListPrice, report.AverageSaleToListRatio);
        Assert.Equal(1, report.AverageTimeOnMarketForSoldHouses);
        Assert.Equal(0, report.MarketInventoryEnd);
    }
}
