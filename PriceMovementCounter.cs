public readonly record struct PriceMovementSummary(int Reductions, int Increases);

public static class PriceMovementCounter
{
    public const float DefaultTolerance = 0.005f;

    public static PriceMovementSummary CountNetMovements(
        IReadOnlyDictionary<House, float> startingPrices,
        float tolerance = DefaultTolerance)
    {
        ArgumentNullException.ThrowIfNull(startingPrices);
        if (!float.IsFinite(tolerance) || tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance));
        }

        int reductions = 0;
        int increases = 0;
        foreach ((House house, float startingPrice) in startingPrices)
        {
            float difference = house.AskingPrice - startingPrice;
            if (difference < -tolerance) reductions++;
            if (difference > tolerance) increases++;
        }

        return new PriceMovementSummary(reductions, increases);
    }
}
