public class Bid
{
    public Bid(Buyer buyer, House house, float offerAmount)
    {
        Buyer = buyer;
        House = house;
        OfferAmount = MathF.Round(offerAmount, 2);
        house.Bids.Add(this);
    }

    public House House { get; }
    public Buyer Buyer { get; }
    public float OfferAmount { get; }
}
