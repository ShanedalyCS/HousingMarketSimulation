public class SettingsValidationTests
{
    [Fact]
    public void SimulationSettingsRejectInvalidValues()
    {
        SimulationSettings[] invalidSettings =
        [
            new() { NewBuyersPerMonth = -1 },
            new() { NewHousesPerMonth = -1 },
            new() { NoBidPriceReduction = -0.01f },
            new() { MaximumMonthlyMarketValueAdjustment = 1.01f },
            new() { BelowAskingBidTolerance = float.NaN },
            new() { AuctionIncrement = -1 },
            new() { RejectedBidAdjustmentRate = float.PositiveInfinity },
            new() { MonthlySavingsRate = 1.01f }
        ];

        Assert.All(invalidSettings, settings =>
            Assert.ThrowsAny<ArgumentException>(settings.Validate));
    }

    [Fact]
    public void ValuationSettingsRejectInvalidScalarConfiguration()
    {
        ValuationSettings[] invalidSettings =
        [
            new() { ConstructionCostPerSquareMetre = -1 },
            new() { BaseLandPricePerSquareMetre = -1 },
            new() { ComparablesPerFullWeight = 0 },
            new() { MinimumComparableSimilarity = 0 },
            new() { MaximumFloorAreaDifferenceRatio = 0 },
            new() { MaximumHouseAgeDifference = 0 },
            new() { MinimumSellerPriceMultiplier = 2, MaximumSellerPriceMultiplier = 1 },
            new() { MinimumComparablePriceRatio = 2, MaximumComparablePriceRatio = 1 }
        ];

        Assert.All(invalidSettings, settings =>
            Assert.ThrowsAny<ArgumentException>(settings.Validate));
    }

    [Fact]
    public void ValuationSettingsRejectZeroSimilarityWeight()
    {
        ValuationSettings settings = new()
        {
            LocationSimilarityWeight = 0,
            FloorAreaSimilarityWeight = 0,
            BuildQualitySimilarityWeight = 0,
            HouseAgeSimilarityWeight = 0
        };

        ArgumentException exception = Assert.Throws<ArgumentException>(settings.Validate);
        Assert.Contains(nameof(ValuationSettings.LocationSimilarityWeight), exception.ParamName);
    }

    [Fact]
    public void ValuationSettingsRejectMissingEnumMultiplier()
    {
        ValuationSettings settings = new()
        {
            QualityMultipliers = new Dictionary<PropertyQuality, float>
            {
                [PropertyQuality.Basic] = 0.8f,
                [PropertyQuality.Standard] = 1f
            }
        };

        ArgumentException exception = Assert.Throws<ArgumentException>(settings.Validate);
        Assert.Equal(nameof(ValuationSettings.QualityMultipliers), exception.ParamName);
    }

    [Fact]
    public void ValuationServiceValidatesSettingsOnConstruction()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new HouseValuationService(new ValuationSettings
            {
                MaximumComparablePriceRatio = float.NaN
            }));

        Assert.Equal(
            nameof(ValuationSettings.MaximumComparablePriceRatio),
            exception.ParamName);
    }
}
