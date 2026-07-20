public class DataGenerator
{
    private readonly List<string> names = [];
    private readonly Random random;
    private int nextHouseId;

    public DataGenerator(Random? random = null)
    {
        this.random = random ?? new Random();
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
        Market market,
        bool startHousePricesAtZero = false)
    {
        for (int i = 0; i < numBuyers; i++)
        {
            GenerateBuyer(market);
        }

        for (int i = 0; i < numHouses; i++)
        {
            GenerateHouse(market, startHousePricesAtZero);
        }
    }

    public void AddMonthlyEntrants(
        Market market,
        int numberOfBuyers,
        int numberOfHouses,
        bool startHousePricesAtZero = false)
    {
        for (int i = 0; i < numberOfBuyers; i++)
        {
            GenerateBuyer(market);
        }

        for (int i = 0; i < numberOfHouses; i++)
        {
            GenerateHouse(market, startHousePricesAtZero);
        }
    }

    public Random Random => random;

    private void GenerateBuyer(Market market)
    {
        string name = "Unknown";
        if (names.Count > 0)
        {
            name = names[random.Next(names.Count)];
        }

        int age = random.Next(70);
        float salary = random.Next(30, 200);
        float motivation = random.Next(10);

        float savings = salary / 2;

        bool hasFamily = true;

        Buyer buyer = new(name, age, salary, motivation, savings, hasFamily);

        market.Buyers.Add(buyer);

    }

    private void GenerateHouse(Market market, bool startPriceAtZero)
    {
        float technology = random.Next(-3, 10);
        float age = random.Next(-3, 10);
        float area = random.Next(-3, 10);
        float size = random.Next(-3, 10);


        float value = (technology + age + area + size) * 8;

        if (value <= 0) value = 30;
        if (startPriceAtZero) value = 0;


        House house = new(nextHouseId.ToString(), value, technology, age, area, size);
        nextHouseId++;

        market.Houses.Add(house);
    }
}
