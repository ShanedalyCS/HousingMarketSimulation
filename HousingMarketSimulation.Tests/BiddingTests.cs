public class BiddingTests
{
    [Fact]
    public void HighestValidBidWins()
    {
        House house = CreateHouse(100);
        Buyer lowerBidder = CreateBuyer("Lower");
        Buyer higherBidder = CreateBuyer("Higher");
        _ = new Bid(lowerBidder, house, 101);
        _ = new Bid(higherBidder, house, 105);

        Transaction? transaction = house.DeliberateBids(new Random(1));

        Assert.NotNull(transaction);
        Assert.Same(higherBidder, transaction.Buyer);
        Assert.Equal(105f, transaction.SalePrice);
    }

    [Fact]
    public void BidBelowAskingPriceIsRejected()
    {
        House house = CreateHouse(100);
        _ = new Bid(CreateBuyer("Buyer"), house, 99);

        Assert.Null(house.DeliberateBids(new Random(1)));
    }

    [Fact]
    public void BidsUseAskingPriceThatExistedWhenSubmitted()
    {
        Market market = new();
        House house = CreateHouse(100);
        market.Houses.Add(house);
        market.Buyers.Add(CreateBuyer("A", motivation: 0));
        market.Buyers.Add(CreateBuyer("B", motivation: 0));
        market.Buyers.Add(CreateBuyer("C", motivation: 0));
        Simulation simulation = new(market, new DataGenerator(new Random(10)));

        simulation.RunTick();

        Transaction transaction = Assert.Single(market.Transactions);
        Assert.Equal(100f, transaction.SalePrice);
        Assert.DoesNotContain(house, market.Houses);
        MonthlyMarketReport report = Assert.Single(market.MonthlyReports);
        Assert.Equal(0, report.PriceIncreases);
        Assert.Equal(100f, report.AverageAskingPriceDuringMonth);
    }

    [Fact]
    public void TiedBidOutcomeIsReproducibleWithSameSeed()
    {
        string firstWinner = RunTiedBid(42);
        string secondWinner = RunTiedBid(42);

        Assert.Equal(firstWinner, secondWinner);
    }

    private static string RunTiedBid(int seed)
    {
        House house = CreateHouse(100);
        _ = new Bid(CreateBuyer("A"), house, 105);
        _ = new Bid(CreateBuyer("B"), house, 105);

        return house.DeliberateBids(new Random(seed))!.Buyer.Name;
    }

    private static House CreateHouse(float price)
    {
        return new House("House", price, 10, 10, 10, 10);
    }

    private static Buyer CreateBuyer(string name, float motivation = 5)
    {
        return new Buyer(name, 30, 100, motivation, 100, false);
    }
}
