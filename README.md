# Housing Market Simulation

A simple console-based, agent-style housing market written in C#. Buyers save money, evaluate affordability, choose a house, and submit bids. Houses can sell, remain listed, or change asking price in response to demand. The simulation records a monthly history and exports it to CSV.

All monetary values are expressed in thousands. For example, `100 K` represents 100,000 in the simulated currency.

## Monthly lifecycle

Each month runs in this order:

1. Clear bidding state left from the previous month.
2. Add 20% of each buyer's monthly salary to their savings.
3. Calculate which houses each buyer can afford.
4. Let each buyer choose their highest-quality affordable house and place one bid.
5. In price-discovery mode, establish prices for zero-priced houses that received at least three bids.
6. Deliberate bids against the asking price that existed when the bids were placed, or the newly discovered price for a zero-priced house.
7. Record transactions and remove sold houses and successful buyers.
8. Adjust prices only for houses that remain on the market.
9. Record and print the end-of-month report.
10. Add one buyer and one house that can participate starting next month.

The report is recorded before new entrants are added. Its active counts describe the buyers and houses that participated during the completed month. Average asking price uses those same active houses, including a sold house's final asking price and any end-of-month reduction applied to an unsold house. Remaining counts describe the inventory left after transactions.

## Normal and price-discovery modes

In normal mode, generated houses begin with an asking price derived from their quality characteristics.

In optional zero-price discovery mode, houses begin at `0 K`. Buyers form an initial valuation from house quality and motivation. A zero-priced house needs at least three bids before its first asking price is established from the average of those bids. With fewer than three bids, it remains unpriced and all bids are rejected.

After a price has been established, both modes use the normal bidding and price-adjustment rules.

## Affordability and bids

A buyer must satisfy both limits:

- savings must cover a minimum 20% deposit;
- the purchase price cannot exceed savings plus a mortgage of four times annual salary.

The maximum purchase price is:

```text
minimum of:
    savings / 20%
    savings + (annual salary × 4)
```

A normal offer starts at the asking price and adds a 1% premium for each motivation point. An offer is always capped at the buyer's maximum purchase price.

The highest bid meeting the asking price wins. Equal highest bids are settled by a random lottery. Supplying a seed makes that lottery reproducible.

## Price changes

- A priced house receiving no bids is reduced by 2% if it remains for sale.
- A remaining house receiving at least three bids increases by 2%.
- Each bid beyond the three-bid threshold adds another 0.1 percentage points to that increase.
- Zero-priced houses are not counted as price reductions; their initial pricing is recorded separately as price discovery.

For example, a remaining house with 20 bids increases by `2% + (17 × 0.1%) = 3.7%`.

## Running the simulation

From the repository root:

```powershell
dotnet run
```

The program asks for:

1. initial buyer count;
2. initial house count;
3. whether to use zero-price discovery mode;
4. an optional integer random seed;
5. number of months to simulate.

Press Enter at the seed prompt for a non-reproducible run. Reusing the same seed and starting inputs produces the same generated market and tied-bid outcomes.

## Tests

Run the complete xUnit suite with:

```powershell
dotnet test HousingMarketSimulation.slnx
```

The tests cover affordability, bid acceptance, asking-price timing, monthly price reductions, report snapshots, entrant timing, initial-data reproducibility, zero-price discovery, and reproducible tie-breaking.

## CSV reports

After the simulation finishes, all monthly reports are exported to:

```text
monthly-market-reports.csv
```

The file is created in the working directory from which the program was launched. It contains one row per month and uses invariant-culture decimal formatting.

## Current limitations

- Houses make simple rule-based acceptance and pricing decisions; there are no true seller agents.
- Buyers all rank houses using the same combined quality score, so preferences are not heterogeneous.
- Buyers submit only one bid per month.
- The model does not deduct a completed purchase from savings or model repayments after purchase because successful buyers leave the market.
- There are no interest rates, taxes, transaction costs, rental markets, construction delays, or geographic submarkets.
- Prices and behavior are intentionally simplified for readability rather than calibrated to real housing data.
