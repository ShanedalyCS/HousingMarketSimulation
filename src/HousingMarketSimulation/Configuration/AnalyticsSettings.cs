public sealed class AnalyticsSettings
{
    public int PriceIndexWindowMonths { get; init; } = 12;
    public int MinimumOverallTransactions { get; init; } = 5;
    public int MinimumLocationTransactions { get; init; } = 3;
    public float MinimumSaleToBaseRatio { get; init; } = 0.50f;
    public float MaximumSaleToBaseRatio { get; init; } = 2.00f;

    public void Validate()
    {
        if (PriceIndexWindowMonths < 1)
            throw new ArgumentOutOfRangeException(nameof(PriceIndexWindowMonths));
        if (MinimumOverallTransactions < 1)
            throw new ArgumentOutOfRangeException(nameof(MinimumOverallTransactions));
        if (MinimumLocationTransactions < 1)
            throw new ArgumentOutOfRangeException(nameof(MinimumLocationTransactions));
        ValidatePositiveFinite(MinimumSaleToBaseRatio, nameof(MinimumSaleToBaseRatio));
        ValidatePositiveFinite(MaximumSaleToBaseRatio, nameof(MaximumSaleToBaseRatio));
        if (MinimumSaleToBaseRatio > MaximumSaleToBaseRatio)
        {
            throw new ArgumentException(
                $"{nameof(MinimumSaleToBaseRatio)} cannot exceed {nameof(MaximumSaleToBaseRatio)}.",
                nameof(MinimumSaleToBaseRatio));
        }
    }

    private static void ValidatePositiveFinite(float value, string propertyName)
    {
        if (!float.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(propertyName);
    }
}
