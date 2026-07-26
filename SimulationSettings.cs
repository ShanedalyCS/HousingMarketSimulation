public sealed class SimulationSettings
{
    public int NewBuyersPerMonth { get; init; } = 1;
    public int NewHousesPerMonth { get; init; } = 1;
    public float NoBidPriceReduction { get; init; } = 0.02f;
    public float MaximumMonthlyMarketValueAdjustment { get; init; } = 0.03f;
    public float BelowAskingBidTolerance { get; init; } = 0.05f;
    public float AuctionIncrement { get; init; } = 0.50f;
    public float RejectedBidAdjustmentRate { get; init; } = 0.50f;
    public float MonthlySavingsRate { get; init; } = 0.20f;

    public void Validate()
    {
        if (NewBuyersPerMonth < 0) throw new ArgumentOutOfRangeException(nameof(NewBuyersPerMonth));
        if (NewHousesPerMonth < 0) throw new ArgumentOutOfRangeException(nameof(NewHousesPerMonth));
        ValidateRate(NoBidPriceReduction, nameof(NoBidPriceReduction));
        ValidateRate(MaximumMonthlyMarketValueAdjustment, nameof(MaximumMonthlyMarketValueAdjustment));
        ValidateRate(BelowAskingBidTolerance, nameof(BelowAskingBidTolerance));
        ValidateRate(RejectedBidAdjustmentRate, nameof(RejectedBidAdjustmentRate));
        ValidateRate(MonthlySavingsRate, nameof(MonthlySavingsRate));
        if (AuctionIncrement < 0) throw new ArgumentOutOfRangeException(nameof(AuctionIncrement));
    }

    private static void ValidateRate(float value, string parameterName)
    {
        if (value is < 0 or > 1) throw new ArgumentOutOfRangeException(parameterName);
    }
}
