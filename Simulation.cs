using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;

public class Simulation(Market market)
{
    public Market Market { get; } = market;
    public int CurrentMonth { get; private set; } = 0;

    public void RunTick()
    {
        CurrentMonth++;
        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");
        Market.Bids.Clear();

        UpdateBuyerFinances();
        CheckAffordableHouses();
        LogAffordableHouses();
        FindBestAffordableHouse();
        MakeAndLogBids();
        DeliberateBids();
        LogTransactionDetails();
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
            if (buyer.AffordableHouses.Count > 0)
            {

                foreach (House affordableHouse in buyer.AffordableHouses)
                {
                    lineOfAffordableHouses += affordableHouse.Name + ", ";
                }
                Console.WriteLine(buyer.Name + " can afford " + lineOfAffordableHouses);
            }
        }
    }

    public void FindBestAffordableHouse()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            buyer.WinningHouse = buyer.AffordableHouses
                .OrderByDescending(house => house.Quality)
                .FirstOrDefault();
        }
    }

    public void MakeAndLogBids()
    {
        foreach (Buyer buyer in Market.Buyers)
        {
            if (buyer.winningHouse != null)
            {
                Market.Bids.Add(new Bid(buyer, buyer.winningHouse, buyer.winningHouse.value));
            }
        }
        Market.LogBidDetails();
    }

    public void DeliberateBids()
    {
        foreach (House house in Market.Houses)
        {
            house.DeliberateBids();
            Market.RemoveSoldHousesAndBuyersFromMarket();
        }
    }

    public void LogTransactionDetails()
    {
        Market.LogTransactionDetails();
    }
}
