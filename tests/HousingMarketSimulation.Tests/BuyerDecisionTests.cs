public class BuyerDecisionTests
{
    [Fact]
    public void BuyersWithDifferentPreferencesChooseDifferentHouses()
    {
        House primeCompact = TestHouseFactory.Create(
            200, "Prime", floorArea: 80, plotSize: 100,
            location: LocationDesirability.Prime);
        House largeRural = TestHouseFactory.Create(
            200, "Large", floorArea: 180, plotSize: 600,
            location: LocationDesirability.Low);
        Buyer locationBuyer = CreateBuyer(
            new BuyerPreferences(0.80f, 0.05f, 0.05f, 0.05f, 0.05f));
        Buyer spaceBuyer = CreateBuyer(
            new BuyerPreferences(0.05f, 0.05f, 0.50f, 0.35f, 0.05f));
        BuyerDecisionService service = new();

        House? firstChoice = service.ChooseHouse(
            locationBuyer, [primeCompact, largeRural], new Random(1))?.House;
        House? secondChoice = service.ChooseHouse(
            spaceBuyer, [primeCompact, largeRural], new Random(1))?.House;

        Assert.Same(primeCompact, firstChoice);
        Assert.Same(largeRural, secondChoice);
    }

    [Fact]
    public void WillingnessToPayNeverExceedsAffordability()
    {
        Buyer buyer = new(
            "Buyer", 30, 20, 10, 30, false,
            new BuyerPreferences(1, 1, 1, 1, 1));
        House house = TestHouseFactory.Create(100);

        BuyerHouseEvaluation evaluation = new BuyerDecisionService().Evaluate(buyer, house);

        Assert.Equal(110, buyer.CalculateMaximumPurchasePrice());
        Assert.True(evaluation.MaximumBid <= 110);
    }

    [Fact]
    public void BuyerDoesNotBidWhenValueIsTooFarBelowAsking()
    {
        House house = TestHouseFactory.Create(1000);
        Buyer buyer = CreateBuyer(BuyerPreferences.Balanced);
        BuyerDecisionService service = new(
            new SimulationSettings { BelowAskingBidTolerance = 0.05f });

        BuyerHouseEvaluation evaluation = service.Evaluate(buyer, house);

        Assert.True(evaluation.MaximumBid < 950);
        Assert.False(service.CanSubmitBid(evaluation));
        Assert.Null(service.ChooseHouse(buyer, [house], new Random(1)));
    }

    private static Buyer CreateBuyer(BuyerPreferences preferences) =>
        new("Buyer", 35, 250, 5, 250, false, preferences);
}
