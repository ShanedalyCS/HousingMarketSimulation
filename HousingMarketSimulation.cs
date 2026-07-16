using System;
public class HousingMarketSimulation
{

    public static void Main(string[] args)
    {
        Console.WriteLine("hello world.");


        Buyer bOne = new("Shane Daly", 22, 35, 10, 20, false);
        Console.WriteLine(bOne.age + " " + bOne.salary);
    }
}