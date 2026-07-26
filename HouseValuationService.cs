public sealed class HouseValuationService
{
    public HouseValuationService(ValuationSettings? settings = null)
    {
        Settings = settings ?? new ValuationSettings();
        Settings.Validate();
    }

    public ValuationSettings Settings { get; }

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
        return MathF.Round(
            MathF.Max(0, landValue + replacementCost - replacementCost * depreciationRate),
            2);
    }

    public float GenerateSellerPricingMultiplier(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        float range = Settings.MaximumSellerPriceMultiplier - Settings.MinimumSellerPriceMultiplier;
        return Settings.MinimumSellerPriceMultiplier + (float)random.NextDouble() * range;
    }

    public float GenerateAskingPrice(float baseValue, Random random)
    {
        if (baseValue < 0) throw new ArgumentOutOfRangeException(nameof(baseValue));
        return MathF.Round(baseValue * GenerateSellerPricingMultiplier(random), 2);
    }

    public float EstimateMarketValue(House house, IEnumerable<Transaction> completedTransactions)
    {
        ArgumentNullException.ThrowIfNull(house);
        ArgumentNullException.ThrowIfNull(completedTransactions);

        List<(Transaction Transaction, float Similarity)> comparables = completedTransactions
            .Where(transaction => transaction.House != house && transaction.House.BaseValue > 0)
            .Select(transaction => (
                Transaction: transaction,
                Similarity: CalculateSimilarity(house, transaction.House)))
            .Where(item => item.Similarity >= Settings.MinimumComparableSimilarity)
            .ToList();

        if (comparables.Count == 0)
        {
            house.EstimatedMarketValue = house.BaseValue;
            return house.EstimatedMarketValue;
        }

        float similarityTotal = comparables.Sum(item => item.Similarity);
        float weightedPriceRatio = comparables.Sum(item =>
        {
            float priceRatio = item.Transaction.SalePrice / item.Transaction.House.BaseValue;
            float boundedRatio = Math.Clamp(
                priceRatio,
                Settings.MinimumComparablePriceRatio,
                Settings.MaximumComparablePriceRatio);
            return boundedRatio * item.Similarity;
        }) / similarityTotal;
        float comparableEstimate = house.BaseValue * weightedPriceRatio;
        float comparableWeight = MathF.Min(
            Settings.MaximumComparableWeight,
            similarityTotal / Settings.ComparablesPerFullWeight * Settings.MaximumComparableWeight);
        house.EstimatedMarketValue = MathF.Round(
            house.BaseValue * (1f - comparableWeight) + comparableEstimate * comparableWeight,
            2);
        return house.EstimatedMarketValue;
    }

    private float CalculateSimilarity(House subject, House comparable)
    {
        int locationDistance = Math.Abs((int)subject.Location - (int)comparable.Location);
        if (locationDistance >= 2) return 0;
        float locationSimilarity = locationDistance switch
        {
            0 => 1f,
            1 => 0.35f,
            _ => 0f
        };
        float floorDifferenceRatio = MathF.Abs(
            subject.FloorAreaSquareMetres - comparable.FloorAreaSquareMetres)
            / MathF.Max(subject.FloorAreaSquareMetres, comparable.FloorAreaSquareMetres);
        float floorSimilarity = 1f - MathF.Min(
            floorDifferenceRatio / Settings.MaximumFloorAreaDifferenceRatio,
            1f);
        float qualitySimilarity = 1f
            - Math.Abs((int)subject.BuildQuality - (int)comparable.BuildQuality) / 2f;
        float ageSimilarity = 1f - MathF.Min(
            MathF.Abs(subject.AgeYears - comparable.AgeYears)
                / Settings.MaximumHouseAgeDifference,
            1f);

        float weightTotal = Settings.LocationSimilarityWeight
            + Settings.FloorAreaSimilarityWeight
            + Settings.BuildQualitySimilarityWeight
            + Settings.HouseAgeSimilarityWeight;
        return (
            locationSimilarity * Settings.LocationSimilarityWeight
            + floorSimilarity * Settings.FloorAreaSimilarityWeight
            + qualitySimilarity * Settings.BuildQualitySimilarityWeight
            + ageSimilarity * Settings.HouseAgeSimilarityWeight)
            / weightTotal;
    }
}
