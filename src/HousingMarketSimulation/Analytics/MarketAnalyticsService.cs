public sealed class MarketAnalyticsService
{
    private readonly BuyerDecisionService buyerDecisionService;
    private readonly PriceIndexCalculator priceIndexCalculator;

    public MarketAnalyticsService(
        AnalyticsSettings? analyticsSettings = null,
        SimulationSettings? simulationSettings = null)
    {
        AnalyticsSettings settings = analyticsSettings ?? new AnalyticsSettings();
        settings.Validate();
        SimulationSettings decisions = simulationSettings ?? new SimulationSettings();
        decisions.Validate();
        buyerDecisionService = new BuyerDecisionService(decisions);
        priceIndexCalculator = new PriceIndexCalculator(settings);
    }

    public AffordabilityObservation CaptureAffordability(
        IReadOnlyCollection<Buyer> activeBuyers,
        IReadOnlyCollection<House> activeListings)
    {
        ArgumentNullException.ThrowIfNull(activeBuyers);
        ArgumentNullException.ThrowIfNull(activeListings);

        float? medianPurchasingPower = MedianOrNull(
            activeBuyers.Select(buyer => buyer.CalculateMaximumPurchasePrice()));
        float? medianAskingPrice = MedianOrNull(
            activeListings.Where(house => house.AskingPrice >= 0)
                .Select(house => house.AskingPrice));
        float? askingToPower = medianPurchasingPower is > 0 && medianAskingPrice.HasValue
            ? SafeRatio(medianAskingPrice.Value, medianPurchasingPower.Value)
            : null;
        int capableBuyers = activeBuyers.Count(buyer =>
            activeListings.Any(house =>
                buyerDecisionService.CanSubmitBid(
                    buyerDecisionService.Evaluate(buyer, house))));
        float? capablePercentage = activeBuyers.Count == 0
            ? null
            : SafeRatio(capableBuyers, activeBuyers.Count);

        return new AffordabilityObservation(
            medianPurchasingPower,
            askingToPower,
            capablePercentage);
    }

    public MonthlyAnalyticsSnapshot CreateSnapshot(
        int month,
        int newListings,
        IReadOnlyCollection<Buyer> activeBuyers,
        IReadOnlyCollection<House> activeListings,
        int bidsSubmitted,
        IReadOnlyCollection<Transaction> completedTransactions,
        IReadOnlyCollection<House> endingInventory,
        IReadOnlyCollection<Transaction> allTransactions,
        AffordabilityObservation affordability)
    {
        if (month < 1) throw new ArgumentOutOfRangeException(nameof(month));
        if (newListings < 0) throw new ArgumentOutOfRangeException(nameof(newListings));
        if (bidsSubmitted < 0) throw new ArgumentOutOfRangeException(nameof(bidsSubmitted));
        ArgumentNullException.ThrowIfNull(activeBuyers);
        ArgumentNullException.ThrowIfNull(activeListings);
        ArgumentNullException.ThrowIfNull(completedTransactions);
        ArgumentNullException.ThrowIfNull(endingInventory);
        ArgumentNullException.ThrowIfNull(allTransactions);
        ArgumentNullException.ThrowIfNull(affordability);

        PriceIndexResult priceIndex = priceIndexCalculator.Calculate(month, allTransactions);
        List<LocationAnalytics> locations = [];
        foreach (LocationDesirability location in Enum.GetValues<LocationDesirability>())
        {
            Transaction[] locationSales = completedTransactions
                .Where(transaction => transaction.House.Location == location)
                .ToArray();
            locations.Add(new LocationAnalytics(
                location,
                priceIndex.LocationIndices[location],
                priceIndex.LocationTransactionCounts[location],
                locationSales.Length,
                MedianOrNull(locationSales.Select(transaction => transaction.SalePrice)),
                AverageOrNull(locationSales.Select(
                    transaction => (float)transaction.MonthsOnMarket)),
                AverageOrNull(locationSales
                    .Where(transaction => transaction.ListPrice > 0)
                    .Select(transaction => transaction.SalePrice / transaction.ListPrice))));
        }

        int activeBuyerCount = activeBuyers.Count;
        int activeListingCount = activeListings.Count;
        int transactionCount = completedTransactions.Count;
        float? clearanceRate = activeListingCount == 0
            ? null
            : SafeRatio(transactionCount, activeListingCount);

        return new MonthlyAnalyticsSnapshot(
            month,
            newListings,
            AverageOrNull(endingInventory.Select(house => house.AskingPrice)),
            MedianOrNull(endingInventory.Select(house => house.AskingPrice)),
            AverageOrNull(completedTransactions.Select(transaction => transaction.SalePrice)),
            MedianOrNull(completedTransactions.Select(transaction => transaction.SalePrice)),
            priceIndex.OverallIndex,
            locations,
            AverageOrNull(completedTransactions
                .Where(transaction => transaction.ListPrice > 0)
                .Select(transaction => transaction.SalePrice / transaction.ListPrice)),
            activeBuyerCount,
            activeListingCount,
            activeListingCount == 0 ? null : SafeRatio(activeBuyerCount, activeListingCount),
            bidsSubmitted,
            activeListingCount == 0 ? null : SafeRatio(bidsSubmitted, activeListingCount),
            transactionCount,
            clearanceRate,
            endingInventory.Count,
            transactionCount == 0 ? null : SafeRatio(endingInventory.Count, transactionCount),
            AverageOrNull(completedTransactions.Select(
                transaction => (float)transaction.MonthsOnMarket)),
            MedianOrNull(completedTransactions.Select(
                transaction => (float)transaction.MonthsOnMarket)),
            clearanceRate,
            activeBuyerCount == 0 ? null : SafeRatio(transactionCount, activeBuyerCount),
            completedTransactions.Sum(transaction => transaction.SalePrice),
            affordability.MedianBuyerMaximumPurchasePrice,
            affordability.AskingPriceToPurchasingPowerRatio,
            affordability.PercentageBuyersCapableOfBidding);
    }

    private static float? AverageOrNull(IEnumerable<float> values)
    {
        float[] valid = values.Where(float.IsFinite).ToArray();
        return valid.Length == 0 ? null : valid.Average();
    }

    private static float? MedianOrNull(IEnumerable<float> values)
    {
        float[] valid = values.Where(float.IsFinite).Order().ToArray();
        if (valid.Length == 0) return null;
        int middle = valid.Length / 2;
        return valid.Length % 2 == 1
            ? valid[middle]
            : (valid[middle - 1] + valid[middle]) / 2f;
    }

    private static float? SafeRatio(float numerator, float denominator)
    {
        if (!float.IsFinite(numerator) || !float.IsFinite(denominator) || denominator == 0)
            return null;
        float ratio = numerator / denominator;
        return float.IsFinite(ratio) ? ratio : null;
    }
}
