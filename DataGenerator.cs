public class DataGenerator
{
    private readonly List<string> names = [];
    private readonly Random random;
    private readonly HouseValuationService valuationService;
    private int nextHouseId;

    public DataGenerator(Random? random = null, HouseValuationService? valuationService = null)
    {
        this.random = random ?? new Random();
        this.valuationService = valuationService ?? new HouseValuationService();
        const string path = "first-names.txt";

        if (File.Exists(path))
        {
            foreach (string line in File.ReadLines(path))
            {
                if (!string.IsNullOrWhiteSpace(line)) names.Add(line.Trim());
            }
        }
    }

    public void GenerateData(
        int numBuyers,
        int numHouses,
        Market market)
    {
        for (int i = 0; i < numBuyers; i++)
        {
            GenerateBuyer(market);
        }

        for (int i = 0; i < numHouses; i++)
        {
            GenerateHouse(market);
        }
    }

    public void AddMonthlyEntrants(
        Market market,
        int numberOfBuyers,
        int numberOfHouses)
    {
        for (int i = 0; i < numberOfBuyers; i++)
        {
            GenerateBuyer(market);
        }

        for (int i = 0; i < numberOfHouses; i++)
        {
            GenerateHouse(market);
        }
    }

    public Random Random => random;
    public HouseValuationService ValuationService => valuationService;

    private void GenerateBuyer(Market market)
    {
        string name = "Unknown";
        if (names.Count > 0)
        {
            name = names[random.Next(names.Count)];
        }

        int age = random.Next(18, 71);
        float salary = random.Next(30, 200);
        float motivation = MathF.Round((float)random.NextDouble() * 10f, 2);
        float savings = MathF.Round(
            salary * (0.25f + (float)random.NextDouble()),
            2);
        bool hasFamily = age >= 22 && random.NextDouble() < 0.45;
        BuyerPreferences preferences = BuyerPreferences.Generate(random, hasFamily);

        Buyer buyer = new(
            name,
            age,
            salary,
            motivation,
            savings,
            hasFamily,
            preferences);

        market.Buyers.Add(buyer);

    }

    private void GenerateHouse(Market market)
    {
        float floorArea = random.Next(70, 221);
        float plotSize = random.Next(100, 701);

        House house = new(
            nextHouseId.ToString(),
            floorArea,
            plotSize,
            random.Next(0, 101),
            (PropertyQuality)random.Next(Enum.GetValues<PropertyQuality>().Length),
            (LocationDesirability)random.Next(Enum.GetValues<LocationDesirability>().Length),
            valuationService,
            random,
            market.Transactions);

        nextHouseId++;

        market.Houses.Add(house);
    }
}
