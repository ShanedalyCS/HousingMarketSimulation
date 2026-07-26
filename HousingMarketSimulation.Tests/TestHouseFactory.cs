internal static class TestHouseFactory
{
    public static House Create(float askingPrice, string name = "House")
    {
        House house = new(
            name,
            80,
            120,
            20,
            PropertyQuality.Standard,
            LocationDesirability.Average,
            new HouseValuationService(),
            new Random(1));
        house.AskingPrice = askingPrice;
        return house;
    }
}
