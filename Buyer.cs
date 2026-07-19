public class Buyer
{
    private string name;
    public int age;
    public float salary;
    public float motivation;
    public float savings;
    public bool hasFamily;

    public List<House> affordableHouses;

    public Buyer(string name, int age, float salary, float motivation, float savings, bool hasFamily)
    {
        this.name = name;
        this.age = age;
        this.salary = salary;
        this.motivation = motivation;
        this.savings = savings;
        this.hasFamily = hasFamily;

        this.affordableHouses = [];
    }

    public List<House> AffordableHouses
    {
        get
        {
            return affordableHouses;
        }

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

    // to use getters and setters in c# : string name = person.Name; for getters. person.Age = 29; for setters.
}