public class Bid
{
    public House house;
    public Buyer buyer;
    public float offerAmount;

    public Bid(Buyer buyer, House house, float offerAmount)
    {
        this.house = house;
        this.buyer = buyer;
        this.offerAmount = offerAmount;

        MakeBid(house);
    }

    public void MakeBid(House house)
    {
        house.numberOfBids++;
        house.bids.Add(this);
    }
}