public sealed record PriceIndexResult(
    float? OverallIndex,
    float? OverallMarketFactor,
    int OverallTransactionCount,
    IReadOnlyDictionary<LocationDesirability, float?> LocationIndices,
    IReadOnlyDictionary<LocationDesirability, int> LocationTransactionCounts);

public sealed class PriceIndexCalculator
{
    private readonly AnalyticsSettings settings;
    private float? overallBaselineFactor;
    private readonly Dictionary<LocationDesirability, float> locationBaselineFactors = [];

    public PriceIndexCalculator(AnalyticsSettings? settings = null)
    {
        this.settings = settings ?? new AnalyticsSettings();
        this.settings.Validate();
    }

    public PriceIndexResult Calculate(int month, IEnumerable<Transaction> transactions)
    {
        if (month < 1) throw new ArgumentOutOfRangeException(nameof(month));
        ArgumentNullException.ThrowIfNull(transactions);

        List<Transaction> window = transactions
            .Where(transaction =>
                transaction.Month <= month
                && transaction.Month >= Math.Max(1, month - settings.PriceIndexWindowMonths + 1))
            .ToList();

        List<float> overallRatios = ValidRatios(window).ToList();
        float? overallFactor = overallRatios.Count >= settings.MinimumOverallTransactions
            ? Median(overallRatios)
            : null;
        if (overallBaselineFactor is null && overallFactor.HasValue)
            overallBaselineFactor = overallFactor.Value;
        float? overallIndex = CreateIndex(overallFactor, overallBaselineFactor);

        Dictionary<LocationDesirability, float?> locationIndices = [];
        Dictionary<LocationDesirability, int> locationCounts = [];
        foreach (LocationDesirability location in Enum.GetValues<LocationDesirability>())
        {
            List<float> ratios = ValidRatios(
                window.Where(transaction => transaction.House.Location == location))
                .ToList();
            locationCounts[location] = ratios.Count;
            float? factor = ratios.Count >= settings.MinimumLocationTransactions
                ? Median(ratios)
                : null;
            if (!locationBaselineFactors.ContainsKey(location) && factor.HasValue)
                locationBaselineFactors[location] = factor.Value;
            locationIndices[location] = CreateIndex(
                factor,
                locationBaselineFactors.TryGetValue(location, out float baseline)
                    ? baseline
                    : null);
        }

        return new PriceIndexResult(
            overallIndex,
            overallFactor,
            overallRatios.Count,
            locationIndices,
            locationCounts);
    }

    public float? CalculateMedianMarketFactor(IEnumerable<Transaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        List<float> ratios = ValidRatios(transactions).ToList();
        return ratios.Count == 0 ? null : Median(ratios);
    }

    private IEnumerable<float> ValidRatios(IEnumerable<Transaction> transactions)
    {
        foreach (Transaction transaction in transactions)
        {
            float baseValue = transaction.House.BaseValue;
            if (!float.IsFinite(baseValue) || baseValue <= 0
                || !float.IsFinite(transaction.SalePrice)
                || transaction.SalePrice < 0)
            {
                continue;
            }

            float ratio = transaction.SalePrice / baseValue;
            if (!float.IsFinite(ratio) || ratio < 0) continue;
            yield return Math.Clamp(
                ratio,
                settings.MinimumSaleToBaseRatio,
                settings.MaximumSaleToBaseRatio);
        }
    }

    private static float? CreateIndex(float? factor, float? baseline)
    {
        if (!factor.HasValue || !baseline.HasValue || baseline.Value <= 0) return null;
        float index = 100f * factor.Value / baseline.Value;
        return float.IsFinite(index) ? MathF.Round(index, 4) : null;
    }

    internal static float Median(IEnumerable<float> values)
    {
        float[] sorted = values.Order().ToArray();
        if (sorted.Length == 0)
            throw new ArgumentException("At least one value is required.", nameof(values));
        int middle = sorted.Length / 2;
        return sorted.Length % 2 == 1
            ? sorted[middle]
            : (sorted[middle - 1] + sorted[middle]) / 2f;
    }
}
