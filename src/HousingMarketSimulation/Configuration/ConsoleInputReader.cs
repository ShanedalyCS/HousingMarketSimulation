using System.Globalization;

public sealed class ConsoleInputReader(TextReader input, TextWriter output)
{
    public int ReadNonNegativeInt(string prompt)
    {
        while (true)
        {
            output.Write(prompt);
            string text = input.ReadLine()?.Trim() ?? string.Empty;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value >= 0)
            {
                return value;
            }

            output.WriteLine("Enter a whole number of zero or greater.");
        }
    }

    public int? ReadOptionalSeed(string prompt)
    {
        while (true)
        {
            output.Write(prompt);
            string text = input.ReadLine()?.Trim() ?? string.Empty;
            if (text.Length == 0) return null;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int seed))
                return seed;
            output.WriteLine("Enter a whole-number seed, or press Enter for a random run.");
        }
    }

    public SimulationSettings ReadSimulationSettings()
    {
        SimulationSettings defaults = new();
        bool useDefaults = ReadUseDefaults();
        if (useDefaults)
        {
            defaults.Validate();
            return defaults;
        }

        SimulationSettings settings = new()
        {
            NewBuyersPerMonth = ReadIntWithDefault(
                "New buyers per month", defaults.NewBuyersPerMonth),
            NewHousesPerMonth = ReadIntWithDefault(
                "New houses per month", defaults.NewHousesPerMonth),
            NoBidPriceReduction = ReadPercentageWithDefault(
                "No-bid price reduction", defaults.NoBidPriceReduction),
            MaximumMonthlyMarketValueAdjustment = ReadPercentageWithDefault(
                "Maximum monthly market-value adjustment",
                defaults.MaximumMonthlyMarketValueAdjustment),
            BelowAskingBidTolerance = ReadPercentageWithDefault(
                "Below-asking bid tolerance", defaults.BelowAskingBidTolerance),
            AuctionIncrement = ReadFloatWithDefault(
                "Auction increment (K)", defaults.AuctionIncrement),
            RejectedBidAdjustmentRate = ReadPercentageWithDefault(
                "Rejected-bid adjustment", defaults.RejectedBidAdjustmentRate),
            MonthlySavingsRate = ReadPercentageWithDefault(
                "Monthly savings rate", defaults.MonthlySavingsRate)
        };
        settings.Validate();
        return settings;
    }

    private bool ReadUseDefaults()
    {
        while (true)
        {
            output.Write("Use default simulation settings? (y/n) [y]: ");
            string text = input.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;
            if (text is "" or "y" or "yes") return true;
            if (text is "n" or "no") return false;
            output.WriteLine("Enter y or n, or press Enter to use defaults.");
        }
    }

    private int ReadIntWithDefault(string label, int defaultValue)
    {
        while (true)
        {
            output.Write($"{label} [{defaultValue}]: ");
            string text = input.ReadLine()?.Trim() ?? string.Empty;
            if (text.Length == 0) return defaultValue;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value >= 0)
            {
                return value;
            }
            output.WriteLine("Enter a whole number of zero or greater.");
        }
    }

    private float ReadPercentageWithDefault(string label, float defaultRate)
    {
        float defaultPercentage = defaultRate * 100f;
        while (true)
        {
            output.Write($"{label} percentage [{defaultPercentage:G}%]: ");
            string text = input.ReadLine()?.Trim() ?? string.Empty;
            if (text.Length == 0) return defaultRate;
            if (TryParseFloat(text, out float value) && value is >= 0 and <= 100)
                return value / 100f;
            output.WriteLine("Enter a percentage from 0 to 100.");
        }
    }

    private float ReadFloatWithDefault(string label, float defaultValue)
    {
        while (true)
        {
            output.Write($"{label} [{defaultValue:G}]: ");
            string text = input.ReadLine()?.Trim() ?? string.Empty;
            if (text.Length == 0) return defaultValue;
            if (TryParseFloat(text, out float value) && value >= 0) return value;
            output.WriteLine("Enter a finite number of zero or greater.");
        }
    }

    private static bool TryParseFloat(string text, out float value) =>
        float.TryParse(
            text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value)
        && float.IsFinite(value);
}
