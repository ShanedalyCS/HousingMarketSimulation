public class Simulation(Market market)
{
    public Market Market { get; } = market;
    public int CurrentMonth { get; private set; } = 0;

    public void RunTick()
    {
        CurrentMonth++;
        Console.WriteLine($"\n===== MONTH {CurrentMonth} =====");
        
        ClearMonthlyBiddingState();

        UpdateBuyerFinances();
        CheckAffordableHouses();
        LogAffordableHouses();
        FindBestAffordableHouse();
        MakeAndLogBids();
        List<Transaction> completedTransactions = DeliberateBids();
        Market.LogTransactionDetails(completedTransactions);
    }

    private void ClearMonthlyBiddingState()
    {
        Market.Bids.Clear();

        foreach (House house in Market.Houses)
        {
            house.bids.Clear();
        }
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
                House house = buyer.winningHouse;
                float motivationPremium = house.Value * (buyer.Motivation / 100f);
                float offerAmount = MathF.Min(
                    house.Value + motivationPremium,
                    buyer.CalculateMaximumPurchasePrice());

                Market.Bids.Add(new Bid(buyer, house, offerAmount));
            }
        }
        Market.LogBidDetails();
    }

    public List<Transaction> DeliberateBids()
    {
        List<Transaction> completedTransactions = [];
        HashSet<Buyer> successfulBuyers = [];

        foreach (House house in Market.Houses)
        {
            Transaction? transaction = house.DeliberateBids();
            if (transaction != null && successfulBuyers.Add(transaction.Buyer))
            {
                completedTransactions.Add(transaction);
            }
        }

        Market.Transactions.AddRange(completedTransactions);
        Market.RemoveSoldHousesAndBuyersFromMarket(completedTransactions);

        return completedTransactions;
    }
}
