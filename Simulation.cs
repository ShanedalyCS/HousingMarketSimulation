public class Simulation
{
    private readonly DataGenerator dataGenerator;
    private readonly SimulationSettings settings;
    private readonly BuyerDecisionService buyerDecisionService;
    private readonly AuctionService auctionService;
    private readonly SellerPricingService sellerPricingService;
    private readonly float startingAverageAskingPrice;

    public Simulation(
        Market market,
        DataGenerator? generator = null,
        SimulationSettings? settings = null)
    {
        Market = market;
        dataGenerator = generator ?? new DataGenerator();
        this.settings = settings ?? new SimulationSettings();
        this.settings.Validate();
        buyerDecisionService = new BuyerDecisionService(this.settings);
        auctionService = new AuctionService(this.settings);
        sellerPricingService = new SellerPricingService(this.settings);
        startingAverageAskingPrice = CalculateAverage(market.Houses.Select(house => house.AskingPrice));
    }

    public Market Market { get; }
    public int CurrentMonth { get; private set; }

    public void RunTick()
    {
        CurrentMonth++;
        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");

        ClearMonthlyState();
        int buyersActiveThisMonth = Market.Buyers.Count;
        List<House> housesActiveThisMonth = [.. Market.Houses];

        UpdateBuyerFinances();
        IncrementTimeOnMarket();
        RecalculateEstimatedMarketValues();
        (int priceReductions, int priceIncreases) = ApplyMarketInformedPricing();

        float[] askingPricesAtEvaluation = housesActiveThisMonth
            .Where(house => house.AskingPrice > 0)
            .Select(house => house.AskingPrice)
            .ToArray();
        FindBestAffordableHouse();
        MakeBids();
        List<Transaction> completedTransactions = DeliberateBids();
        Market.LogTransactionDetails(completedTransactions);

        (int unsuccessfulReductions, int unsuccessfulIncreases) =
            AdjustPricesForRemainingHouses();
        priceReductions += unsuccessfulReductions;
        priceIncreases += unsuccessfulIncreases;

        RecordMonthlyReport(
            buyersActiveThisMonth,
            housesActiveThisMonth.Count,
            completedTransactions,
            priceReductions,
            priceIncreases,
            askingPricesAtEvaluation);
        AddMonthlyEntrants();
    }

    private void ClearMonthlyState()
    {
        Market.Bids.Clear();
        foreach (House house in Market.Houses) house.Bids.Clear();
        foreach (Buyer buyer in Market.Buyers)
        {
            buyer.AffordableHouses.Clear();
            buyer.WinningHouse = null;
            buyer.SelectedEvaluation = null;
        }
    }

    private void UpdateBuyerFinances()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            buyer.Savings += buyer.Salary / 12f * settings.MonthlySavingsRate;
        }
    }

    private void IncrementTimeOnMarket()
    {
        foreach (House house in Market.Houses) house.MonthsOnMarket++;
    }

    private void RecalculateEstimatedMarketValues()
    {
        foreach (House house in Market.Houses)
        {
            dataGenerator.ValuationService.EstimateMarketValue(house, Market.Transactions);
        }
    }

    private (int PriceReductions, int PriceIncreases) ApplyMarketInformedPricing()
    {
        int reductions = 0;
        int increases = 0;
        foreach (House house in Market.Houses)
        {
            CountDirection(
                sellerPricingService.MoveTowardMarketTarget(house),
                ref reductions,
                ref increases);
        }
        return (reductions, increases);
    }

    public void FindBestAffordableHouse()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            BuyerHouseEvaluation? choice = buyerDecisionService.ChooseHouse(
                buyer,
                Market.Houses,
                dataGenerator.Random);
            buyer.SelectedEvaluation = choice;
            buyer.WinningHouse = choice?.House;

            // This list remains available for existing callers, but now means
            // houses inside the buyer's credible bidding range.
            buyer.AffordableHouses.Clear();
            buyer.AffordableHouses.AddRange(Market.Houses.Where(house =>
                buyerDecisionService.CanSubmitBid(buyerDecisionService.Evaluate(buyer, house))));
        }
    }

    public void MakeBids()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            if (buyer.SelectedEvaluation is not BuyerHouseEvaluation evaluation) continue;
            Market.Bids.Add(new Bid(buyer, evaluation.House, evaluation.MaximumBid));
        }
    }

    public List<Transaction> DeliberateBids()
    {
        List<Transaction> completedTransactions = [];
        HashSet<Buyer> successfulBuyers = [];
        foreach (House house in Market.Houses)
        {
            Transaction? transaction = auctionService.Settle(
                house,
                dataGenerator.Random,
                successfulBuyers);
            if (transaction is not null) completedTransactions.Add(transaction);
        }

        Market.Transactions.AddRange(completedTransactions);
        Market.RemoveSoldHousesAndBuyersFromMarket(completedTransactions);
        return completedTransactions;
    }

    private (int PriceReductions, int PriceIncreases) AdjustPricesForRemainingHouses()
    {
        int reductions = 0;
        int increases = 0;
        foreach (House house in Market.Houses)
        {
            CountDirection(
                sellerPricingService.AdjustUnsuccessfulListing(house),
                ref reductions,
                ref increases);
        }
        return (reductions, increases);
    }

    private void RecordMonthlyReport(
        int buyersActiveThisMonth,
        int housesActiveThisMonth,
        List<Transaction> completedTransactions,
        int priceReductions,
        int priceIncreases,
        IReadOnlyCollection<float> askingPricesAtEvaluation)
    {
        float averageAskingPrice = CalculateAverage(askingPricesAtEvaluation);
        float averageSalePrice = CalculateAverage(
            completedTransactions.Select(transaction => transaction.SalePrice));
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
            Market.Buyers.Count,
            Market.Houses.Count,
            averageAskingPrice,
            averageSalePrice,
            startingAverageAskingPrice,
            askingPriceChange,
            askingPricePercentageChange,
            CalculateMedian(askingPricesAtEvaluation),
            CalculateMedian(completedTransactions.Select(transaction => transaction.SalePrice)),
            CalculateAverage(completedTransactions
                .Where(transaction => transaction.ListPrice > 0)
                .Select(transaction => transaction.SalePrice / transaction.ListPrice)),
            CalculateAverage(completedTransactions.Select(
                transaction => (float)transaction.MonthsOnMarket)),
            completedTransactions.Sum(transaction => transaction.SalePrice),
            Market.Houses.Count);

        Market.MonthlyReports.Add(report);
        report.Print();
    }

    private void AddMonthlyEntrants()
    {
        dataGenerator.AddMonthlyEntrants(
            Market,
            settings.NewBuyersPerMonth,
            settings.NewHousesPerMonth);
    }

    private static void CountDirection(int direction, ref int reductions, ref int increases)
    {
        if (direction < 0) reductions++;
        if (direction > 0) increases++;
    }

    private static float CalculateAverage(IEnumerable<float> values)
    {
        float[] materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static float CalculateMedian(IEnumerable<float> values)
    {
        float[] sorted = values.Order().ToArray();
        if (sorted.Length == 0) return 0;
        int middle = sorted.Length / 2;
        return sorted.Length % 2 == 1
            ? sorted[middle]
            : (sorted[middle - 1] + sorted[middle]) / 2f;
    }
}
