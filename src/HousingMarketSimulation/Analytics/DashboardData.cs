public sealed record DashboardData(
    string ProjectTitle,
    IReadOnlyList<ScenarioDashboardData> Scenarios);

public sealed record ScenarioDashboardData(
    string Id,
    string Name,
    int? Seed,
    int DurationMonths,
    int InitialBuyers,
    int InitialHouses,
    int NewBuyersPerMonth,
    int NewHousesPerMonth,
    ScenarioSummary Summary,
    IReadOnlyList<ScenarioLocationAnalytics> LocationAnalytics,
    IReadOnlyList<MonthlyAnalyticsSnapshot> MonthlyAnalytics);

public sealed record ScenarioLocationAnalytics(
    LocationDesirability Location,
    float? ConstantQualityPriceIndex,
    int Transactions,
    float? MedianSalePrice,
    float? AverageTimeOnMarket,
    float? AverageSaleToListRatio);

public static class DashboardDataFactory
{
    public static ScenarioDashboardData CreateScenario(
        string id,
        string name,
        int? seed,
        int durationMonths,
        int initialBuyers,
        int initialHouses,
        SimulationSettings settings,
        ScenarioSummary summary,
        Market market)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(market);

        List<ScenarioLocationAnalytics> locations = [];
        foreach (LocationDesirability location in Enum.GetValues<LocationDesirability>())
        {
            Transaction[] transactions = market.Transactions
                .Where(transaction => transaction.House.Location == location)
                .ToArray();
            float? latestIndex = market.AnalyticsSnapshots
                .Select(snapshot => snapshot.LocationAnalytics
                    .Single(item => item.Location == location)
                    .ConstantQualityPriceIndex)
                .LastOrDefault(value => value.HasValue);
            locations.Add(new ScenarioLocationAnalytics(
                location,
                latestIndex,
                transactions.Length,
                MedianOrNull(transactions.Select(transaction => transaction.SalePrice)),
                AverageOrNull(transactions.Select(
                    transaction => (float)transaction.MonthsOnMarket)),
                AverageOrNull(transactions
                    .Where(transaction => transaction.ListPrice > 0)
                    .Select(transaction => transaction.SalePrice / transaction.ListPrice))));
        }

        return new ScenarioDashboardData(
            id,
            name,
            seed,
            durationMonths,
            initialBuyers,
            initialHouses,
            settings.NewBuyersPerMonth,
            settings.NewHousesPerMonth,
            summary,
            locations,
            [.. market.AnalyticsSnapshots]);
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
}
