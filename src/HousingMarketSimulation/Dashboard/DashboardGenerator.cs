public static class DashboardGenerator
{
    private const string DataPlaceholder = "__DASHBOARD_DATA__";

    public static string Generate(
        IReadOnlyCollection<ScenarioRunResult> results,
        string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        if (results.Count == 0)
            throw new ArgumentException("At least one scenario is required.", nameof(results));

        string templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Dashboard",
            "dashboard-template.html");
        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Dashboard template was not found.", templatePath);

        string template = File.ReadAllText(templatePath);
        if (!template.Contains(DataPlaceholder, StringComparison.Ordinal))
            throw new InvalidOperationException("Dashboard template is missing its data placeholder.");

        DashboardData data = new(
            "Agent-Based Housing Market Simulation",
            results.Select(result => result.DashboardData).ToArray());
        string json = DashboardJsonExporter.Serialize(data);
        string html = template.Replace(
            DataPlaceholder,
            json,
            StringComparison.Ordinal);

        Directory.CreateDirectory(outputDirectory);
        string indexPath = Path.Combine(outputDirectory, "index.html");
        string compatibilityPath = Path.Combine(outputDirectory, "dashboard.html");
        File.WriteAllText(indexPath, html);
        File.WriteAllText(compatibilityPath, html);
        return indexPath;
    }
}
