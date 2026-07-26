public class HouseValuationTests
{
    private readonly HouseValuationService service = new();

    [Fact]
    public void BaseValueUsesCostApproachFormula()
    {
        House house = Create(floorArea: 120, plot: 300, age: 20);

        // Land: 300 × 0.35 = 105; replacement: 120 × 1.8 = 216;
        // depreciation: 216 × 20% = 43.2.
        Assert.Equal(277.8f, house.BaseValue);
    }

    [Fact]
    public void LargerHouseHasHigherBaseValue()
    {
        Assert.True(Create(floorArea: 160).BaseValue > Create(floorArea: 100).BaseValue);
    }

    [Fact]
    public void OlderHouseHasMoreDepreciation()
    {
        Assert.True(Create(age: 60).BaseValue < Create(age: 10).BaseValue);
    }

    [Fact]
    public void BetterQualityIncreasesValue()
    {
        House ordinary = Create(quality: PropertyQuality.Basic);
        House better = Create(quality: PropertyQuality.Premium);
        Assert.True(better.BaseValue > ordinary.BaseValue);
    }

    [Fact]
    public void LocationAffectsLandValue()
    {
        Assert.True(Create(location: LocationDesirability.Prime).BaseValue
            > Create(location: LocationDesirability.Low).BaseValue);
    }

    [Fact]
    public void AskingPriceIsReproducibleWithFixedSeed()
    {
        Assert.Equal(Create(random: new Random(42)).AskingPrice, Create(random: new Random(42)).AskingPrice);
    }

    [Fact]
    public void NoComparablesLeavesEstimateAtBaseValue()
    {
        House house = Create();
        Assert.Equal(house.BaseValue, service.EstimateMarketValue(house, []));
    }

    [Fact]
    public void RelevantComparableInfluencesEstimate()
    {
        House subject = Create();
        House comparable = Create();
        Transaction sale = new(new Buyer("Buyer", 30, 100, 5, 100, false), comparable, 500);
        float originalBaseValue = subject.BaseValue;

        Assert.NotEqual(subject.BaseValue, service.EstimateMarketValue(subject, [sale]));
        Assert.Equal(originalBaseValue, subject.BaseValue);
    }

    [Fact]
    public void UnrelatedLocationIsExcluded()
    {
        House subject = Create();
        House otherLocation = Create(location: LocationDesirability.Prime);
        Buyer buyer = new("Buyer", 30, 100, 5, 100, false);

        float estimate = service.EstimateMarketValue(
            subject,
            [new Transaction(buyer, otherLocation, 900)]);

        Assert.Equal(subject.BaseValue, estimate);
    }

    [Fact]
    public void NegativePhysicalDataIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(floorArea: -1));
    }

    private House Create(
        float floorArea = 120,
        float plot = 300,
        float age = 20,
        PropertyQuality quality = PropertyQuality.Standard,
        LocationDesirability location = LocationDesirability.Average,
        Random? random = null)
    {
        return new House(
            "Test", floorArea, plot, age, quality, location,
            service, random ?? new Random(1));
    }
}
