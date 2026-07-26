public sealed record BuyerPreferences(
    float LocationWeight,
    float BuildQualityWeight,
    float FloorAreaWeight,
    float PlotSizeWeight,
    float HouseAgeWeight)
{
    public static BuyerPreferences Balanced { get; } = new(1, 1, 1, 1, 1);

    public static BuyerPreferences Generate(Random random, bool hasFamily)
    {
        ArgumentNullException.ThrowIfNull(random);

        float location = 0.5f + (float)random.NextDouble();
        float quality = 0.5f + (float)random.NextDouble();
        float floorArea = 0.5f + (float)random.NextDouble() + (hasFamily ? 0.65f : 0);
        float plotSize = 0.5f + (float)random.NextDouble() + (hasFamily ? 0.45f : 0);
        float age = 0.5f + (float)random.NextDouble();
        return new BuyerPreferences(location, quality, floorArea, plotSize, age).Normalize();
    }

    public BuyerPreferences Normalize()
    {
        float total = LocationWeight + BuildQualityWeight + FloorAreaWeight
            + PlotSizeWeight + HouseAgeWeight;
        if (total <= 0) throw new InvalidOperationException("Preference weights must have a positive total.");

        return new BuyerPreferences(
            LocationWeight / total,
            BuildQualityWeight / total,
            FloorAreaWeight / total,
            PlotSizeWeight / total,
            HouseAgeWeight / total);
    }
}
