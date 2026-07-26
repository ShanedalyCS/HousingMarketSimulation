# Housing Market Simulation

A deterministic, monthly agent-style housing market written in C#. Buyers form
individual willingness-to-pay values, submit sealed bids, and leave the market
after purchasing. Completed sales feed back into later valuations and seller
prices. A separate analytics layer observes the simulation and produces
reproducible CSV/JSON datasets plus a self-contained interactive dashboard.

All monetary values are in thousands. For example, `100 K` represents 100,000
in the simulated currency.

## Repository structure

```text
HousingMarketSimulation.slnx
README.md
src/HousingMarketSimulation/
  Analytics/       rolling indices and monthly analytical snapshots
  Configuration/   simulation, valuation, and analytics settings
  Dashboard/       offline HTML dashboard generator and template
  Data/            runtime seed data
  Domain/          buyers, houses, bids, and transactions
  Reporting/       CSV and JSON exporters
  Services/        bidding, valuation, pricing, and buyer decisions
  Simulation/      market state, lifecycle, scenarios, and data generation
tests/HousingMarketSimulation.Tests/
```

Generated analysis is kept under `analysis-output/` and ignored by Git, so the
repository root remains source-focused.

## Monthly lifecycle

Each month runs in this order:

1. Clear previous bidding and buyer-selection state.
2. Add 20% of monthly salary to each active buyer's savings.
3. Increment each active listing's time on market.
4. Recalculate estimated values from completed comparable sales.
5. Move asking prices toward each seller's market-informed target.
6. Capture evaluation-time affordability and asking-price analytics.
7. Let buyers evaluate listings and submit at most one sealed bid.
8. Settle each house's auction and timestamp each transaction with the month.
9. Remove sold houses and successful buyers.
10. Adjust unsuccessful listings.
11. Capture the end-of-month analytical snapshot and monthly report.
12. Add configured entrants, who participate beginning next month.

The analytical services only read simulation state. They do not change buyer,
seller, bidding, valuation, or settlement behaviour.

## Values and market feedback

The model keeps four monetary concepts separate:

- `BaseValue`: immutable fundamental cost valuation.
- `EstimatedMarketValue`: fundamental value blended with comparable sales.
- `AskingPrice`: seller's current list price.
- `SalePrice`: auction settlement recorded in a transaction.

```text
BaseValue = LandValue + ReplacementCost - AgeDepreciation
seller target = EstimatedMarketValue × persistent seller multiplier
```

Land begins at `0.35 K` per plot square metre and varies by location.
Replacement cost is floor area times `1.8 K`, adjusted for quality. It
depreciates by 1% per year, capped at 60%. Each seller receives one seeded
multiplier from 0.95 to 1.15 for the life of the listing. Asking prices move
toward the target in either direction, capped at 3% per month by default.

Comparable sales are scored by location, floor-area similarity, build quality,
and age. A comparable's bounded sale-price-to-base-value ratio is applied to the
subject property's base value. Similar transactions receive more weight, while
sample influence is capped. With no suitable comparables, the fundamental value
is retained.

## Buyer decisions and settlement

Buyer suitability combines normalized preferences for location, quality, floor
area, plot size, and age:

```text
perceived value = EstimatedMarketValue × (0.80 + 0.40 × suitability)
motivated value = perceived value × (1 + motivation / 200)
maximum bid = min(motivated value, affordability limit)
```

Affordability is the lower of the deposit constraint and income multiple:

```text
min(savings / 20%, savings + annual salary × 4)
```

A buyer considers a below-asking bid only within the configured tolerance.
With one qualifying bid, the winner pays asking price. With multiple qualifying
bids, the winner pays the lower of their own maximum and the greater of asking
price or second-highest bid plus the configured increment. Seeded lotteries
resolve equal bids.

Listings with no bids are reduced by 2% by default. Listings with rejected bids
move halfway toward the highest rejected bid, subject to the monthly movement
cap. Successful sale evidence—not an artificial bid-count rule—drives upward
market feedback.

## Analytics

`MarketAnalyticsService` records one snapshot per completed month.
Evaluation-time metrics use the active buyer and listing stocks immediately
before bidding. Ending-inventory metrics use the post-sale, post-adjustment
market before new entrants. `NewListings` is a flow: initial stock in month 1,
then configured monthly entrants.

The snapshot includes:

- average and median raw asking and sale prices;
- a rolling constant-quality price index, overall and by location;
- active buyers, active listings, new listings, transactions, and inventory;
- months of supply and inventory-to-buyer ratio;
- sales, price reductions, and price increases;
- sale-to-list ratio, time on market, and total transaction value;
- median buyer affordability and asking price;
- percentage of buyers capable of bidding and median affordability gap.

Undefined values are represented as blank CSV fields and JSON `null`, never
fabricated zeroes, `NaN`, or infinity. For example, months of supply is
undefined when a month has no transactions.

### Constant-quality price index

For each valid transaction:

```text
quality-adjusted ratio = SalePrice / BaseValue
```

Ratios outside the configured 0.50–2.00 bounds are excluded. The monthly index
uses the median ratio across the rolling 12-month window:

```text
index = 100 × rolling median ratio / first eligible rolling median ratio
```

The overall series requires five valid observations; each location series
requires three. Before a series has enough evidence it is `null`. Its first
eligible value becomes the fixed baseline of 100. Segmenting by location and
normalizing by immutable base value reduces mix effects, although it cannot
remove every form of composition bias.

Raw average prices can move merely because a different mix of homes sold—for
example, more large or desirable properties. The constant-quality index asks a
different question: how did sale prices move relative to the modeled
fundamental characteristics of the homes that sold? The dashboard deliberately
shows both rather than treating either as a complete market measure.

## Running

Interactive mode:

```powershell
dotnet run --project src/HousingMarketSimulation
```

It asks for initial counts, months, an optional integer seed, and settings.
Reusing the same seed and settings reproduces generation, choices, ties,
transactions, reports, and analytics. It writes these files in the launch
directory:

```text
monthly-market-reports.csv
monthly-analytics.csv
dashboard-data.json
```

Run the three deterministic 120-month scenarios:

```powershell
dotnet run --project src/HousingMarketSimulation -- --scenarios
```

Generate the scenario datasets and self-contained dashboard:

```powershell
dotnet run --project src/HousingMarketSimulation -- --dashboard
```

Output:

```text
analysis-output/
  dashboard.html
  scenario-comparison.json
  balanced-market/
    monthly-market-reports.csv
    monthly-analytics.csv
    dashboard-data.json
  excess-demand-market/
    ...
  excess-supply-market/
    ...
```

Open `analysis-output/dashboard.html` directly in a browser. It has no server,
CDN, package, or network dependency. Scenario selection, comparison mode,
location series, legend controls, tooltips, KPI cards, and reset controls all
operate locally.

### What the dashboard demonstrates

- Raw prices and the constant-quality index may diverge when the sold-property
  mix changes.
- Excess demand and excess supply produce visibly different buyer/inventory
  stocks, liquidity, price movements, and affordability.
- Overall movement can conceal different location-level paths.
- Missing evidence appears as chart gaps, making sparse or zero-transaction
  periods explicit.

## Reports and formulas

The original monthly report remains available. Its asking-price measures are
captured after comparable updates and seller movement, when buyers evaluate
listings. Remaining inventory is measured after settlement and unsuccessful
listing adjustments, before entrants.

Key analytical formulas are:

```text
months of supply = ending inventory / monthly transactions
inventory-to-buyer ratio = ending inventory / active buyers
sale-to-list ratio = mean(SalePrice / ListPrice)
capable buyer percentage =
  buyers able to bid on at least one evaluation-time listing / active buyers × 100
```

CSV output uses stable headers and invariant-culture decimals.

## Configuration

- `SimulationSettings`: entrants, price adjustments, bid tolerance, auction
  increment, and savings rate.
- `ValuationSettings`: cost assumptions, seller multipliers, comparable
  similarity, weighting, and ratio bounds.
- `AnalyticsSettings`: rolling window, minimum overall/location observations,
  and price-index ratio bounds.

All three reject non-finite values, invalid ranges, and inconsistent bounds.

## Tests and CI

```powershell
dotnet test HousingMarketSimulation.slnx
```

Tests cover transaction timestamps, lifecycle timing, market behaviour,
constant-quality baselines and rolling windows, sparse data, composition bias,
location segmentation, affordability timing, schema stability, JSON nulls,
dashboard embedding, deterministic replay, and 120-month scenario sanity.

GitHub Actions restores, builds, and tests Release configuration for pushes to
`main` or `master` and pull requests targeting `main`.

## Modelling limitations

- Formulas are transparent simulation rules, not official statistics or
  empirically calibrated housing indices.
- Base value is a modeled cost proxy and cannot capture every quality or
  location characteristic, so residual composition bias remains.
- Small samples make medians and location indices volatile; thresholds prevent
  false precision but do not create evidence.
- There are no interest rates, repayments, taxes, transaction costs, rentals,
  construction delays, or geographic submarkets within a location category.
- Successful buyers leave the market; post-purchase cash flows are not modeled.
- Sellers have persistent tendencies but are not strategic agents.
