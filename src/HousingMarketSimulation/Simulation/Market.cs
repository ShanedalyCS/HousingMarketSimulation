public class Market
{
    public List<House> Houses { get; set; }
    public List<Buyer> Buyers { get; set; }
    public List<Bid> Bids { get; set; }
    public List<Transaction> Transactions { get; set; }
    public List<MonthlyMarketReport> MonthlyReports { get; set; }
    public List<MonthlyAnalyticsSnapshot> AnalyticsSnapshots { get; set; }

    public Market()
    {
        Houses = new List<House>();
        Buyers = new List<Buyer>();
        Bids = new List<Bid>();
        Transactions = new List<Transaction>();
        MonthlyReports = new List<MonthlyMarketReport>();
        AnalyticsSnapshots = new List<MonthlyAnalyticsSnapshot>();
    }

    public void LogBuyerDetails()
    {
        for (int i = 0; i < Buyers.Count; i++)
        {
            if (i == 0) Console.WriteLine("====== BUYER DETAILS ======");
            Buyer b = Buyers[i];

            Console.WriteLine(
                "Name: " + b.Name + "| Age: " + b.Age + "| Salary (k): " + b.Salary +
                "| Motivation: " + b.Motivation + "| Savings (k): " + b.Savings +
                "| Has Family: " + b.HasFamily);
            if (i == Buyers.Count - 1) Console.WriteLine("====== END OF BUYERS ======");
        }
    }

    public void LogHouseDetails()
    {
        for (int i = 0; i < Houses.Count; i++)
        {

            if (i == 0) Console.WriteLine("====== HOUSE DETAILS ======");

            House h = Houses[i];
            Console.WriteLine(
                $" Name: {h.Name}| Asking price (k): {h.AskingPrice}" +
                $"| Base value (k): {h.BaseValue}| Age: {h.AgeYears}" +
                $"| Floor area: {h.FloorAreaSquareMetres}");

            if (i == Houses.Count() - 1) Console.WriteLine("====== END OF HOUSES ======");
        }
    }

    public void LogBidDetails()
    {
        foreach (Bid bid in Bids)
        {
            Console.WriteLine(
                bid.Buyer.Name + " placed a bid on " + bid.House.Name +
                " for " + bid.OfferAmount + " K");
        }
    }

    public void LogTransactionDetails(IEnumerable<Transaction> transactions)
    {
        foreach (Transaction transaction in transactions)
        {
            Console.WriteLine("    TRANSACTION : " + transaction.Buyer.Name + " bought " + transaction.House.Name + " for " + transaction.SalePrice + " K");
        }
    }

    public void RemoveSoldHousesAndBuyersFromMarket(IEnumerable<Transaction> transactions)
    {
        foreach (Transaction transaction in transactions)
        {
            Houses.Remove(transaction.House);
            Buyers.Remove(transaction.Buyer);
        }
    }

}
