public class ConsoleInputReaderTests
{
    [Fact]
    public void BlankSettingsChoiceUsesDefaults()
    {
        ConsoleInputReader reader = new(new StringReader(Environment.NewLine), new StringWriter());

        SimulationSettings settings = reader.ReadSimulationSettings();

        Assert.Equal(1, settings.NewBuyersPerMonth);
        Assert.Equal(0.02f, settings.NoBidPriceReduction);
        Assert.Equal(0.50f, settings.AuctionIncrement);
    }

    [Fact]
    public void AdvancedSettingsConvertPercentagesToRates()
    {
        string input = string.Join(Environment.NewLine,
        [
            "n", "2", "3", "4", "5", "6", "0.75", "40", "25"
        ]);
        ConsoleInputReader reader = new(new StringReader(input), new StringWriter());

        SimulationSettings settings = reader.ReadSimulationSettings();

        Assert.Equal(2, settings.NewBuyersPerMonth);
        Assert.Equal(3, settings.NewHousesPerMonth);
        Assert.Equal(0.04f, settings.NoBidPriceReduction);
        Assert.Equal(0.05f, settings.MaximumMonthlyMarketValueAdjustment);
        Assert.Equal(0.06f, settings.BelowAskingBidTolerance);
        Assert.Equal(0.75f, settings.AuctionIncrement);
        Assert.Equal(0.40f, settings.RejectedBidAdjustmentRate);
        Assert.Equal(0.25f, settings.MonthlySavingsRate);
    }

    [Fact]
    public void InvalidCountsAndSeedAreExplainedAndRetried()
    {
        StringWriter output = new();
        ConsoleInputReader countReader = new(
            new StringReader($"invalid{Environment.NewLine}-1{Environment.NewLine}5"),
            output);
        ConsoleInputReader seedReader = new(
            new StringReader($"invalid{Environment.NewLine}42"),
            output);

        Assert.Equal(5, countReader.ReadNonNegativeInt("Count: "));
        Assert.Equal(42, seedReader.ReadOptionalSeed("Seed: "));
        Assert.Contains("zero or greater", output.ToString());
        Assert.Contains("whole-number seed", output.ToString());
    }

    [Fact]
    public void InvalidAdvancedValueIsRetried()
    {
        string input = string.Join(Environment.NewLine,
        [
            "n",
            "-1", "2",
            "", "", "", "", "", "", ""
        ]);
        StringWriter output = new();
        ConsoleInputReader reader = new(new StringReader(input), output);

        SimulationSettings settings = reader.ReadSimulationSettings();

        Assert.Equal(2, settings.NewBuyersPerMonth);
        Assert.Contains("zero or greater", output.ToString());
    }
}
