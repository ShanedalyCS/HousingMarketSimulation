public class House
{
    private string name;

    public float value;

    public float quality; // will be made up of a score of the four variables below. 
    public float technology;
    public float age;
    public float area;
    public float size;

    public int numberOfBids;

    public List<Bid> bids;

    public House(string name, float value, float technology, float age, float area, float size)
    {
        this.name = name;
        this.value = value;
        this.technology = technology;
        this.age = age;
        this.area = area;
        this.size = size;

        this.quality = technology + age + area + size;

        this.numberOfBids = 0;
        this.bids = [];

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

    public float Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
        }
    }

    public float Quality
    {
        get
        {
            return quality;
        }
    }

    public float Technology
    {
        get
        {
            return technology;
        }
        set
        {
            technology = value;
        }
    }

    public float Age
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

    public float Area
    {
        get
        {
            return area;
        }
        set
        {
            area = value;
        }
    }

    public float Size
    {
        get
        {
            return size;
        }
        set
        {
            size = value;
        }
    }

    public string PrintAll()
    {
        return ("name : " + name);
    }

    // to use getters and setters in c# : string name = person.Name; for getters. person.Age = 29; for setters.
}