public class HousingMarketSimulation
{

    public static void Main(string[] args)
    {
        Market market = new();
        DataGenerator dataGenerator = new();

        Console.WriteLine("How many people?");
        int numberOfBuyers = int.Parse(Console.ReadLine()!);

        Console.WriteLine("How many houses?");
        int numberOfHouses = int.Parse(Console.ReadLine()!);

        dataGenerator.GenerateData(numberOfBuyers, numberOfHouses, market);



        market.LogBuyerDetails();
        market.LogHouseDetails();

        Simulation simulation = new(market);

        Console.WriteLine("How many months so simulate?");
        int numberOfTicks = int.Parse(Console.ReadLine()!);

        for (int i = 0; i < numberOfTicks; i++)
        {
            simulation.RunTick();
            Thread.Sleep(100);
        }



    }
}
