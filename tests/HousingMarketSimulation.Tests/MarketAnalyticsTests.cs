public class MarketAnalyticsTests
{
    [Fact]
    public void MonthsOfSupplyIsNullWhenNoTransactionsOccur()
    {
        MarketAnalyticsService service = new();
        House inventory = TestHouseFactory.Create(100);

        MonthlyAnalyticsSnapshot snapshot = service.CreateSnapshot(
            1,
            1,
            [],
            [inventory],
            0,
            [],
            [inventory],
            [],
            new AffordabilityObservation(null, null, null));

        Assert.Null(snapshot.MonthsOfSupply);
    }

    [Fact]
    public void AffordabilityUsesBuyersBeforeSuccessfulBuyerRemoval()
    {
        Market market = new();
        market.Houses.Add(TestHouseFactory.Create(100));
        market.Buyers.Add(new Buyer("Buyer", 35, 200, 10, 200, false));
        Simulation simulation = new(
            market,
            new DataGenerator(new Random(12)),
            new SimulationSettings { NewBuyersPerMonth = 0, NewHousesPerMonth = 0 });

        simulation.RunTick();

        MonthlyAnalyticsSnapshot snapshot = Assert.Single(market.AnalyticsSnapshots);
        Assert.Empty(market.Buyers);
        Assert.Equal(1, snapshot.ActiveBuyers);
        Assert.NotNull(snapshot.MedianBuyerMaximumPurchasePrice);
        Assert.Equal(1f, snapshot.PercentageBuyersCapableOfBidding);
    }

    [Fact]
    public void SnapshotIsTakenBeforeNewEntrants()
    {
        Market market = new();
        market.Houses.Add(TestHouseFactory.Create(200));
        Simulation simulation = new(
            market,
            new DataGenerator(new Random(13)),
            new SimulationSettings { NewBuyersPerMonth = 0, NewHousesPerMonth = 1 });

        simulation.RunTick();

        MonthlyAnalyticsSnapshot snapshot = Assert.Single(market.AnalyticsSnapshots);
        Assert.Equal(1, snapshot.ActiveListings);
        Assert.Equal(1, snapshot.EndingInventory);
        Assert.Equal(1, snapshot.NewListings);
        Assert.Equal(2, market.Houses.Count);
    }

    [Fact]
    public void SnapshotContainsOnlyFiniteNumericValues()
    {
        Market market = new();
        DataGenerator generator = new(new Random(14));
        generator.GenerateData(20, 15, market);
        Simulation simulation = new(market, generator);
        for (int month = 0; month < 24; month++) simulation.RunTick();

        Assert.Equal(24, market.AnalyticsSnapshots.Count);
        Assert.All(market.AnalyticsSnapshots, AssertFinite);
    }

    private static void AssertFinite(MonthlyAnalyticsSnapshot snapshot)
    {
        float?[] values =
        [
            snapshot.RawAverageAskingPrice,
            snapshot.RawMedianAskingPrice,
            snapshot.RawAverageSalePrice,
            snapshot.RawMedianSalePrice,
            snapshot.ConstantQualityPriceIndex,
            snapshot.AverageSaleToListRatio,
            snapshot.BuyerToListingRatio,
            snapshot.BidsPerActiveListing,
            snapshot.ClearanceRate,
            snapshot.MonthsOfSupply,
            snapshot.AverageTimeOnMarket,
            snapshot.MedianTimeOnMarket,
            snapshot.PercentageActiveListingsSold,
            snapshot.PercentageActiveBuyersPurchasing,
            snapshot.TotalTransactionValue,
            snapshot.MedianBuyerMaximumPurchasePrice,
            snapshot.AskingPriceToPurchasingPowerRatio,
            snapshot.PercentageBuyersCapableOfBidding
        ];
        Assert.All(values.Where(value => value.HasValue), value =>
            Assert.True(float.IsFinite(value!.Value)));
    }
}
