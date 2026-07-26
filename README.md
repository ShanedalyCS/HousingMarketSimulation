# Housing Market Simulation

A seeded, monthly agent-style housing market written in C#. Buyers have different
preferences, form their own willingness to pay, and submit one sealed bid per
month. Completed sales feed back into comparable valuations and future seller
prices.

All monetary values are expressed in thousands. For example, `100 K` represents
100,000 in the simulated currency.

## Monthly lifecycle

Each month runs in this order:

1. Clear previous bidding and buyer-selection state.
2. Add 20% of monthly salary to each active buyer's savings.
3. Increment `MonthsOnMarket` for every active listing.
4. Recalculate each listing's `EstimatedMarketValue` from completed sales.
5. Move asking prices gradually toward each seller's market-informed target.
6. Let buyers evaluate listings using their personal preferences.
7. Submit at most one sealed, affordability-capped bid per buyer.
8. Settle each house's auction.
9. Record transactions and remove sold houses and successful buyers.
10. Reduce or market-adjust unsuccessful listings.
11. Record the monthly report.
12. Add configured new entrants. New houses use all transactions completed
    through step 9 immediately, but participate beginning next month.

The default entrant counts are one buyer and one house per month.

## Values and market feedback

The model deliberately keeps four monetary concepts separate:

- `BaseValue` is the immutable fundamental cost valuation.
- `EstimatedMarketValue` blends the fundamental value with similar completed
  sales.
- `AskingPrice` is the seller's current list price.
- `SalePrice` is the auction settlement recorded in a transaction.

The cost valuation is:

```text
BaseValue = LandValue + ReplacementCost - AgeDepreciation
```

Land starts at `0.35 K` per plot square metre and uses a location multiplier.
Replacement cost is floor area times `1.8 K`, adjusted for build quality.
Replacement cost depreciates by 1% per year, capped at 60%.

Each new house receives one seeded seller multiplier from 0.95 to 1.15. The
multiplier persists for the entire listing; it is not redrawn monthly. Its
market-informed target is:

```text
seller target = EstimatedMarketValue × persistent seller multiplier
```

The asking price moves toward that target in either direction, capped by the
configured maximum monthly adjustment (3% by default). This avoids abrupt resets.

### Comparable sales

Comparables are scored using location, floor-area similarity, build quality, and
house age. Locations two or more categories apart are excluded. Remaining sales
must meet the configured weighted-similarity threshold.

The service applies the comparable's bounded sale-price-to-base-value ratio to
the subject's own `BaseValue`. Similar transactions receive more weight. Total
comparable influence grows with sample size and is capped at 70%; a single
perfect comparable receives only 14% weight by default. Individual price ratios
are bounded between 0.60 and 1.60, limiting the impact of an outlier. When there
are no suitable comparables, the fundamental value is the fallback.

Because buyers form perceived values from `EstimatedMarketValue`, transaction
history affects future asking prices, buyer choices, maximum bids, and eventual
sale prices.

## Buyer decisions

Generated buyers are at least 18 years old. Salary, savings, motivation, family
status, and preferences all vary using the supplied seeded `Random`. A family is
not assigned to every buyer; family-oriented generated preferences give more
weight to floor and plot area.

Every buyer has normalized weights for:

- location desirability;
- build quality;
- floor area;
- plot size;
- lower house age.

For each listing the decision service calculates two separate values:

```text
suitability = weighted normalized physical/location features
perceived value = EstimatedMarketValue × (0.80 + 0.40 × suitability)
motivated value = perceived value × (1 + motivation / 200)
maximum bid = min(motivated value, affordability limit)
```

Suitability is unitless and drives 70% of the ranking. Value for money, derived
from perceived value relative to asking price, drives 30%. These are kept
separate because a preference-fit score and money are not interchangeable units.

A buyer will consider a below-asking bid only when their maximum bid is within
the configured tolerance (5% by default). Otherwise they do not bid. Equal
ranking scores are resolved using the shared seeded random source.

### Affordability

The maximum purchase price remains:

```text
minimum of:
    savings / 20% deposit rate
    savings + (annual salary × 4)
```

Willingness to pay and every submitted bid are capped by this limit.

## Sealed-bid settlement

Each buyer can submit one bid and complete at most one purchase in a month.
Auctions are processed with this rule:

1. If the highest available bid is below asking, no sale occurs.
2. With one bid at or above asking, that bidder wins and pays the asking price.
3. With multiple qualifying bids, the highest bidder wins and pays the lower of:
   - their own maximum bid; or
   - the greater of asking price and the second-highest bid plus the configured
     increment (`0.50 K` by default).
4. Equal highest bids use a seeded lottery.

Thus a winner never pays above their submitted maximum, and valid demand is not
rejected merely because a listing receives several bids.

## Unsuccessful listings

- No bids: reduce asking price by 2% by default.
- One or more bids, all below asking: move halfway toward the highest rejected
  bid by default, still subject to the monthly movement cap.
- Successful sale: remove the listing and use its transaction as a future
  comparable.

Upward movement comes from successful sale evidence, comparable estimates, and
seller targets rather than an artificial bid-count branch.

## Configuration

`SimulationSettings` contains:

- new buyers and houses per month;
- no-bid reduction;
- maximum monthly market-value adjustment;
- below-asking tolerance;
- auction increment;
- rejected-bid adjustment rate;
- monthly savings rate.

`ValuationSettings` contains the cost inputs, seller multiplier range,
comparable weights and similarity threshold, similarity component weights,
sample-size cap, and outlier bounds. No dependency-injection framework is
required; settings can be passed directly to the services or `Simulation`.
Both settings types reject non-finite values, invalid rate ranges, inconsistent
bounds, and incomplete multiplier dictionaries instead of silently correcting
them.

Interactive mode offers a quick default path:

```text
Use default simulation settings? (y/n) [y]:
```

Press Enter or enter `y` to use the documented defaults. Enter `n` to configure
monthly entrant counts, no-bid reduction, maximum market-value movement,
below-asking tolerance, auction increment, rejected-bid adjustment, and monthly
savings. Every prompt shows its default; Enter accepts it. Percentages are
entered as user-facing values from 0 to 100 and converted to decimal rates.
Invalid text or out-of-range input is explained and prompted again.

## Monthly reports and CSV snapshots

Every report preserves the original fields and adds:

- median asking price;
- median sale price;
- average sale-to-list ratio;
- average time on market for sold houses;
- total transaction value.

Snapshot definitions are:

- **active buyers/houses**: entrants present at the start of that month;
- **bids and transactions**: activity completed in that month;
- **average and median asking price**: list prices after the beginning-of-month
  comparable update and seller movement, at the exact point buyers evaluate
  houses; this includes every house active that month;
- **average and median sale price**: completed sales that month, or zero when
  none complete;
- **sale-to-list ratio**: each sale price divided by that house's asking price
  at bidding time, averaged across completed sales;
- **time on market**: completed sales' `MonthsOnMarket`, averaged in months;
- **total transaction value**: sum of the month's sale prices;
- **remaining buyers/houses**: after sales and unsuccessful price adjustments,
  before new entrants; `HousesRemaining` is the single ending-inventory metric;
- **price reductions/increases**: unique listings whose final asking price is
  below or above its price at the start of the tick. Each originally active
  listing, including a sold one, is counted at most once according to its net
  movement after all monthly adjustments;
- **change since start**: evaluation-time average asking price compared with the
  initial market's average asking price.

The CSV is written to `monthly-market-reports.csv` in the launch directory. It
uses invariant-culture decimal formatting and one row per report.

## Running

```powershell
dotnet run
```

The program asks for initial buyers, initial houses, an optional integer seed,
the number of months, and whether to use default simulation settings. Counts and
the seed are validated without crashing on invalid text. Reusing the same seed
and settings reproduces generation, preferences, choices, ties, transactions,
and reports.

### Reproducible scenarios

Run the balanced, excess-demand, and excess-supply 120-month scenarios without
interactive prompts:

```powershell
dotnet run -- --scenarios
```

The command prints a concise comparison and writes deterministic report files:

```text
scenario-output/balanced-market.csv
scenario-output/excess-demand-market.csv
scenario-output/excess-supply-market.csv
```

Scenario mode does not overwrite `monthly-market-reports.csv`.

Run the complete xUnit suite with:

```powershell
dotnet test HousingMarketSimulation.slnx
```

The suite includes targeted reporting and settings tests plus 120-month sanity
checks. Long-run validation covers finite and non-negative monetary values,
transaction/report consistency, unique sold houses and successful buyers,
movement-count bounds, deterministic replay, and all three supplied scenarios.
It deliberately avoids narrow price-range assertions that would prevent
legitimate emergent behavior.

GitHub Actions runs restore, Release build, and the complete Release test suite
on pushes to `main` or `master` and pull requests targeting `main`.

## Modelling limitations

- Preference and valuation formulas are explanatory rules, not empirically
  calibrated demand estimates.
- There are no interest rates, repayments, taxes, transaction costs, rental
  markets, construction delays, or geographic submarkets.
- Successful buyers leave the market, so purchase cash flows are not modelled
  afterward.
- Sellers have persistent pricing tendencies but are not strategic agents.
- Auctions process houses in stable market-list order. The one-purchase rule can
  therefore matter if externally supplied bids put one buyer into multiple
  auctions; normal simulation buyers submit only one bid.
