public class Simulation(
    Market market,
    DataGenerator? generator = null)
{
    private const float UnsoldPriceReductionRate = 0.02f;
    private const float HighDemandPriceIncreaseRate = 0.02f;
    private const float AdditionalPriceIncreasePerBid = 0.001f;
    private const int HighDemandBidThreshold = 3;
    private const int NewBuyersPerMonth = 1;
    private const int NewHousesPerMonth = 1;

    private readonly DataGenerator dataGenerator = generator ?? new DataGenerator();
    private readonly float startingAverageAskingPrice = CalculateAverageAskingPrice(market.Houses);

    public Market Market { get; } = market;
    public int CurrentMonth { get; private set; }

    public void RunTick()
    {
        CurrentMonth++;
        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");

        ClearMonthlyBiddingState();

        int buyersActiveThisMonth = Market.Buyers.Count;
        List<House> housesActiveThisMonth = [.. Market.Houses];

        UpdateBuyerFinances();
        CheckAffordableHouses();
        FindBestAffordableHouse();
        MakeBids();

        List<Transaction> completedTransactions = DeliberateBids();
        Market.LogTransactionDetails(completedTransactions);

        (int priceReductions, int priceIncreases) = AdjustPricesForRemainingHouses();
        float averageAskingPriceDuringMonth = CalculateAverageAskingPrice(housesActiveThisMonth);
        RecordMonthlyReport(
            buyersActiveThisMonth,
            housesActiveThisMonth.Count,
            completedTransactions,
            priceReductions,
            priceIncreases,
            averageAskingPriceDuringMonth);

        AddMonthlyEntrants();
    }

    private void ClearMonthlyBiddingState()
    {
        Market.Bids.Clear();

        foreach (House house in Market.Houses)
        {
            house.bids.Clear();
        }
    }

    private void UpdateBuyerFinances()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            float monthlySalary = buyer.Salary / 12f;
            buyer.Savings += monthlySalary * 0.20f;
        }
    }

    private void CheckAffordableHouses()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            buyer.AffordableHouses.Clear();

            foreach (House house in Market.Houses)
            {
                if (buyer.CanAfford(house))
                {
                    buyer.AffordableHouses.Add(house);
                }
            }
        }
    }

    public void FindBestAffordableHouse()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            buyer.WinningHouse = buyer.AffordableHouses
                .OrderByDescending(house => house.Quality)
                .FirstOrDefault();
        }
    }

    public void MakeBids()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            if (buyer.WinningHouse is not House house)
            {
                continue;
            }

            float startingOffer = house.AskingPrice;
            float motivationPremium = startingOffer * (buyer.Motivation / 100f);
            float offerAmount = MathF.Min(
                startingOffer + motivationPremium,
                buyer.CalculateMaximumPurchasePrice());

            Market.Bids.Add(new Bid(buyer, house, offerAmount));
        }
    }

    public List<Transaction> DeliberateBids()
    {
        List<Transaction> completedTransactions = [];
        HashSet<Buyer> successfulBuyers = [];

        foreach (House house in Market.Houses)
        {
            Transaction? transaction = house.DeliberateBids(dataGenerator.Random);
            if (transaction != null && successfulBuyers.Add(transaction.Buyer))
            {
                completedTransactions.Add(transaction);
            }
        }

        Market.Transactions.AddRange(completedTransactions);
        Market.RemoveSoldHousesAndBuyersFromMarket(completedTransactions);
        foreach (House house in Market.Houses)
        {
            dataGenerator.ValuationService.EstimateMarketValue(
                house,
                Market.Transactions);
        }

        return completedTransactions;
    }

    private (int PriceReductions, int PriceIncreases) AdjustPricesForRemainingHouses()
    {
        int priceReductions = 0;
        int priceIncreases = 0;

        foreach (House house in Market.Houses)
        {
            if (house.AskingPrice == 0)
            {
                continue;
            }

            if (house.bids.Count == 0)
            {
                house.AskingPrice = MathF.Round(
                    house.AskingPrice * (1f - UnsoldPriceReductionRate), 2);
                priceReductions++;
            }
            else if (house.bids.Count >= HighDemandBidThreshold)
            {
                int extraBids = house.bids.Count - HighDemandBidThreshold;
                float priceIncreaseRate = HighDemandPriceIncreaseRate
                    + extraBids * AdditionalPriceIncreasePerBid;
                house.AskingPrice = MathF.Round(
                    house.AskingPrice * (1f + priceIncreaseRate), 2);
                priceIncreases++;
            }
        }

        return (priceReductions, priceIncreases);
    }

    private void RecordMonthlyReport(
        int buyersActiveThisMonth,
        int housesActiveThisMonth,
        List<Transaction> completedTransactions,
        int priceReductions,
        int priceIncreases,
        float averageAskingPriceDuringMonth)
    {
        float averageSalePrice = completedTransactions.Count == 0
            ? 0
            : completedTransactions.Average(transaction => transaction.SalePrice);
        float askingPriceChange = averageAskingPriceDuringMonth - startingAverageAskingPrice;
        float askingPricePercentageChange = startingAverageAskingPrice == 0
            ? 0
            : askingPriceChange / startingAverageAskingPrice * 100f;

        MonthlyMarketReport report = new(
            CurrentMonth,
            buyersActiveThisMonth,
            housesActiveThisMonth,
            Market.Bids.Count,
            completedTransactions.Count,
            priceReductions,
            priceIncreases,
            Market.Buyers.Count,
            Market.Houses.Count,
            averageAskingPriceDuringMonth,
            averageSalePrice,
            startingAverageAskingPrice,
            askingPriceChange,
            askingPricePercentageChange);

        Market.MonthlyReports.Add(report);
        report.Print();
    }

    private void AddMonthlyEntrants()
    {
        dataGenerator.AddMonthlyEntrants(
            Market,
            NewBuyersPerMonth,
            NewHousesPerMonth);
    }

    private static float CalculateAverageAskingPrice(IEnumerable<House> houses)
    {
        List<House> pricedHouses = houses
            .Where(house => house.AskingPrice > 0)
            .ToList();

        return pricedHouses.Count == 0
            ? 0
            : pricedHouses.Average(house => house.AskingPrice);
    }
}
