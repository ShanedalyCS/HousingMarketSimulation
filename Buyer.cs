public class Buyer
{
    public Buyer(
        string name,
        int age,
        float salary,
        float motivation,
        float savings,
        bool hasFamily,
        BuyerPreferences? preferences = null)
    {
        Name = name;
        Age = age;
        Salary = salary;
        Motivation = motivation;
        Savings = savings;
        HasFamily = hasFamily;
        Preferences = preferences ?? BuyerPreferences.Balanced;
    }

    public string Name { get; set; }
    public int Age { get; set; }
    public float Salary { get; set; }
    public float Motivation { get; set; }
    public float Savings { get; set; }
    public bool HasFamily { get; set; }
    public BuyerPreferences Preferences { get; }
    public List<House> AffordableHouses { get; } = [];
    public House? WinningHouse { get; set; }
    public BuyerHouseEvaluation? SelectedEvaluation { get; internal set; }

    public bool CanAfford(House house) =>
        house.AskingPrice <= CalculateMaximumPurchasePrice();

    public float CalculateMaximumPurchasePrice()
    {
        const float depositRate = 0.20f;
        float maximumPriceFromDeposit = Savings / depositRate;
        float maximumPriceFromTotalFunds = Savings + Salary * 4f;
        return MathF.Min(maximumPriceFromDeposit, maximumPriceFromTotalFunds);
    }
}
