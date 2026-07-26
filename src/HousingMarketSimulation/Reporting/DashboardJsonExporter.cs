using System.Text.Json;
using System.Text.Json.Serialization;

public static class DashboardJsonExporter
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static void Export(DashboardData data, string filePath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ValidateFinite(data);

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, Serialize(data));
    }

    public static string Serialize(DashboardData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateFinite(data);
        return JsonSerializer.Serialize(data, Options);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static void ValidateFinite(DashboardData data)
    {
        foreach (ScenarioDashboardData scenario in data.Scenarios)
        {
            ValidateFinite(scenario.Summary.StartingAverageAskingPrice);
            ValidateFinite(scenario.Summary.EndingAverageAskingPrice);
            ValidateFinite(scenario.Summary.AskingPricePercentageChange);
            ValidateFinite(scenario.Summary.AverageSaleToListRatio);
            ValidateFinite(scenario.Summary.AverageTimeOnMarket);
            ValidateFinite(scenario.Summary.TotalTransactionValue);
            foreach (ScenarioLocationAnalytics location in scenario.LocationAnalytics)
            {
                ValidateFinite(location.ConstantQualityPriceIndex);
                ValidateFinite(location.MedianSalePrice);
                ValidateFinite(location.AverageTimeOnMarket);
                ValidateFinite(location.AverageSaleToListRatio);
            }
            foreach (MonthlyAnalyticsSnapshot snapshot in scenario.MonthlyAnalytics)
            {
                ValidateFinite(snapshot.RawAverageAskingPrice);
                ValidateFinite(snapshot.RawMedianAskingPrice);
                ValidateFinite(snapshot.RawAverageSalePrice);
                ValidateFinite(snapshot.RawMedianSalePrice);
                ValidateFinite(snapshot.ConstantQualityPriceIndex);
                ValidateFinite(snapshot.AverageSaleToListRatio);
                ValidateFinite(snapshot.BuyerToListingRatio);
                ValidateFinite(snapshot.BidsPerActiveListing);
                ValidateFinite(snapshot.ClearanceRate);
                ValidateFinite(snapshot.MonthsOfSupply);
                ValidateFinite(snapshot.AverageTimeOnMarket);
                ValidateFinite(snapshot.MedianTimeOnMarket);
                ValidateFinite(snapshot.PercentageActiveListingsSold);
                ValidateFinite(snapshot.PercentageActiveBuyersPurchasing);
                ValidateFinite(snapshot.TotalTransactionValue);
                ValidateFinite(snapshot.MedianBuyerMaximumPurchasePrice);
                ValidateFinite(snapshot.AskingPriceToPurchasingPowerRatio);
                ValidateFinite(snapshot.PercentageBuyersCapableOfBidding);
                foreach (LocationAnalytics location in snapshot.LocationAnalytics)
                {
                    ValidateFinite(location.ConstantQualityPriceIndex);
                    ValidateFinite(location.MedianSalePrice);
                    ValidateFinite(location.AverageTimeOnMarket);
                    ValidateFinite(location.AverageSaleToListRatio);
                }
            }
        }
    }

    private static void ValidateFinite(float? value)
    {
        if (value.HasValue && !float.IsFinite(value.Value))
            throw new InvalidOperationException("Dashboard data contains a non-finite number.");
    }
}
