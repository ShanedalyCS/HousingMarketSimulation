public class Transaction
{
    public Transaction(
        Buyer buyer,
        House house,
        float salePrice,
        int month,
        float? listPrice = null,
        int? monthsOnMarket = null)
    {
        if (month < 1) throw new ArgumentOutOfRangeException(nameof(month));
        Buyer = buyer;
        House = house;
        SalePrice = salePrice;
        Month = month;
        ListPrice = listPrice ?? house.AskingPrice;
        MonthsOnMarket = monthsOnMarket ?? house.MonthsOnMarket;
    }

    public Buyer Buyer { get; set; }
    public House House { get; set; }
    public float SalePrice { get; set; }
    public int Month { get; }
    public float ListPrice { get; }
    public int MonthsOnMarket { get; }
}
