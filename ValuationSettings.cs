public sealed class ValuationSettings
{
    public float ConstructionCostPerSquareMetre { get; init; } = 1.8f;
    public float BaseLandPricePerSquareMetre { get; init; } = 0.35f;
    public float AnnualDepreciationRate { get; init; } = 0.01f;
    public float MaximumDepreciationRate { get; init; } = 0.60f;
    public float ComparablesPerFullWeight { get; init; } = 5f;
    public float MaximumComparableWeight { get; init; } = 0.70f;
    public float MinimumSellerPriceMultiplier { get; init; } = 0.95f;
    public float MaximumSellerPriceMultiplier { get; init; } = 1.15f;
    public float MinimumComparableSimilarity { get; init; } = 0.55f;
    public float LocationSimilarityWeight { get; init; } = 0.35f;
    public float FloorAreaSimilarityWeight { get; init; } = 0.30f;
    public float BuildQualitySimilarityWeight { get; init; } = 0.20f;
    public float HouseAgeSimilarityWeight { get; init; } = 0.15f;
    public float MaximumFloorAreaDifferenceRatio { get; init; } = 0.60f;
    public float MaximumHouseAgeDifference { get; init; } = 50f;
    public float MinimumComparablePriceRatio { get; init; } = 0.60f;
    public float MaximumComparablePriceRatio { get; init; } = 1.60f;

    public IReadOnlyDictionary<PropertyQuality, float> QualityMultipliers { get; init; } =
        new Dictionary<PropertyQuality, float>
        {
            [PropertyQuality.Basic] = 0.85f,
            [PropertyQuality.Standard] = 1f,
            [PropertyQuality.Premium] = 1.20f
        };

    public IReadOnlyDictionary<LocationDesirability, float> LocationMultipliers { get; init; } =
        new Dictionary<LocationDesirability, float>
        {
            [LocationDesirability.Low] = 0.70f,
            [LocationDesirability.Average] = 1f,
            [LocationDesirability.High] = 1.35f,
            [LocationDesirability.Prime] = 1.75f
        };

    public void Validate()
    {
        ValidateNonNegative(ConstructionCostPerSquareMetre, nameof(ConstructionCostPerSquareMetre));
        ValidateNonNegative(BaseLandPricePerSquareMetre, nameof(BaseLandPricePerSquareMetre));
        ValidateRate(AnnualDepreciationRate, nameof(AnnualDepreciationRate));
        ValidateRate(MaximumDepreciationRate, nameof(MaximumDepreciationRate));
        ValidatePositive(ComparablesPerFullWeight, nameof(ComparablesPerFullWeight));
        ValidateRate(MaximumComparableWeight, nameof(MaximumComparableWeight));
        ValidatePositive(MinimumSellerPriceMultiplier, nameof(MinimumSellerPriceMultiplier));
        ValidatePositive(MaximumSellerPriceMultiplier, nameof(MaximumSellerPriceMultiplier));
        if (MinimumSellerPriceMultiplier > MaximumSellerPriceMultiplier)
            throw new ArgumentException(
                $"{nameof(MinimumSellerPriceMultiplier)} cannot exceed {nameof(MaximumSellerPriceMultiplier)}.",
                nameof(MinimumSellerPriceMultiplier));
        if (!float.IsFinite(MinimumComparableSimilarity)
            || MinimumComparableSimilarity is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(MinimumComparableSimilarity));

        ValidateNonNegative(LocationSimilarityWeight, nameof(LocationSimilarityWeight));
        ValidateNonNegative(FloorAreaSimilarityWeight, nameof(FloorAreaSimilarityWeight));
        ValidateNonNegative(BuildQualitySimilarityWeight, nameof(BuildQualitySimilarityWeight));
        ValidateNonNegative(HouseAgeSimilarityWeight, nameof(HouseAgeSimilarityWeight));
        float totalSimilarityWeight = LocationSimilarityWeight
            + FloorAreaSimilarityWeight
            + BuildQualitySimilarityWeight
            + HouseAgeSimilarityWeight;
        if (!float.IsFinite(totalSimilarityWeight) || totalSimilarityWeight <= 0)
            throw new ArgumentException(
                "The total similarity weight must be greater than zero.",
                nameof(LocationSimilarityWeight));

        ValidatePositive(MaximumFloorAreaDifferenceRatio, nameof(MaximumFloorAreaDifferenceRatio));
        ValidatePositive(MaximumHouseAgeDifference, nameof(MaximumHouseAgeDifference));
        ValidatePositive(MinimumComparablePriceRatio, nameof(MinimumComparablePriceRatio));
        ValidatePositive(MaximumComparablePriceRatio, nameof(MaximumComparablePriceRatio));
        if (MinimumComparablePriceRatio > MaximumComparablePriceRatio)
            throw new ArgumentException(
                $"{nameof(MinimumComparablePriceRatio)} cannot exceed {nameof(MaximumComparablePriceRatio)}.",
                nameof(MinimumComparablePriceRatio));

        ValidateMultipliers(QualityMultipliers, nameof(QualityMultipliers));
        ValidateMultipliers(LocationMultipliers, nameof(LocationMultipliers));
    }

    private static void ValidateRate(float value, string propertyName)
    {
        if (!float.IsFinite(value) || value is < 0 or > 1)
            throw new ArgumentOutOfRangeException(propertyName);
    }

    private static void ValidateNonNegative(float value, string propertyName)
    {
        if (!float.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(propertyName);
    }

    private static void ValidatePositive(float value, string propertyName)
    {
        if (!float.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(propertyName);
    }

    private static void ValidateMultipliers<TEnum>(
        IReadOnlyDictionary<TEnum, float>? multipliers,
        string propertyName)
        where TEnum : struct, Enum
    {
        if (multipliers is null)
            throw new ArgumentNullException(propertyName);

        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            if (!multipliers.TryGetValue(value, out float multiplier))
                throw new ArgumentException(
                    $"{propertyName} is missing an entry for {value}.",
                    propertyName);
            if (!float.IsFinite(multiplier) || multiplier <= 0)
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    $"{propertyName}[{value}] must be finite and greater than zero.");
        }
    }
}
