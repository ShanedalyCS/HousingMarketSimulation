public class House
{
    public List<Bid> bids = [];

    public House(
        string name,
        float floorAreaSquareMetres,
        float plotSizeSquareMetres,
        float ageYears,
        PropertyQuality buildQuality,
        LocationDesirability location,
        HouseValuationService valuationService,
        Random random)
    {
        Name = name;
        FloorAreaSquareMetres = floorAreaSquareMetres;
        PlotSizeSquareMetres = plotSizeSquareMetres;
        AgeYears = ageYears;
        BuildQuality = buildQuality;
        Location = location;
        ValidateCharacteristics();

        BaseValue = valuationService.CalculateBaseValue(this);
        EstimatedMarketValue = BaseValue;
        AskingPrice = valuationService.GenerateAskingPrice(BaseValue, random);
    }

    public string Name { get; set; }
    public float BaseValue { get; private set; }
    public float AskingPrice { get; set; }
    public float EstimatedMarketValue { get; internal set; }
    public PropertyQuality BuildQuality { get; set; } = PropertyQuality.Standard;
    public LocationDesirability Location { get; set; } = LocationDesirability.Average;
    public float FloorAreaSquareMetres { get; set; }
    public float PlotSizeSquareMetres { get; set; }
    public float AgeYears { get; set; }

    public float Quality =>
        FloorAreaSquareMetres + PlotSizeSquareMetres / 4f + (int)BuildQuality * 10f;

    public void ValidateCharacteristics()
    {
        if (FloorAreaSquareMetres < 0) throw new ArgumentOutOfRangeException(nameof(FloorAreaSquareMetres));
        if (PlotSizeSquareMetres < 0) throw new ArgumentOutOfRangeException(nameof(PlotSizeSquareMetres));
        if (AgeYears < 0) throw new ArgumentOutOfRangeException(nameof(AgeYears));
    }

    public Transaction? DeliberateBids(Random random)
    {
        if (bids.Count == 0) return null;
        float highestOffer = bids.Max(bid => bid.offerAmount);
        if (highestOffer < AskingPrice)
        {
            Console.WriteLine($"{Name} rejected all bids and remains on the market at {AskingPrice:F2} K");
            return null;
        }

        List<Bid> highestBids = bids.Where(bid => bid.offerAmount == highestOffer).ToList();
        Bid winningBid = highestBids[random.Next(highestBids.Count)];
        return new Transaction(winningBid.buyer, this, winningBid.offerAmount);
    }

    public string PrintAll() => "name : " + Name;
}
