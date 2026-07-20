using System.Globalization;
using System.Text;

public static class MonthlyReportCsvExporter
{
    public static void Export(IEnumerable<MonthlyMarketReport> reports, string filePath)
    {
        StringBuilder csv = new();
        csv.AppendLine(
            "Month,BuyersActiveDuringMonth,HousesActiveDuringMonth,BidsPlaced," +
            "TransactionsCompleted,PriceReductions,PriceIncreases,PriceDiscoveries," +
            "BuyersRemaining,HousesRemaining,AverageAskingPrice,AverageSalePrice," +
            "StartingAverageAskingPrice,AskingPriceChange,AskingPricePercentageChange");

        foreach (MonthlyMarketReport report in reports)
        {
            string[] values =
            [
                report.Month.ToString(CultureInfo.InvariantCulture),
                report.BuyersActiveDuringMonth.ToString(CultureInfo.InvariantCulture),
                report.HousesActiveDuringMonth.ToString(CultureInfo.InvariantCulture),
                report.BidsPlaced.ToString(CultureInfo.InvariantCulture),
                report.TransactionsCompleted.ToString(CultureInfo.InvariantCulture),
                report.PriceReductions.ToString(CultureInfo.InvariantCulture),
                report.PriceIncreases.ToString(CultureInfo.InvariantCulture),
                report.PriceDiscoveries.ToString(CultureInfo.InvariantCulture),
                report.BuyersRemaining.ToString(CultureInfo.InvariantCulture),
                report.HousesRemaining.ToString(CultureInfo.InvariantCulture),
                report.AverageAskingPrice.ToString(CultureInfo.InvariantCulture),
                report.AverageSalePrice.ToString(CultureInfo.InvariantCulture),
                report.StartingAverageAskingPrice.ToString(CultureInfo.InvariantCulture),
                report.AskingPriceChange.ToString(CultureInfo.InvariantCulture),
                report.AskingPricePercentageChange.ToString(CultureInfo.InvariantCulture)
            ];

            csv.AppendLine(string.Join(',', values));
        }

        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }
}
