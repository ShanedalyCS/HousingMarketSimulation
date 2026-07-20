public class Transaction(Buyer buyer, House house, float salePrice)
{
    public Buyer Buyer { get; set; } = buyer;
    public House House { get; set; } = house;
    public float SalePrice { get; set; } = salePrice;
}