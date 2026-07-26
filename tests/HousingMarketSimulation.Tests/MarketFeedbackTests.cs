public class MarketFeedbackTests
{
    [Fact]
    public void CompletedTransactionsInfluenceEstimateAndLaterAskingPrice()
    {
        HouseValuationService valuation = new();
        House subject = CreateComparableHouse("Subject", valuation);
        House soldComparable = CreateComparableHouse("Sold", valuation);
        float originalBase = subject.BaseValue;
        float originalAsk = subject.AskingPrice;
        Market market = new();
        market.Houses.Add(subject);
        market.Transactions.Add(new Transaction(
            CreateBuyer(), soldComparable, soldComparable.BaseValue * 1.5f, 1));

        new Simulation(
            market,
            new DataGenerator(new Random(10), valuation),
            new SimulationSettings { NewBuyersPerMonth = 0, NewHousesPerMonth = 0 })
            .RunTick();

        Assert.True(subject.EstimatedMarketValue > subject.BaseValue);
        Assert.True(subject.AskingPrice > originalAsk);
        Assert.Equal(originalBase, subject.BaseValue);
    }

    [Fact]
    public void TransactionFeedbackChangesFutureBuyerWillingnessToPay()
    {
        HouseValuationService valuation = new();
        House subject = CreateComparableHouse("Subject", valuation);
        House comparable = CreateComparableHouse("Comparable", valuation);
        Buyer buyer = new(
            "Buyer",
            35,
            500,
            5,
            500,
            false,
            new BuyerPreferences(1, 1, 1, 1, 1));
        BuyerDecisionService decisions = new();
        float before = decisions.Evaluate(buyer, subject).MaximumBid;

        valuation.EstimateMarketValue(
            subject,
            [new Transaction(CreateBuyer(), comparable, comparable.BaseValue * 1.5f, 1)]);
        float after = decisions.Evaluate(buyer, subject).MaximumBid;

        Assert.True(after > before);
    }

    [Fact]
    public void DissimilarTransactionHasNoMaterialInfluence()
    {
        HouseValuationService valuation = new();
        House subject = CreateComparableHouse("Subject", valuation);
        House unrelated = TestHouseFactory.Create(
            100,
            "Unrelated",
            floorArea: 220,
            plotSize: 700,
            age: 90,
            quality: PropertyQuality.Premium,
            location: LocationDesirability.Prime,
            valuationService: valuation);

        float estimate = valuation.EstimateMarketValue(
            subject,
            [new Transaction(CreateBuyer(), unrelated, 2000, 1)]);

        Assert.Equal(subject.BaseValue, estimate);
    }

    [Fact]
    public void OneUnusualComparableHasLimitedInfluence()
    {
        HouseValuationService valuation = new();
        House subject = CreateComparableHouse("Subject", valuation);
        House comparable = CreateComparableHouse("Comparable", valuation);

        float estimate = valuation.EstimateMarketValue(
            subject,
            [new Transaction(CreateBuyer(), comparable, comparable.BaseValue * 10, 1)]);

        Assert.InRange(estimate / subject.BaseValue, 1f, 1.10f);
    }

    [Fact]
    public void NewHouseUsesExistingComparableDataImmediately()
    {
        const int seed = 77;
        Market previewMarket = new();
        new DataGenerator(new Random(seed)).GenerateData(0, 1, previewMarket);
        House preview = Assert.Single(previewMarket.Houses);
        House comparable = new(
            "Comparable",
            preview.FloorAreaSquareMetres,
            preview.PlotSizeSquareMetres,
            preview.AgeYears,
            preview.BuildQuality,
            preview.Location,
            new HouseValuationService(),
            new Random(5));
        Market market = new();
        market.Transactions.Add(new Transaction(
            CreateBuyer(), comparable, comparable.BaseValue * 1.4f, 1));

        new DataGenerator(new Random(seed)).AddMonthlyEntrants(market, 0, 1);

        House entrant = Assert.Single(market.Houses);
        Assert.True(entrant.EstimatedMarketValue > entrant.BaseValue);
        Assert.Equal(
            MathF.Round(entrant.EstimatedMarketValue * entrant.SellerPricingMultiplier, 2),
            entrant.AskingPrice);
    }

    [Fact]
    public void RejectedBelowAskingBidMovesPriceTowardOffer()
    {
        House house = TestHouseFactory.Create(100);
        _ = new Bid(CreateBuyer(), house, 98);
        SellerPricingService service = new(new SimulationSettings
        {
            RejectedBidAdjustmentRate = 0.5f,
            MaximumMonthlyMarketValueAdjustment = 0.03f
        });

        int direction = service.AdjustUnsuccessfulListing(house);

        Assert.Equal(-1, direction);
        Assert.Equal(99, house.AskingPrice);
    }

    private static House CreateComparableHouse(string name, HouseValuationService valuation) =>
        new(
            name,
            120,
            300,
            20,
            PropertyQuality.Standard,
            LocationDesirability.Average,
            valuation,
            new Random(1));

    private static Buyer CreateBuyer() =>
        new("Buyer", 35, 200, 5, 200, false);
}
