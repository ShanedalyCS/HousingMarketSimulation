public sealed record BuyerHouseEvaluation(
    House House,
    float SuitabilityScore,
    float PerceivedValue,
    float MaximumBid,
    float RankingScore);

public sealed class BuyerDecisionService(SimulationSettings? settings = null)
{
    private readonly SimulationSettings settings = settings ?? new SimulationSettings();

    public BuyerHouseEvaluation Evaluate(Buyer buyer, House house)
    {
        ArgumentNullException.ThrowIfNull(buyer);
        ArgumentNullException.ThrowIfNull(house);

        BuyerPreferences preferences = buyer.Preferences.Normalize();
        float suitability =
            preferences.LocationWeight * ((int)house.Location / 3f)
            + preferences.BuildQualityWeight * ((int)house.BuildQuality / 2f)
            + preferences.FloorAreaWeight * MathF.Min(house.FloorAreaSquareMetres / 180f, 1f)
            + preferences.PlotSizeWeight * MathF.Min(house.PlotSizeSquareMetres / 500f, 1f)
            + preferences.HouseAgeWeight * (1f - MathF.Min(house.AgeYears / 100f, 1f));

        // Preference fit moves perceived value between 80% and 120% of the market estimate.
        float perceivedValue = house.EstimatedMarketValue * (0.80f + 0.40f * suitability);
        float motivatedValue = perceivedValue * (1f + buyer.Motivation / 200f);
        float maximumBid = MathF.Min(buyer.CalculateMaximumPurchasePrice(), motivatedValue);
        float valueForMoney = house.AskingPrice <= 0
            ? 1f
            : MathF.Min(perceivedValue / house.AskingPrice, 2f) / 2f;
        float rankingScore = 0.70f * suitability + 0.30f * valueForMoney;

        return new BuyerHouseEvaluation(
            house,
            MathF.Round(suitability, 6),
            MathF.Round(perceivedValue, 2),
            MathF.Round(maximumBid, 2),
            MathF.Round(rankingScore, 6));
    }

    public bool CanSubmitBid(BuyerHouseEvaluation evaluation) =>
        evaluation.MaximumBid >= evaluation.House.AskingPrice
            * (1f - settings.BelowAskingBidTolerance);

    public BuyerHouseEvaluation? ChooseHouse(
        Buyer buyer,
        IEnumerable<House> houses,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        List<BuyerHouseEvaluation> eligible = houses
            .Select(house => Evaluate(buyer, house))
            .Where(CanSubmitBid)
            .ToList();
        if (eligible.Count == 0) return null;

        float bestScore = eligible.Max(evaluation => evaluation.RankingScore);
        List<BuyerHouseEvaluation> tied = eligible
            .Where(evaluation => MathF.Abs(evaluation.RankingScore - bestScore) < 0.000001f)
            .ToList();
        return tied.Count == 1 ? tied[0] : tied[random.Next(tied.Count)];
    }
}
