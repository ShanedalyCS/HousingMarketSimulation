[Collection(ConsoleSensitiveCollection.Name)]
public class LiveSimulationSessionTests
{
    [Fact]
    public void SessionAdvancesExactlyOneConfiguredMonthAtATime()
    {
        LiveSimulationSession session = new(new LiveSimulationConfiguration(
            InitialBuyers: 8,
            InitialHouses: 6,
            DurationMonths: 2,
            Seed: 123,
            NewBuyersPerMonth: 0,
            NewHousesPerMonth: 0));

        Assert.True(session.AdvanceOneMonth());
        Assert.Equal(1, session.CurrentMonth);
        Assert.Single(session.Market.AnalyticsSnapshots);
        Assert.False(session.IsComplete);

        Assert.True(session.AdvanceOneMonth());
        Assert.Equal(2, session.CurrentMonth);
        Assert.Equal(2, session.Market.AnalyticsSnapshots.Count);
        Assert.True(session.IsComplete);
        Assert.False(session.AdvanceOneMonth());
    }

    [Fact]
    public void IdenticalLiveSessionsRemainDeterministic()
    {
        LiveSimulationConfiguration configuration = new(
            InitialBuyers: 12,
            InitialHouses: 10,
            DurationMonths: 4,
            Seed: 456);
        LiveSimulationSession first = new(configuration);
        LiveSimulationSession second = new(configuration);

        for (int month = 0; month < configuration.DurationMonths; month++)
        {
            first.AdvanceOneMonth();
            second.AdvanceOneMonth();
        }

        Assert.Equal(
            System.Text.Json.JsonSerializer.Serialize(first.Market.AnalyticsSnapshots),
            System.Text.Json.JsonSerializer.Serialize(second.Market.AnalyticsSnapshots));
        Assert.Equal(
            first.Market.Transactions.Select(transaction => (
                transaction.Month,
                transaction.House.Name,
                transaction.SalePrice)),
            second.Market.Transactions.Select(transaction => (
                transaction.Month,
                transaction.House.Name,
                transaction.SalePrice)));
    }

    [Theory]
    [InlineData(-1, 1, 1)]
    [InlineData(1, -1, 1)]
    [InlineData(1, 1, 0)]
    public void ConfigurationRejectsInvalidCounts(
        int buyers,
        int houses,
        int months)
    {
        LiveSimulationConfiguration configuration = new(
            buyers,
            houses,
            months,
            Seed: 1);

        Assert.ThrowsAny<ArgumentException>(configuration.Validate);
    }
}
