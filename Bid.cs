public class Bid
{
    public string houseId;
    public string buyerId;
    public float offerAmount;

    public Bid(Buyer buyer, House house, float offerAmount)
    {
        this.houseId = house.Name;
        this.buyerId = buyer.Name;
        this.offerAmount = offerAmount;

        MakeBid(house);
    }

    public void MakeBid(House house)
    {
        house.numberOfBids++;
        house.bids.Add(this);
    }
}