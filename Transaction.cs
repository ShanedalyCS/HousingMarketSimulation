public class Transaction
{
    public Transaction(
        Buyer buyer,
        House house,
        float salePrice,
        float? listPrice = null,
        int? monthsOnMarket = null)
    {
        Buyer = buyer;
        House = house;
        SalePrice = salePrice;
        ListPrice = listPrice ?? house.AskingPrice;
        MonthsOnMarket = monthsOnMarket ?? house.MonthsOnMarket;
    }

    public Buyer Buyer { get; set; }
    public House House { get; set; }
    public float SalePrice { get; set; }
    public float ListPrice { get; }
    public int MonthsOnMarket { get; }
}
