public class PriceIndexCalculatorTests
{
    [Fact]
    public void CalculatesMedianSaleToBaseFactor()
    {
        PriceIndexCalculator calculator = CreateCalculator(minimumOverall: 1);
        Transaction[] transactions =
        [
            CreateTransaction(0.8f, 1),
            CreateTransaction(1.2f, 1),
            CreateTransaction(1.0f, 1)
        ];

        Assert.Equal(1f, calculator.CalculateMedianMarketFactor(transactions));
    }

    [Fact]
    public void FirstAdequatelySampledPeriodRebasesToOneHundred()
    {
        PriceIndexCalculator calculator = CreateCalculator(minimumOverall: 2);

        PriceIndexResult result = calculator.Calculate(
            1,
            [CreateTransaction(0.9f, 1), CreateTransaction(1.1f, 1)]);

        Assert.Equal(100f, result.OverallIndex);
    }

    [Fact]
    public void RollingWindowExcludesOlderTransactions()
    {
        PriceIndexCalculator calculator = CreateCalculator(
            minimumOverall: 1,
            windowMonths: 2);
        Transaction[] transactions =
        [
            CreateTransaction(1f, 1),
            CreateTransaction(1f, 2),
            CreateTransaction(2f, 3)
        ];

        Assert.Equal(100f, calculator.Calculate(1, transactions).OverallIndex);
        Assert.Equal(150f, calculator.Calculate(3, transactions).OverallIndex);
    }

    [Fact]
    public void InsufficientSampleReturnsNull()
    {
        PriceIndexCalculator calculator = CreateCalculator(minimumOverall: 2);

        Assert.Null(calculator.Calculate(1, [CreateTransaction(1f, 1)]).OverallIndex);
    }

    [Fact]
    public void InvalidZeroBaseValueIsExcluded()
    {
        House zeroValueHouse = TestHouseFactory.Create(
            0,
            floorArea: 0,
            plotSize: 0,
            age: 0);
        Transaction transaction = new(
            CreateBuyer(),
            zeroValueHouse,
            100,
            month: 1);
        PriceIndexCalculator calculator = CreateCalculator(minimumOverall: 1);

        PriceIndexResult result = calculator.Calculate(1, [transaction]);

        Assert.Null(result.OverallIndex);
        Assert.Equal(0, result.OverallTransactionCount);
    }

    [Fact]
    public void ExtremeRatiosAreBounded()
    {
        PriceIndexCalculator calculator = new(new AnalyticsSettings
        {
            MinimumOverallTransactions = 1,
            MinimumLocationTransactions = 1,
            MinimumSaleToBaseRatio = 0.5f,
            MaximumSaleToBaseRatio = 2f
        });

        Assert.Equal(
            2f,
            calculator.CalculateMedianMarketFactor([CreateTransaction(100f, 1)]));
    }

    [Fact]
    public void SameRatioProducesSameFactorForDifferentlyPricedHouses()
    {
        Transaction small = CreateTransaction(1.15f, 1, floorArea: 80, plotSize: 100);
        Transaction large = CreateTransaction(1.15f, 1, floorArea: 220, plotSize: 700);
        PriceIndexCalculator calculator = CreateCalculator(minimumOverall: 1);

        Assert.Equal(
            1.15f,
            calculator.CalculateMedianMarketFactor([small, large])!.Value,
            precision: 4);
    }

    [Fact]
    public void LocationIndicesAreIndependentAndMissingSegmentsRemainNull()
    {
        PriceIndexCalculator calculator = CreateCalculator(
            minimumOverall: 2,
            minimumLocation: 1);
        Transaction[] firstMonth =
        [
            CreateTransaction(1f, 1, location: LocationDesirability.Low),
            CreateTransaction(2f, 1, location: LocationDesirability.Prime)
        ];
        _ = calculator.Calculate(1, firstMonth);
        PriceIndexResult second = calculator.Calculate(
            2,
            [
                .. firstMonth,
                CreateTransaction(1.5f, 2, location: LocationDesirability.Low)
            ]);

        Assert.Equal(125f, second.LocationIndices[LocationDesirability.Low]);
        Assert.Equal(100f, second.LocationIndices[LocationDesirability.Prime]);
        Assert.Null(second.LocationIndices[LocationDesirability.Average]);
        Assert.Null(second.LocationIndices[LocationDesirability.High]);
    }

    [Fact]
    public void PropertyMixCanLowerRawAverageWhileIndexRemainsConstant()
    {
        AnalyticsSettings settings = new()
        {
            MinimumOverallTransactions = 1,
            MinimumLocationTransactions = 1
        };
        MarketAnalyticsService service = new(settings);
        Transaction expensive = CreateTransaction(
            1f, 1, floorArea: 220, plotSize: 700);
        Transaction inexpensive = CreateTransaction(
            1f, 2, floorArea: 70, plotSize: 100);
        AffordabilityObservation affordability = new(null, null, null);

        MonthlyAnalyticsSnapshot first = service.CreateSnapshot(
            1, 0, [], [], 0, [expensive], [], [expensive], affordability);
        MonthlyAnalyticsSnapshot second = service.CreateSnapshot(
            2, 0, [], [], 0, [inexpensive], [], [expensive, inexpensive], affordability);

        Assert.True(second.RawAverageSalePrice < first.RawAverageSalePrice);
        Assert.Equal(100f, first.ConstantQualityPriceIndex);
        Assert.Equal(100f, second.ConstantQualityPriceIndex);
    }

    private static PriceIndexCalculator CreateCalculator(
        int minimumOverall,
        int minimumLocation = 1,
        int windowMonths = 12) =>
        new(new AnalyticsSettings
        {
            MinimumOverallTransactions = minimumOverall,
            MinimumLocationTransactions = minimumLocation,
            PriceIndexWindowMonths = windowMonths
        });

    private static Transaction CreateTransaction(
        float ratio,
        int month,
        LocationDesirability location = LocationDesirability.Average,
        float floorArea = 120,
        float plotSize = 300)
    {
        House house = TestHouseFactory.Create(
            100,
            floorArea: floorArea,
            plotSize: plotSize,
            location: location);
        return new Transaction(
            CreateBuyer(),
            house,
            house.BaseValue * ratio,
            month);
    }

    private static Buyer CreateBuyer() =>
        new("Buyer", 35, 200, 5, 200, false);
}
