public class TransactionMonthTests
{
    [Fact]
    public void AuctionRecordsExplicitTransactionMonth()
    {
        House house = TestHouseFactory.Create(100);
        Buyer buyer = new("Buyer", 30, 100, 5, 100, false);
        _ = new Bid(buyer, house, 105);

        Transaction transaction = new AuctionService()
            .Settle(house, new Random(1), transactionMonth: 7)!;

        Assert.Equal(7, transaction.Month);
    }

    [Fact]
    public void TransactionRejectsMonthBeforeOne()
    {
        House house = TestHouseFactory.Create(100);
        Buyer buyer = new("Buyer", 30, 100, 5, 100, false);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Transaction(buyer, house, 100, month: 0));
    }

    [Fact]
    public void SimulationTransactionsTrackSuccessiveTicks()
    {
        Market market = new();
        AddPurchasablePair(market, "First");
        Simulation simulation = new(
            market,
            new DataGenerator(new Random(4)),
            new SimulationSettings { NewBuyersPerMonth = 0, NewHousesPerMonth = 0 });

        simulation.RunTick();
        AddPurchasablePair(market, "Second");
        simulation.RunTick();

        Assert.Equal([1, 2], market.Transactions.Select(transaction => transaction.Month));
    }

    private static void AddPurchasablePair(Market market, string name)
    {
        market.Houses.Add(TestHouseFactory.Create(100, name));
        market.Buyers.Add(new Buyer($"{name} Buyer", 35, 200, 10, 200, false));
    }
}
