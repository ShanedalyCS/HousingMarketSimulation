using System;

public class Simulation
{
    public Market Market { get; }
    public int CurrentMonth { get; private set; }

    public Simulation(Market market)
    {
        Market = market;
        CurrentMonth = 0;
    }

    public void RunTick()
    {
        CurrentMonth++;

        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");

        UpdateBuyerFinances();
        CheckAffordableHouses();
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
            foreach (House house in Market.Houses)
            {
                if (buyer.CanAfford(house))
                {
                    Console.WriteLine(
                        $"{buyer.Name} can afford {house.Name}");
                }
            }
        }
    }
}