public class BiddingTests
{
    [Fact]
    public void SingleValidBidderPurchasesAtAskingPrice()
    {
        House house = TestHouseFactory.Create(100);
        Buyer buyer = CreateBuyer("Buyer");
        _ = new Bid(buyer, house, 105);

        Transaction? transaction = new AuctionService().Settle(house, new Random(1), 1);

        Assert.NotNull(transaction);
        Assert.Same(buyer, transaction.Buyer);
        Assert.Equal(100f, transaction.SalePrice);
    }

    [Fact]
    public void BidBelowAskingPriceIsRejected()
    {
        House house = TestHouseFactory.Create(100);
        _ = new Bid(CreateBuyer("Buyer"), house, 99);

        Assert.Null(new AuctionService().Settle(house, new Random(1), 1));
    }

    [Fact]
    public void MultipleBiddersUseSecondPricePlusIncrement()
    {
        House house = TestHouseFactory.Create(100);
        Buyer winner = CreateBuyer("Winner");
        _ = new Bid(CreateBuyer("Lower"), house, 104);
        _ = new Bid(winner, house, 110);

        Transaction transaction = new AuctionService(
            new SimulationSettings { AuctionIncrement = 0.5f })
            .Settle(house, new Random(1), 1)!;

        Assert.Same(winner, transaction.Buyer);
        Assert.Equal(104.5f, transaction.SalePrice);
        Assert.True(transaction.SalePrice <= 110);
    }

    [Fact]
    public void SettlementNeverExceedsWinningMaximumBid()
    {
        House house = TestHouseFactory.Create(100);
        _ = new Bid(CreateBuyer("A"), house, 105);
        _ = new Bid(CreateBuyer("B"), house, 105);

        Transaction transaction = new AuctionService(
            new SimulationSettings { AuctionIncrement = 10 })
            .Settle(house, new Random(1), 1)!;

        Assert.Equal(105, transaction.SalePrice);
    }

    [Fact]
    public void TiedBidOutcomeIsReproducibleWithSameSeed()
    {
        Assert.Equal(RunTiedBid(42), RunTiedBid(42));
    }

    [Fact]
    public void SuccessfulBuyerCannotWinAnotherAuctionInSameMonth()
    {
        Buyer buyer = CreateBuyer("Buyer");
        House first = TestHouseFactory.Create(100, "First");
        House second = TestHouseFactory.Create(100, "Second");
        _ = new Bid(buyer, first, 110);
        _ = new Bid(buyer, second, 110);
        HashSet<Buyer> successful = [];
        AuctionService service = new();

        Assert.NotNull(service.Settle(first, new Random(1), 1, successful));
        Assert.Null(service.Settle(second, new Random(1), 1, successful));
    }

    private static string RunTiedBid(int seed)
    {
        House house = TestHouseFactory.Create(100);
        _ = new Bid(CreateBuyer("A"), house, 105);
        _ = new Bid(CreateBuyer("B"), house, 105);
        return new AuctionService().Settle(house, new Random(seed), 1)!.Buyer.Name;
    }

    private static Buyer CreateBuyer(string name) =>
        new(name, 30, 100, 5, 100, false);
}
