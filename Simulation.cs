using System;
using System.Diagnostics;

public class Simulation(Market market)
{
    public Market Market { get; } = market;
    public int CurrentMonth { get; private set; } = 0;

    public void RunTick()
    {
        CurrentMonth++;

        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");

        UpdateBuyerFinances();
        CheckAffordableHouses();
        LogAffordableHouses();
    }

    private void UpdateBuyerFinances()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            float monthlySalary = buyer.Salary / 12f;
            float amountSaved = monthlySalary * 0.20f;

            buyer.Savings += amountSaved;
        }
    }

    private void CheckAffordableHouses()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            buyer.AffordableHouses.Clear();
            foreach (House house in Market.Houses)
            {
                if (buyer.CanAfford(house))
                {
                    buyer.AffordableHouses.Add(house);
                }
            }
        }
    }


    public void LogAffordableHouses()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            string lineOfAffordableHouses = "";
            if (buyer.AffordableHouses != null)
            {

                foreach (House affordableHouse in buyer.AffordableHouses)
                {
                    lineOfAffordableHouses += affordableHouse.Name + ", ";
                }
                Console.WriteLine(buyer.Name + " can afford " + lineOfAffordableHouses);
            }
        }
    }

}