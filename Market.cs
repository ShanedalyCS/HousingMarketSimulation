using System.Collections.Generic;

class Market
{
    public List<House> Houses { get; set; }
    public List<Buyer> Buyers { get; set; }

    public Market()
    {
        Houses = new List<House>();
        Buyers = new List<Buyer>();
    }
}