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

}
