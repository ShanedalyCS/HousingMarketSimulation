public class Simulation(
    Market market,
    DataGenerator? generator = null,
    bool usePriceDiscoveryMode = false)
{
    private const float UnsoldPriceReductionRate = 0.02f;
    private const float HighDemandPriceIncreaseRate = 0.02f;
    private const float AdditionalPriceIncreasePerBid = 0.001f;
    private const int HighDemandBidThreshold = 3;
    private const int NewBuyersPerMonth = 1;
    private const int NewHousesPerMonth = 1;

    private readonly DataGenerator dataGenerator = generator ?? new DataGenerator();
    private readonly bool priceDiscoveryMode = usePriceDiscoveryMode;
    private readonly float startingAverageAskingPrice = CalculateAverageAskingPrice(market.Houses);

    public Market Market { get; } = market;
    public int CurrentMonth { get; private set; }

    public void RunTick()
    {
        CurrentMonth++;
        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");

        ClearMonthlyBiddingState();

        int buyersActiveThisMonth = Market.Buyers.Count;
        int housesActiveThisMonth = Market.Houses.Count;

        UpdateBuyerFinances();
        CheckAffordableHouses();
        FindBestAffordableHouse();
        MakeBids();

        int priceDiscoveries = DiscoverZeroPrices();
        List<Transaction> completedTransactions = DeliberateBids();
        Market.LogTransactionDetails(completedTransactions);

        (int priceReductions, int priceIncreases) = AdjustPricesForRemainingHouses();
        RecordMonthlyReport(
            buyersActiveThisMonth,
            housesActiveThisMonth,
            completedTransactions,
            priceReductions,
            priceIncreases,
            priceDiscoveries);

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

            float startingOffer = priceDiscoveryMode && house.Value == 0
                ? house.CalculateQualityBasedValue()
                : house.Value;
            float motivationPremium = startingOffer * (buyer.Motivation / 100f);
            float offerAmount = MathF.Min(
                startingOffer + motivationPremium,
                buyer.CalculateMaximumPurchasePrice());

            Market.Bids.Add(new Bid(buyer, house, offerAmount));
        }
    }

    private int DiscoverZeroPrices()
    {
        if (!priceDiscoveryMode)
        {
            return 0;
        }

        int priceDiscoveries = 0;

        foreach (House house in Market.Houses.Where(house => house.Value == 0))
        {
            if (house.bids.Count >= HighDemandBidThreshold)
            {
                house.Value = MathF.Round(
                    house.bids.Average(bid => bid.offerAmount), 2);
                priceDiscoveries++;
            }
        }

        return priceDiscoveries;
    }

    public List<Transaction> DeliberateBids()
    {
        List<Transaction> completedTransactions = [];
        HashSet<Buyer> successfulBuyers = [];

        foreach (House house in Market.Houses)
        {
            if (priceDiscoveryMode && house.Value == 0)
            {
                if (house.bids.Count > 0)
                {
                    Console.WriteLine(
                        $"{house.Name} rejected all bids because price discovery requires " +
                        $"{HighDemandBidThreshold} bids");
                }

                continue;
            }

            Transaction? transaction = house.DeliberateBids(dataGenerator.Random);
            if (transaction != null && successfulBuyers.Add(transaction.Buyer))
            {
                completedTransactions.Add(transaction);
            }
        }

        Market.Transactions.AddRange(completedTransactions);
        Market.RemoveSoldHousesAndBuyersFromMarket(completedTransactions);

        return completedTransactions;
    }

    private (int PriceReductions, int PriceIncreases) AdjustPricesForRemainingHouses()
    {
        int priceReductions = 0;
        int priceIncreases = 0;

        foreach (House house in Market.Houses)
        {
            if (house.Value == 0)
            {
                continue;
            }

            if (house.bids.Count == 0)
            {
                house.Value = MathF.Round(
                    house.Value * (1f - UnsoldPriceReductionRate), 2);
                priceReductions++;
            }
            else if (house.bids.Count >= HighDemandBidThreshold)
            {
                int extraBids = house.bids.Count - HighDemandBidThreshold;
                float priceIncreaseRate = HighDemandPriceIncreaseRate
                    + extraBids * AdditionalPriceIncreasePerBid;
                house.Value = MathF.Round(
                    house.Value * (1f + priceIncreaseRate), 2);
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
        int priceDiscoveries)
    {
        float averageSalePrice = completedTransactions.Count == 0
            ? 0
            : completedTransactions.Average(transaction => transaction.SalePrice);
        float averageAskingPrice = CalculateAverageAskingPrice(Market.Houses);
        float askingPriceChange = averageAskingPrice - startingAverageAskingPrice;
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
            priceDiscoveries,
            Market.Buyers.Count,
            Market.Houses.Count,
            averageAskingPrice,
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
            NewHousesPerMonth,
            priceDiscoveryMode);
    }

    private static float CalculateAverageAskingPrice(IEnumerable<House> houses)
    {
        List<House> pricedHouses = houses
            .Where(house => house.Value > 0)
            .ToList();

        return pricedHouses.Count == 0
            ? 0
            : pricedHouses.Average(house => house.Value);
    }
}
