public sealed record LocationAnalytics(
    LocationDesirability Location,
    float? ConstantQualityPriceIndex,
    int TransactionsInIndexWindow,
    int TransactionsThisMonth,
    float? MedianSalePrice,
    float? AverageTimeOnMarket,
    float? AverageSaleToListRatio);

public sealed record MonthlyAnalyticsSnapshot(
    int Month,
    int NewListings,
    float? RawAverageAskingPrice,
    float? RawMedianAskingPrice,
    float? RawAverageSalePrice,
    float? RawMedianSalePrice,
    float? ConstantQualityPriceIndex,
    IReadOnlyList<LocationAnalytics> LocationAnalytics,
    float? AverageSaleToListRatio,
    int ActiveBuyers,
    int ActiveListings,
    float? BuyerToListingRatio,
    int BidsSubmitted,
    float? BidsPerActiveListing,
    int Transactions,
    float? ClearanceRate,
    int EndingInventory,
    float? MonthsOfSupply,
    float? AverageTimeOnMarket,
    float? MedianTimeOnMarket,
    float? PercentageActiveListingsSold,
    float? PercentageActiveBuyersPurchasing,
    float TotalTransactionValue,
    float? MedianBuyerMaximumPurchasePrice,
    float? AskingPriceToPurchasingPowerRatio,
    float? PercentageBuyersCapableOfBidding);

public sealed record AffordabilityObservation(
    float? MedianBuyerMaximumPurchasePrice,
    float? AskingPriceToPurchasingPowerRatio,
    float? PercentageBuyersCapableOfBidding);
