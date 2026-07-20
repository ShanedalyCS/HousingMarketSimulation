public class RandomnessTests
{
    [Fact]
    public void IdenticalSeedsGenerateIdenticalInitialMarkets()
    {
        Market first = GenerateMarket(12345);
        Market second = GenerateMarket(12345);

        Assert.Equal(
            first.Buyers.Select(BuyerSnapshot),
            second.Buyers.Select(BuyerSnapshot));
        Assert.Equal(
            first.Houses.Select(HouseSnapshot),
            second.Houses.Select(HouseSnapshot));
    }

    private static Market GenerateMarket(int seed)
    {
        Market market = new();
        new DataGenerator(new Random(seed)).GenerateData(5, 5, market);
        return market;
    }

    private static string BuyerSnapshot(Buyer buyer)
    {
        return $"{buyer.Name}|{buyer.Age}|{buyer.Salary}|{buyer.Motivation}|{buyer.Savings}|{buyer.HasFamily}";
    }

    private static string HouseSnapshot(House house)
    {
        return $"{house.Name}|{house.Value}|{house.Technology}|{house.Age}|{house.Area}|{house.Size}";
    }
}
