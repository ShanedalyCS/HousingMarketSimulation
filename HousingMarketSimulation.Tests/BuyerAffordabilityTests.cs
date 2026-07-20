public class BuyerAffordabilityTests
{
    [Fact]
    public void AdditionalSavingsCanBeUsedAboveMinimumDeposit()
    {
        Buyer buyer = new("Buyer", 30, 20, 5, 30, false);

        Assert.Equal(110f, buyer.CalculateMaximumPurchasePrice());
        Assert.True(buyer.CanAfford(CreateHouse(110)));
    }

    [Fact]
    public void MinimumDepositStillLimitsPurchasePrice()
    {
        Buyer buyer = new("Buyer", 30, 100, 5, 10, false);

        Assert.Equal(50f, buyer.CalculateMaximumPurchasePrice());
        Assert.False(buyer.CanAfford(CreateHouse(51)));
    }

    private static House CreateHouse(float price)
    {
        return new House("House", price, 1, 1, 1, 1);
    }
}
