public class Simulation(
    Market market,
    DataGenerator? generator = null,
    bool usePriceDiscoveryMode = false)
{
    private const float UnsoldPriceReductionRate = 0.02f;
    private const float HighDemandPriceIncreaseRate = 0.02f;
    private const int HighDemandBidThreshold = 3;
    private const int NewBuyersPerMonth = 1;
    private const int NewHousesPerMonth = 1;

    private readonly DataGenerator dataGenerator = generator ?? new DataGenerator();
    private readonly bool priceDiscoveryMode = usePriceDiscoveryMode;
    private readonly float startingAverageAskingPrice = market.Houses.Count == 0
        ? 0
        : market.Houses.Average(house => house.Value);

    public Market Market { get; } = market;
    public int CurrentMonth { get; private set; } = 0;

    public void RunTick()
    {
        CurrentMonth++;
        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");
        
        ClearMonthlyBiddingState();

        UpdateBuyerFinances();
        CheckAffordableHouses();
        LogAffordableHouses();
        FindBestAffordableHouse();
        MakeAndLogBids();
        (int priceReductions, int priceIncreases) = AdjustHousePrices();
        float averageEstablishedAskingPrice = CalculateAverageEstablishedAskingPrice();
        List<Transaction> completedTransactions = DeliberateBids();
        Market.LogTransactionDetails(completedTransactions);
        AddMonthlyEntrants();
        RecordMonthlyReport(
            completedTransactions,
            priceReductions,
            priceIncreases,
            averageEstablishedAskingPrice);
    }

    private (int PriceReductions, int PriceIncreases) AdjustHousePrices()
    {
        int priceReductions = 0;
        int priceIncreases = 0;

        foreach (House house in Market.Houses)
        {
            if (house.bids.Count == 0 && house.Value > 0)
            {
                house.Value = MathF.Round(house.Value * (1f - UnsoldPriceReductionRate), 2);
                priceReductions++;
            }
            else if (house.bids.Count >= HighDemandBidThreshold)
            {
                if (priceDiscoveryMode && house.Value == 0)
                {
                    house.Value = MathF.Round(
                        house.bids.Average(bid => bid.offerAmount), 2);
                }
                else
                {
                    house.Value = MathF.Round(
                        house.Value * (1f + HighDemandPriceIncreaseRate), 2);
                }

                priceIncreases++;
            }
        }

        return (priceReductions, priceIncreases);
    }

    private float CalculateAverageEstablishedAskingPrice()
    {
        List<House> pricedHouses = Market.Houses
            .Where(house => house.Value > 0)
            .ToList();

        return pricedHouses.Count == 0
            ? 0
            : pricedHouses.Average(house => house.Value);
    }

    private void AddMonthlyEntrants()
    {
        dataGenerator.AddMonthlyEntrants(
            Market,
            NewBuyersPerMonth,
            NewHousesPerMonth,
            priceDiscoveryMode);
    }

    private void RecordMonthlyReport(
        List<Transaction> completedTransactions,
        int priceReductions,
        int priceIncreases,
        float averageAskingPrice)
    {
        float averageSalePrice = completedTransactions.Count == 0
            ? 0
            : completedTransactions.Average(transaction => transaction.SalePrice);
        float askingPriceChange = averageAskingPrice - startingAverageAskingPrice;
        float askingPricePercentageChange = startingAverageAskingPrice == 0
            ? 0
            : askingPriceChange / startingAverageAskingPrice * 100f;

        MonthlyMarketReport report = new(
            CurrentMonth,
            Market.Bids.Count,
            completedTransactions.Count,
            averageSalePrice,
            priceReductions,
            priceIncreases,
            NewBuyersPerMonth,
            NewHousesPerMonth,
            Market.Buyers.Count,
            Market.Houses.Count,
            averageAskingPrice,
            startingAverageAskingPrice,
            askingPriceChange,
            askingPricePercentageChange);

        Market.MonthlyReports.Add(report);
        report.Print();
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
            float amountSaved = monthlySalary * 0.20f;

            buyer.Savings += amountSaved;
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


    public void LogAffordableHouses()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            string lineOfAffordableHouses = "";
            if (buyer.AffordableHouses.Count > 0)
            {

                foreach (House affordableHouse in buyer.AffordableHouses)
                {
                    lineOfAffordableHouses += affordableHouse.Name + ", ";
                }
                Console.WriteLine(buyer.Name + " can afford " + lineOfAffordableHouses);
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

    public void MakeAndLogBids()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            if (buyer.winningHouse != null)
            {
                House house = buyer.winningHouse;
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
        Market.LogBidDetails();
    }

    public List<Transaction> DeliberateBids()
    {
        List<Transaction> completedTransactions = [];
        HashSet<Buyer> successfulBuyers = [];

        foreach (House house in Market.Houses)
        {
            if (priceDiscoveryMode
                && house.Value == 0
                && house.bids.Count < HighDemandBidThreshold)
            {
                if (house.bids.Count > 0)
                {
                    Console.WriteLine(
                        $"{house.Name} rejected all bids because price discovery requires " +
                        $"{HighDemandBidThreshold} bids");
                }

                continue;
            }

            Transaction? transaction = house.DeliberateBids();
            if (transaction != null && successfulBuyers.Add(transaction.Buyer))
            {
                completedTransactions.Add(transaction);
            }
        }

        Market.Transactions.AddRange(completedTransactions);
        Market.RemoveSoldHousesAndBuyersFromMarket(completedTransactions);

        return completedTransactions;
    }
}
