public class MonthlyMarketReport(
    int month,
    int buyersActiveDuringMonth,
    int housesActiveDuringMonth,
    int bidsPlaced,
    int transactionsCompleted,
    int priceReductions,
    int priceIncreases,
    int priceDiscoveries,
    int buyersRemaining,
    int housesRemaining,
    float averageAskingPriceDuringMonth,
    float averageSalePrice,
    float startingAverageAskingPrice,
    float askingPriceChange,
    float askingPricePercentageChange)
{
    public int Month { get; } = month;
    public int BuyersActiveDuringMonth { get; } = buyersActiveDuringMonth;
    public int HousesActiveDuringMonth { get; } = housesActiveDuringMonth;
    public int BidsPlaced { get; } = bidsPlaced;
    public int TransactionsCompleted { get; } = transactionsCompleted;
    public int PriceReductions { get; } = priceReductions;
    public int PriceIncreases { get; } = priceIncreases;
    public int PriceDiscoveries { get; } = priceDiscoveries;
    public int BuyersRemaining { get; } = buyersRemaining;
    public int HousesRemaining { get; } = housesRemaining;
    public float AverageAskingPriceDuringMonth { get; } = averageAskingPriceDuringMonth;
    public float AverageSalePrice { get; } = averageSalePrice;
    public float StartingAverageAskingPrice { get; } = startingAverageAskingPrice;
    public float AskingPriceChange { get; } = askingPriceChange;
    public float AskingPricePercentageChange { get; } = askingPricePercentageChange;

    public void Print()
    {
        Console.WriteLine("----- MONTHLY MARKET REPORT -----");
        Console.WriteLine($"Buyers / houses active this month: {BuyersActiveDuringMonth} / {HousesActiveDuringMonth}");
        Console.WriteLine($"Bids placed: {BidsPlaced}");
        Console.WriteLine($"Transactions completed: {TransactionsCompleted}");
        Console.WriteLine($"Price reductions / increases / discoveries: {PriceReductions} / {PriceIncreases} / {PriceDiscoveries}");
        Console.WriteLine($"Buyers / houses remaining: {BuyersRemaining} / {HousesRemaining}");
        Console.WriteLine($"Average asking price of houses active this month: {AverageAskingPriceDuringMonth:F2} K");
        Console.WriteLine($"Average sale price: {AverageSalePrice:F2} K");

        string direction = AskingPriceChange > 0
            ? "up"
            : AskingPriceChange < 0 ? "down" : "unchanged";
        string percentageChange = StartingAverageAskingPrice == 0
            ? "percentage unavailable: starting average was 0"
            : $"{MathF.Abs(AskingPricePercentageChange):F2}%";
        Console.WriteLine(
            $"Average asking price since start: {direction} {MathF.Abs(AskingPriceChange):F2} K " +
            $"({percentageChange})");
        Console.WriteLine("---------------------------------");
    }
}
