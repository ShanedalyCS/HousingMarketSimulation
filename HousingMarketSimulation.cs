using System;
public class HousingMarketSimulation
{

    public static void Main(string[] args)
    {
        Console.WriteLine("hello world.");


        Buyer bOne = new("Shane Daly", 22, 35, 10, 20, false);
        Buyer bTwo = new("Clara Aguilar", 22, 30, 5, 12, false);
        Buyer bThree = new("Dan Theman", 39, 70, 9, 40, true);
        Buyer bFour = new("Ruth Bababooie", 32, 100, 4, 60, true);

        House houseOne = new("23 Home Street", 230, 6, 8, 5, 1);

        Market market = new();
        market.Buyers.Add(bOne);
        market.Buyers.Add(bTwo);
        market.Buyers.Add(bThree);
        market.Buyers.Add(bFour);

        market.Houses.Add(houseOne);

        market.LogBuyerDetails();
        market.LogHouseDetails();

        Simulation simulation = new(market);
        simulation.RunTick();


    }
}