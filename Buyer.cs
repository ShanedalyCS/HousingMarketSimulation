public class Buyer(string name, int age, float salary, float motivation, float savings, bool hasFamily)
{
    private string name = name;
    public int age = age;
    public float salary = salary;
    public float motivation = motivation;
    public float savings = savings;
    public bool hasFamily = hasFamily;

    public List<House> affordableHouses = [];

    public House? winningHouse;

    public List<House> AffordableHouses
    {
        get
        {
            return affordableHouses;
        }

    }

    public House? WinningHouse
    {
        get { return winningHouse; }
        set { this.winningHouse = value; }
    }

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    public int Age
    {
        get
        {
            return age;
        }
        set
        {
            age = value;
        }
    }

    public float Salary
    {
        get
        {
            return salary;
        }
        set
        {
            salary = value;
        }
    }

    public float Motivation
    {
        get
        {
            return motivation;
        }
        set
        {
            motivation = value;
        }
    }

    public float Savings
    {
        get
        {
            return savings;
        }
        set
        {
            savings = value;
        }
    }

    public bool HasFamily
    {
        get
        {
            return hasFamily;
        }
        set
        {
            hasFamily = value;
        }
    }

    public bool CanAfford(House house)
    {
        float minimumDeposit = house.value * 0.2f;
        float maximumMortgage = salary * 4;

        if (savings >= minimumDeposit && maximumMortgage >= house.value) return true;
        return false;
    }
}
