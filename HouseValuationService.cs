public sealed class HouseValuationService(ValuationSettings? settings = null)
{
    public ValuationSettings Settings { get; } = settings ?? new ValuationSettings();

    public float CalculateBaseValue(House house)
    {
        ArgumentNullException.ThrowIfNull(house);
        house.ValidateCharacteristics();

        float landValue = house.PlotSizeSquareMetres
            * Settings.BaseLandPricePerSquareMetre
            * Settings.LocationMultipliers[house.Location];
        float replacementCost = house.FloorAreaSquareMetres
            * Settings.ConstructionCostPerSquareMetre
            * Settings.QualityMultipliers[house.BuildQuality];
        float depreciationRate = MathF.Min(
            house.AgeYears * Settings.AnnualDepreciationRate,
            Settings.MaximumDepreciationRate);
        float ageDepreciation = replacementCost * depreciationRate;
        return MathF.Round(MathF.Max(0, landValue + replacementCost - ageDepreciation), 2);
    }

    public float GenerateAskingPrice(float baseValue, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (baseValue < 0) throw new ArgumentOutOfRangeException(nameof(baseValue));

        float range = Settings.MaximumSellerPriceMultiplier - Settings.MinimumSellerPriceMultiplier;
        float multiplier = Settings.MinimumSellerPriceMultiplier + (float)random.NextDouble() * range;
        return MathF.Round(baseValue * multiplier, 2);
    }

    public float EstimateMarketValue(House house, IEnumerable<Transaction> completedTransactions)
    {
        ArgumentNullException.ThrowIfNull(completedTransactions);
        List<Transaction> comparables = completedTransactions
            .Where(transaction =>
                transaction.House != house
                && transaction.House.Location == house.Location
                && transaction.House.FloorAreaSquareMetres > 0)
            .ToList();

        if (comparables.Count == 0)
        {
            house.EstimatedMarketValue = house.BaseValue;
            return house.EstimatedMarketValue;
        }

        float averagePricePerSquareMetre = comparables.Average(
            transaction => transaction.SalePrice / transaction.House.FloorAreaSquareMetres);
        float comparableEstimate = averagePricePerSquareMetre * house.FloorAreaSquareMetres;
        float weight = MathF.Min(
            Settings.MaximumComparableWeight,
            comparables.Count / Settings.ComparablesPerFullWeight * Settings.MaximumComparableWeight);
        house.EstimatedMarketValue = MathF.Round(
            (1f - weight) * house.BaseValue + weight * comparableEstimate,
            2);
        return house.EstimatedMarketValue;
    }
}
