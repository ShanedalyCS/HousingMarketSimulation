public sealed class SellerPricingService(SimulationSettings? settings = null)
{
    private readonly SimulationSettings settings = settings ?? new SimulationSettings();

    public int MoveTowardMarketTarget(House house)
    {
        float target = house.EstimatedMarketValue * house.SellerPricingMultiplier;
        return MoveToward(house, target, settings.MaximumMonthlyMarketValueAdjustment);
    }

    public int AdjustUnsuccessfulListing(House house)
    {
        if (house.Bids.Count == 0)
        {
            return MoveToward(
                house,
                house.AskingPrice * (1f - settings.NoBidPriceReduction),
                settings.NoBidPriceReduction);
        }

        float highestRejectedBid = house.Bids.Max(bid => bid.OfferAmount);
        if (highestRejectedBid < house.AskingPrice)
        {
            float target = house.AskingPrice
                + (highestRejectedBid - house.AskingPrice) * settings.RejectedBidAdjustmentRate;
            return MoveToward(house, target, settings.MaximumMonthlyMarketValueAdjustment);
        }

        return 0;
    }

    private static int MoveToward(House house, float target, float maximumRate)
    {
        if (house.AskingPrice <= 0) return 0;
        float original = house.AskingPrice;
        float maximumChange = original * maximumRate;
        float change = Math.Clamp(target - original, -maximumChange, maximumChange);
        house.AskingPrice = MathF.Round(MathF.Max(0, original + change), 2);
        return house.AskingPrice > original ? 1 : house.AskingPrice < original ? -1 : 0;
    }
}
