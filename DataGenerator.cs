public class DataGenerator
{
    List<string> names;
    readonly string path = "first-names.txt";
    Random rnd = new();
    int nextHouseId;

    public DataGenerator()
    {
        names = new List<string>();


        if (File.Exists(path))
        {
            foreach (string line in File.ReadLines(path))
            {
                if (!string.IsNullOrWhiteSpace(line)) names.Add(line.Trim());
            }
        }
    }

    public String GenerateData(
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


        return "Data Generated";
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

    private void GenerateBuyer(Market market)
    {
        string name = "Unknown";
        if (names != null && names.Count > 0)
        {
            var rnd = new Random();
            name = names[rnd.Next(names.Count)];
        }

        int age = rnd.Next(70);

        float salary = rnd.Next(30, 200);

        float motivation = rnd.Next(10);

        float savings = salary / 2;

        bool hasFamily = true;

        Buyer buyer = new(name, age, salary, motivation, savings, hasFamily);

        market.Buyers.Add(buyer);

    }

    private void GenerateHouse(Market market, bool startPriceAtZero)
    {
        float technology = rnd.Next(-3, 10);
        float age = rnd.Next(-3, 10);
        float area = rnd.Next(-3, 10);
        float size = rnd.Next(-3, 10);


        float value = (technology + age + area + size) * 8;

        if (value <= 0) value = 30;
        if (startPriceAtZero) value = 0;


        House house = new(nextHouseId.ToString(), value, technology, age, area, size);
        nextHouseId++;

        market.Houses.Add(house);
    }
}
