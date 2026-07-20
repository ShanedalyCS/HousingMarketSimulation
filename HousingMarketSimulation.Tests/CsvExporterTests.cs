using System.Globalization;

public class CsvExporterTests
{
    [Fact]
    public void ExportUsesInvariantDecimalSeparatorAndOneRowPerReport()
    {
        MonthlyMarketReport report = new(
            month: 1,
            buyersActiveDuringMonth: 2,
            housesActiveDuringMonth: 3,
            bidsPlaced: 2,
            transactionsCompleted: 1,
            priceReductions: 1,
            priceIncreases: 0,
            priceDiscoveries: 0,
            buyersRemaining: 1,
            housesRemaining: 2,
            averageAskingPrice: 98.5f,
            averageSalePrice: 101.25f,
            startingAverageAskingPrice: 100,
            askingPriceChange: -1.5f,
            askingPricePercentageChange: -1.5f);
        string filePath = Path.GetTempFileName();

        try
        {
            MonthlyReportCsvExporter.Export([report], filePath);
            string[] lines = File.ReadAllLines(filePath);

            Assert.Equal(2, lines.Length);
            Assert.Contains("AverageAskingPrice", lines[0]);
            Assert.Contains("98.5,101.25", lines[1]);
            Assert.DoesNotContain(98.5f.ToString(new CultureInfo("de-DE")), lines[1]);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
