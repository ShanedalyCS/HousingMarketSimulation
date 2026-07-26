internal static class TestHouseFactory
{
    public static House Create(
        float askingPrice,
        string name = "House",
        float floorArea = 80,
        float plotSize = 120,
        float age = 20,
        PropertyQuality quality = PropertyQuality.Standard,
        LocationDesirability location = LocationDesirability.Average,
        HouseValuationService? valuationService = null,
        IEnumerable<Transaction>? transactions = null)
    {
        House house = new(
            name,
            floorArea,
            plotSize,
            age,
            quality,
            location,
            valuationService ?? new HouseValuationService(),
            new Random(1),
            transactions);
        house.AskingPrice = askingPrice;
        return house;
    }
}
