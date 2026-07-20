using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;

public class Market
{
    public List<House> Houses { get; set; }
    public List<Buyer> Buyers { get; set; }
    public List<Bid> Bids { get; set; }
    public List<Transaction> Transactions { get; set; }

    public Market()
    {
        Houses = new List<House>();
        Buyers = new List<Buyer>();
        Bids = new List<Bid>();
        Transactions = new List<Transaction>();
    }

    public void LogBuyerDetails()
    {
        for (int i = 0; i < Buyers.Count; i++)
        {
            if (i == 0) Console.WriteLine("====== BUYER DETAILS ======");
            Buyer b = Buyers[i];

            Console.WriteLine("Name: " + b.Name + "| Age: " + b.age + "| Salary (k): " + b.salary + "| Motivation: " + b.motivation + "| Savings (k): " + b.savings + "| Has Family: " + b.hasFamily);
            if (i == Buyers.Count - 1) Console.WriteLine("====== END OF BUYERS ======");
        }
    }

    public void LogHouseDetails()
    {
        for (int i = 0; i < Houses.Count; i++)
        {

            if (i == 0) Console.WriteLine("====== HOUSE DETAILS ======");

            House h = Houses[i];
            Console.WriteLine(" Name: " + h.Name + "| Value (k): " + h.value + "| Tech: " + h.technology + "| Age: " + h.age + "| Size: " + h.size);

            if (i == Houses.Count() - 1) Console.WriteLine("====== END OF HOUSES ======");
        }
    }

    public void LogBidDetails()
    {
        foreach (Bid bid in Bids)
        {
            Console.WriteLine(bid.buyer.Name + " placed a bid on " + bid.house.Name + " for " + bid.offerAmount + " K");
        }
    }

    public void LogTransactionDetails()
    {
        if (Transactions != null)
        {
            foreach (Transaction transaction in Transactions)
            {
                Console.WriteLine("TRANSACTION : " + transaction.Buyer.Name + " bought " + transaction.House.Name + " for " + transaction.SalePrice + " K");
            }
        }
    }

    public void RemoveSoldHousesAndBuyersFromMarket()
    {
        foreach (Transaction transaction in Transactions)
        {
            if (Houses.Contains(transaction.House))
            {
                Houses.Remove(transaction.House);
                Buyers.Remove(transaction.Buyer);
            }
        }
    }

}