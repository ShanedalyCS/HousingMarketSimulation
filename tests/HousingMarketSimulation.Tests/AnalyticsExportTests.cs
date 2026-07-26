using System.Text.Json;

public class AnalyticsExportTests
{
    [Fact]
    public void AnalyticsCsvHasStableHeaderAndConsistentRows()
    {
        MonthlyAnalyticsSnapshot snapshot = CreateUnavailableSnapshot();
        string path = Path.GetTempFileName();
        try
        {
            AnalyticsCsvExporter.Export([snapshot], path);
            string[] lines = File.ReadAllLines(path);

            Assert.Equal(2, lines.Length);
            Assert.Equal(29, lines[0].Split(',').Length);
            Assert.Equal(29, lines[1].Split(',').Length);
            Assert.Contains("ConstantQualityPriceIndex", lines[0]);
            Assert.Contains("PercentageBuyersCapableOfBidding", lines[0]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UnavailableAnalyticsSerializeAsJsonNull()
    {
        ScenarioDashboardData scenario = CreateDashboardScenario(
            [CreateUnavailableSnapshot()]);
        DashboardData data = new("Housing Market Simulation", [scenario]);

        string json = DashboardJsonExporter.Serialize(data);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement snapshot = document.RootElement
            .GetProperty("scenarios")[0]
            .GetProperty("monthlyAnalytics")[0];

        Assert.Equal(JsonValueKind.Null, snapshot.GetProperty("constantQualityPriceIndex").ValueKind);
        Assert.Equal(JsonValueKind.Null, snapshot.GetProperty("monthsOfSupply").ValueKind);
        Assert.DoesNotContain("NaN", json);
        Assert.DoesNotContain("Infinity", json);
    }

    [Fact]
    public void ScenarioAnalyticsJsonIsDeterministic()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            SimulationScenario scenario = ScenarioRunner.Scenarios[0] with { Months = 18 };
            ScenarioRunResult first = ScenarioRunner.Run(scenario, directory);
            string firstJson = DashboardJsonExporter.Serialize(
                new DashboardData("Housing Market Simulation", [first.DashboardData]));
            ScenarioRunResult second = ScenarioRunner.Run(scenario, directory);
            string secondJson = DashboardJsonExporter.Serialize(
                new DashboardData("Housing Market Simulation", [second.DashboardData]));

            Assert.Equal(firstJson, secondJson);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static MonthlyAnalyticsSnapshot CreateUnavailableSnapshot()
    {
        MarketAnalyticsService service = new();
        return service.CreateSnapshot(
            1,
            0,
            [],
            [],
            0,
            [],
            [],
            [],
            new AffordabilityObservation(null, null, null));
    }

    private static ScenarioDashboardData CreateDashboardScenario(
        IReadOnlyList<MonthlyAnalyticsSnapshot> snapshots)
    {
        ScenarioSummary summary = new(
            "Test",
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0);
        return new ScenarioDashboardData(
            "test",
            "Test",
            1,
            1,
            0,
            0,
            0,
            0,
            summary,
            [],
            snapshots);
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"housing-analytics-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
