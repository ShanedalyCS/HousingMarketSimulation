using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Collections.Generic;

public class DataGenerator
{
    // int numBuyers
    // {get; set;}
    // int numHouses
    // {get; set;}

    List<string> names;
    readonly string path = "first-names.txt";
    Random rnd = new();

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

    public String GenerateData(int numBuyers, int numHouses, Market market)
    {
        for (int i = 0; i < numBuyers; i++)
        {
            GenerateBuyer(market);
        }

        for (int i = 0; i < numHouses; i++)
        {
            int TEMP_NAME_VAR = i;
            GenerateHouse(TEMP_NAME_VAR, market);
        }


        return "Data Generated";
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

        // TODO: create Buyer object and populate fields
        // name, age, salary, motivation, savings, hasFamily

        Buyer buyer = new(name, age, salary, motivation, savings, hasFamily);

        market.Buyers.Add(buyer);

    }

    private void GenerateHouse(int TEMP_NAME_VAR, Market market)
    {
        float technology = rnd.Next(-3, 10);
        float age = rnd.Next(-3, 10);
        float area = rnd.Next(-3, 10);
        float size = rnd.Next(-3, 10);


        float value = (technology + age + area + size) * 8;

        if (value <= 0) value = 30;


        //string name, float value, float technology, float age, float area, float size)
        House house = new(TEMP_NAME_VAR.ToString(), value, technology, age, area, size);

        market.Houses.Add(house);
    }
}