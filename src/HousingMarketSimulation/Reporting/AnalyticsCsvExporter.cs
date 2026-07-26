using System.Globalization;
using System.Text;

public static class AnalyticsCsvExporter
{
    public static void Export(
        IEnumerable<MonthlyAnalyticsSnapshot> snapshots,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        StringBuilder csv = new();
        csv.AppendLine(
            "Month,NewListings,RawAverageAskingPrice,RawMedianAskingPrice," +
            "RawAverageSalePrice,RawMedianSalePrice,ConstantQualityPriceIndex," +
            "LowLocationPriceIndex,AverageLocationPriceIndex,HighLocationPriceIndex," +
            "PrimeLocationPriceIndex,AverageSaleToListRatio,ActiveBuyers,ActiveListings," +
            "BuyerToListingRatio,BidsSubmitted,BidsPerActiveListing,Transactions," +
            "ClearanceRate,EndingInventory,MonthsOfSupply,AverageTimeOnMarket," +
            "MedianTimeOnMarket,PercentageActiveListingsSold," +
            "PercentageActiveBuyersPurchasing,TotalTransactionValue," +
            "MedianBuyerMaximumPurchasePrice,AskingPriceToPurchasingPowerRatio," +
            "PercentageBuyersCapableOfBidding");

        foreach (MonthlyAnalyticsSnapshot snapshot in snapshots)
        {
            Dictionary<LocationDesirability, LocationAnalytics> locations =
                snapshot.LocationAnalytics.ToDictionary(item => item.Location);
            string[] values =
            [
                Format(snapshot.Month),
                Format(snapshot.NewListings),
                Format(snapshot.RawAverageAskingPrice),
                Format(snapshot.RawMedianAskingPrice),
                Format(snapshot.RawAverageSalePrice),
                Format(snapshot.RawMedianSalePrice),
                Format(snapshot.ConstantQualityPriceIndex),
                Format(locations[LocationDesirability.Low].ConstantQualityPriceIndex),
                Format(locations[LocationDesirability.Average].ConstantQualityPriceIndex),
                Format(locations[LocationDesirability.High].ConstantQualityPriceIndex),
                Format(locations[LocationDesirability.Prime].ConstantQualityPriceIndex),
                Format(snapshot.AverageSaleToListRatio),
                Format(snapshot.ActiveBuyers),
                Format(snapshot.ActiveListings),
                Format(snapshot.BuyerToListingRatio),
                Format(snapshot.BidsSubmitted),
                Format(snapshot.BidsPerActiveListing),
                Format(snapshot.Transactions),
                Format(snapshot.ClearanceRate),
                Format(snapshot.EndingInventory),
                Format(snapshot.MonthsOfSupply),
                Format(snapshot.AverageTimeOnMarket),
                Format(snapshot.MedianTimeOnMarket),
                Format(snapshot.PercentageActiveListingsSold),
                Format(snapshot.PercentageActiveBuyersPurchasing),
                Format(snapshot.TotalTransactionValue),
                Format(snapshot.MedianBuyerMaximumPurchasePrice),
                Format(snapshot.AskingPriceToPurchasingPowerRatio),
                Format(snapshot.PercentageBuyersCapableOfBidding)
            ];
            csv.AppendLine(string.Join(',', values));
        }

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }

    private static string Format(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private static string Format(float value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private static string Format(float? value) =>
        value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
}
