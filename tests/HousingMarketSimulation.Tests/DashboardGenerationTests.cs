[Collection(ConsoleSensitiveCollection.Name)]
public class DashboardGenerationTests
{
    [Fact]
    public void DashboardEmbedsAllScenariosAndRequiredCharts()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            List<ScenarioRunResult> results = [];
            foreach (SimulationScenario definition in ScenarioRunner.Scenarios)
            {
                SimulationScenario shortScenario = definition with
                {
                    Months = 3,
                    InitialBuyers = 8,
                    InitialHouses = 6
                };
                results.Add(ScenarioRunner.Run(shortScenario, directory));
            }

            string path = DashboardGenerator.Generate(results, directory);
            string html = File.ReadAllText(path);

            Assert.True(File.Exists(path));
            Assert.DoesNotContain("__DASHBOARD_DATA__", html);
            Assert.DoesNotContain("https://", html);
            Assert.Contains("Balanced market", html);
            Assert.Contains("Excess-demand market", html);
            Assert.Contains("Excess-supply market", html);
            Assert.Contains("id=\"quality-index-chart\"", html);
            Assert.Contains("id=\"raw-price-chart\"", html);
            Assert.Contains("id=\"supply-demand-chart\"", html);
            Assert.Contains("id=\"market-activity-chart\"", html);
            Assert.Contains("id=\"liquidity-chart\"", html);
            Assert.Contains("id=\"scenario-comparison-chart\"", html);
            Assert.Contains("id=\"location-analysis\"", html);
            Assert.Contains("constantQualityPriceIndex", html);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ScenarioRunnerCreatesDocumentedOutputTree()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            IReadOnlyList<ScenarioRunResult> results =
                ScenarioRunner.RunAll(directory, TextWriter.Null);

            foreach (ScenarioRunResult result in results)
            {
                Assert.Equal(
                    Path.Combine(directory, result.Scenario.Id, "monthly-market-reports.csv"),
                    result.CsvPath);
                Assert.True(File.Exists(result.CsvPath));
                Assert.True(File.Exists(result.AnalyticsCsvPath));
                Assert.True(File.Exists(result.DashboardJsonPath));
            }
            Assert.True(File.Exists(Path.Combine(directory, "scenario-comparison.json")));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"housing-dashboard-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
