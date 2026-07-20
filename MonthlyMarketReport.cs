public class MonthlyMarketReport(
    int month,
    int bidsPlaced,
    int transactionsCompleted,
    float averageSalePrice,
    int unsoldPriceReductions,
    int highDemandPriceIncreases,
    int newBuyers,
    int newHouses,
    int activeBuyers,
    int availableHouses,
    float averageAskingPrice,
    float startingAverageAskingPrice,
    float askingPriceChange,
    float askingPricePercentageChange)
{
    public int Month { get; } = month;
    public int BidsPlaced { get; } = bidsPlaced;
    public int TransactionsCompleted { get; } = transactionsCompleted;
    public float AverageSalePrice { get; } = averageSalePrice;
    public int UnsoldPriceReductions { get; } = unsoldPriceReductions;
    public int HighDemandPriceIncreases { get; } = highDemandPriceIncreases;
    public int NewBuyers { get; } = newBuyers;
    public int NewHouses { get; } = newHouses;
    public int ActiveBuyers { get; } = activeBuyers;
    public int AvailableHouses { get; } = availableHouses;
    public float AverageAskingPrice { get; } = averageAskingPrice;
    public float StartingAverageAskingPrice { get; } = startingAverageAskingPrice;
    public float AskingPriceChange { get; } = askingPriceChange;
    public float AskingPricePercentageChange { get; } = askingPricePercentageChange;

    public void Print()
    {
        Console.WriteLine("----- MONTHLY MARKET REPORT -----");
        Console.WriteLine($"Bids placed: {BidsPlaced}");
        Console.WriteLine($"Transactions completed: {TransactionsCompleted}");
        Console.WriteLine($"Average sale price: {AverageSalePrice:F2} K");
        Console.WriteLine($"Unsold price reductions: {UnsoldPriceReductions}");
        Console.WriteLine($"High-demand price increases: {HighDemandPriceIncreases}");
        Console.WriteLine($"New buyers / houses: {NewBuyers} / {NewHouses}");
        Console.WriteLine($"Active buyers / available houses: {ActiveBuyers} / {AvailableHouses}");
        Console.WriteLine($"Average asking price: {AverageAskingPrice:F2} K");
        string direction = AskingPriceChange > 0 ? "up" : AskingPriceChange < 0 ? "down" : "unchanged";
        string percentageChange = StartingAverageAskingPrice == 0
            ? "percentage unavailable: starting average was 0"
            : $"{MathF.Abs(AskingPricePercentageChange):F2}%";
        Console.WriteLine(
            $"Average asking price since start: {direction} {MathF.Abs(AskingPriceChange):F2} K " +
            $"({percentageChange})");
        Console.WriteLine("---------------------------------");
    }
}
