public sealed class AuctionService(SimulationSettings? settings = null)
{
    private readonly SimulationSettings settings = settings ?? new SimulationSettings();

    public Transaction? Settle(House house, Random random, ISet<Buyer>? successfulBuyers = null)
    {
        ArgumentNullException.ThrowIfNull(house);
        ArgumentNullException.ThrowIfNull(random);

        List<Bid> availableBids = house.Bids
            .Where(bid => successfulBuyers is null || !successfulBuyers.Contains(bid.Buyer))
            .ToList();
        if (availableBids.Count == 0 || availableBids.Max(bid => bid.OfferAmount) < house.AskingPrice)
        {
            return null;
        }

        List<Bid> qualifying = availableBids
            .Where(bid => bid.OfferAmount >= house.AskingPrice)
            .OrderByDescending(bid => bid.OfferAmount)
            .ToList();
        float highestAmount = qualifying[0].OfferAmount;
        List<Bid> tiedWinners = qualifying
            .Where(bid => bid.OfferAmount == highestAmount)
            .ToList();
        Bid winner = tiedWinners.Count == 1
            ? tiedWinners[0]
            : tiedWinners[random.Next(tiedWinners.Count)];

        float salePrice = house.AskingPrice;
        if (qualifying.Count > 1)
        {
            float secondHighest = qualifying
                .Where(bid => !ReferenceEquals(bid, winner))
                .Max(bid => bid.OfferAmount);
            salePrice = MathF.Min(
                winner.OfferAmount,
                MathF.Max(house.AskingPrice, secondHighest + settings.AuctionIncrement));
        }

        successfulBuyers?.Add(winner.Buyer);
        return new Transaction(
            winner.Buyer,
            house,
            MathF.Round(salePrice, 2),
            house.AskingPrice,
            house.MonthsOnMarket);
    }
}
