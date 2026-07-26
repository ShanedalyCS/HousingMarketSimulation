public class House
{
    public House(
        string name,
        float floorAreaSquareMetres,
        float plotSizeSquareMetres,
        float ageYears,
        PropertyQuality buildQuality,
        LocationDesirability location,
        HouseValuationService valuationService,
        Random random,
        IEnumerable<Transaction>? completedTransactions = null)
    {
        Name = name;
        FloorAreaSquareMetres = floorAreaSquareMetres;
        PlotSizeSquareMetres = plotSizeSquareMetres;
        AgeYears = ageYears;
        BuildQuality = buildQuality;
        Location = location;
        ValidateCharacteristics();

        BaseValue = valuationService.CalculateBaseValue(this);
        SellerPricingMultiplier = valuationService.GenerateSellerPricingMultiplier(random);
        EstimatedMarketValue = BaseValue;
        valuationService.EstimateMarketValue(this, completedTransactions ?? []);
        AskingPrice = MathF.Round(EstimatedMarketValue * SellerPricingMultiplier, 2);
    }

    public string Name { get; set; }
    public float BaseValue { get; private set; }
    public float AskingPrice { get; set; }
    public float EstimatedMarketValue { get; internal set; }
    public float SellerPricingMultiplier { get; }
    public int MonthsOnMarket { get; internal set; }
    public PropertyQuality BuildQuality { get; set; } = PropertyQuality.Standard;
    public LocationDesirability Location { get; set; } = LocationDesirability.Average;
    public float FloorAreaSquareMetres { get; set; }
    public float PlotSizeSquareMetres { get; set; }
    public float AgeYears { get; set; }
    public List<Bid> Bids { get; } = [];

    public float Quality =>
        FloorAreaSquareMetres + PlotSizeSquareMetres / 4f + (int)BuildQuality * 10f;

    public void ValidateCharacteristics()
    {
        if (FloorAreaSquareMetres < 0) throw new ArgumentOutOfRangeException(nameof(FloorAreaSquareMetres));
        if (PlotSizeSquareMetres < 0) throw new ArgumentOutOfRangeException(nameof(PlotSizeSquareMetres));
        if (AgeYears < 0) throw new ArgumentOutOfRangeException(nameof(AgeYears));
    }

    public Transaction? DeliberateBids(Random random) =>
        new AuctionService().Settle(this, random);

    public string PrintAll() => "name : " + Name;
}
