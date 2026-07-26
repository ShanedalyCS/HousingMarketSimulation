public class MonthlyMarketReport(
    int month,
    int buyersActiveDuringMonth,
    int housesActiveDuringMonth,
    int bidsPlaced,
    int transactionsCompleted,
    int priceReductions,
    int priceIncreases,
    int buyersRemaining,
    int housesRemaining,
    float averageAskingPriceDuringMonth,
    float averageSalePrice,
    float startingAverageAskingPrice,
    float askingPriceChange,
    float askingPricePercentageChange,
    float medianAskingPrice = 0,
    float medianSalePrice = 0,
    float averageSaleToListRatio = 0,
    float averageTimeOnMarketForSoldHouses = 0,
    float totalTransactionValue = 0)
{
    public int Month { get; } = month;
    public int BuyersActiveDuringMonth { get; } = buyersActiveDuringMonth;
    public int HousesActiveDuringMonth { get; } = housesActiveDuringMonth;
    public int BidsPlaced { get; } = bidsPlaced;
    public int TransactionsCompleted { get; } = transactionsCompleted;
    public int PriceReductions { get; } = priceReductions;
    public int PriceIncreases { get; } = priceIncreases;
    public int BuyersRemaining { get; } = buyersRemaining;
    public int HousesRemaining { get; } = housesRemaining;
    public float AverageAskingPriceDuringMonth { get; } = averageAskingPriceDuringMonth;
    public float AverageSalePrice { get; } = averageSalePrice;
    public float StartingAverageAskingPrice { get; } = startingAverageAskingPrice;
    public float AskingPriceChange { get; } = askingPriceChange;
    public float AskingPricePercentageChange { get; } = askingPricePercentageChange;
    public float MedianAskingPrice { get; } = medianAskingPrice;
    public float MedianSalePrice { get; } = medianSalePrice;
    public float AverageSaleToListRatio { get; } = averageSaleToListRatio;
    public float AverageTimeOnMarketForSoldHouses { get; } = averageTimeOnMarketForSoldHouses;
    public float TotalTransactionValue { get; } = totalTransactionValue;

    public void Print()
    {
        Console.WriteLine("----- MONTHLY MARKET REPORT -----");
        Console.WriteLine($"Buyers / houses active this month: {BuyersActiveDuringMonth} / {HousesActiveDuringMonth}");
        Console.WriteLine($"Bids / transactions: {BidsPlaced} / {TransactionsCompleted}");
        Console.WriteLine($"Price reductions / increases: {PriceReductions} / {PriceIncreases}");
        Console.WriteLine($"Buyers remaining / houses remaining: {BuyersRemaining} / {HousesRemaining}");
        Console.WriteLine($"Average / median asking price: {AverageAskingPriceDuringMonth:F2} / {MedianAskingPrice:F2} K");
        Console.WriteLine($"Average / median sale price: {AverageSalePrice:F2} / {MedianSalePrice:F2} K");
        Console.WriteLine($"Sale-to-list ratio: {AverageSaleToListRatio:P2}");
        Console.WriteLine($"Average time on market for sales: {AverageTimeOnMarketForSoldHouses:F2} months");
        Console.WriteLine($"Total transaction value: {TotalTransactionValue:F2} K");
        Console.WriteLine("---------------------------------");
    }
}
