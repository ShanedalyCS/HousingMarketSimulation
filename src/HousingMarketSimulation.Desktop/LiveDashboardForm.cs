namespace HousingMarketSimulation.Desktop;

public sealed class LiveDashboardForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(3, 3, 3);
    private static readonly Color PanelColor = Color.FromArgb(20, 21, 24);
    private static readonly Color BorderColor = Color.FromArgb(50, 53, 60);
    private static readonly Color TextColor = Color.FromArgb(244, 246, 250);
    private static readonly Color MutedColor = Color.FromArgb(165, 173, 187);
    private static readonly Color AccentColor = Color.FromArgb(98, 234, 213);
    private static readonly Color BlueColor = Color.FromArgb(138, 169, 255);
    private static readonly Color GoldColor = Color.FromArgb(244, 198, 106);
    private static readonly Color PinkColor = Color.FromArgb(241, 143, 157);

    private readonly NumericUpDown buyersInput = CreateNumberInput(40, 0, 10000);
    private readonly NumericUpDown housesInput = CreateNumberInput(40, 0, 10000);
    private readonly NumericUpDown monthsInput = CreateNumberInput(120, 1, 2000);
    private readonly NumericUpDown seedInput = CreateNumberInput(1101, int.MinValue, int.MaxValue);
    private readonly NumericUpDown newBuyersInput = CreateNumberInput(1, 0, 1000);
    private readonly NumericUpDown newHousesInput = CreateNumberInput(1, 0, 1000);
    private readonly ComboBox speedInput = new();
    private readonly Button startPauseButton = CreateButton("Start");
    private readonly Button stepButton = CreateButton("Step one month");
    private readonly Button resetButton = CreateButton("Reset");
    private readonly ProgressBar progress = new();
    private readonly Label statusLabel = new();
    private readonly Dictionary<string, Label> kpiValues = [];
    private readonly System.Windows.Forms.Timer timer = new();
    private readonly LiveLineChart priceChart = new("Raw market prices", " K")
    {
        Dock = DockStyle.Fill,
        AccessibleName = "Raw asking and sale price chart"
    };
    private readonly LiveLineChart qualityChart = new("Quality-adjusted price index")
    {
        Dock = DockStyle.Fill,
        AccessibleName = "Quality-adjusted price index chart"
    };
    private readonly LiveLineChart supplyChart = new("Supply and demand")
    {
        Dock = DockStyle.Fill,
        AccessibleName = "Active buyers and housing supply chart"
    };
    private readonly LiveLineChart activityChart = new("Market activity")
    {
        Dock = DockStyle.Fill,
        AccessibleName = "Bids and transactions chart"
    };
    private readonly LiveScatterChart listingsPriceChart = new()
    {
        Dock = DockStyle.Fill,
        AccessibleName = "Houses on market versus average sale price chart"
    };

    private LiveSimulationSession? session;
    private bool isRunning;

    public LiveDashboardForm()
    {
        Text = "Housing Market Simulation · Live Dashboard";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1040, 760);
        Size = new Size(1320, 900);
        BackColor = BackgroundColor;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9.5f);
        AutoScaleMode = AutoScaleMode.Dpi;

        ConfigureCharts();
        ConfigureSpeedInput();
        Controls.Add(BuildLayout());

        timer.Tick += (_, _) => AdvanceOneMonth();
        startPauseButton.Click += (_, _) => ToggleRunning();
        stepButton.Click += (_, _) => StepOneMonth();
        resetButton.Click += (_, _) => ResetSession();
        speedInput.SelectedIndexChanged += (_, _) => ApplySelectedSpeed();
        FormClosing += (_, _) =>
        {
            isRunning = false;
            timer.Stop();
        };
        ResetView();
    }

    private Control BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            Padding = new Padding(20),
            RowCount = 5,
            ColumnCount = 1
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildConfigurationPanel(), 0, 1);
        root.Controls.Add(BuildKpiGrid(), 0, 2);
        root.Controls.Add(BuildChartTabs(), 0, 3);
        root.Controls.Add(BuildStatusBar(), 0, 4);
        return root;
    }

    private Control BuildHeader()
    {
        Panel panel = new()
        {
            Dock = DockStyle.Top,
            Height = 84,
            Margin = new Padding(0, 0, 0, 12)
        };
        Label eyebrow = new()
        {
            AutoSize = true,
            ForeColor = AccentColor,
            Font = new Font("Segoe UI Semibold", 9f),
            Text = "LIVE AGENT-BASED SIMULATION",
            Location = new Point(0, 2)
        };
        Label title = new()
        {
            AutoSize = true,
            ForeColor = TextColor,
            Font = new Font("Segoe UI Semibold", 23f),
            Text = "Housing Market Live Dashboard",
            Location = new Point(0, 23)
        };
        Label subtitle = new()
        {
            AutoSize = true,
            ForeColor = MutedColor,
            Text = "Choose a market, then watch buyers, listings, bids, transactions, and prices evolve one month at a time.",
            Location = new Point(2, 62)
        };
        panel.Controls.AddRange([eyebrow, title, subtitle]);
        return panel;
    }

    private Control BuildConfigurationPanel()
    {
        FlowLayoutPanel panel = new()
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            BackColor = PanelColor,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 12),
            WrapContents = true
        };
        panel.Controls.AddRange(
        [
            CreateField("Initial buyers", buyersInput),
            CreateField("Initial houses", housesInput),
            CreateField("Months", monthsInput),
            CreateField("Seed", seedInput),
            CreateField("Buyers / month", newBuyersInput),
            CreateField("Houses / month", newHousesInput),
            CreateField("Speed", speedInput),
            CreateButtonPanel()
        ]);
        PaintBorder(panel);
        return panel;
    }

    private Control BuildKpiGrid()
    {
        TableLayoutPanel grid = new()
        {
            AutoSize = false,
            Height = 86,
            Dock = DockStyle.Top,
            ColumnCount = 8,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
        string[] metrics =
        [
            "Month",
            "Current buyers",
            "Inventory",
            "Transactions",
            "Quality index",
            "Average asking",
            "Sale / list",
            "Time on market"
        ];
        foreach (string metric in metrics)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
            grid.Controls.Add(CreateKpi(metric));
        }
        return grid;
    }

    private Control BuildChartTabs()
    {
        TableLayoutPanel container = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(9, 10, 12),
            RowCount = 2,
            ColumnCount = 1
        };
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        FlowLayoutPanel selector = new()
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            WrapContents = false,
            Margin = Padding.Empty
        };
        Panel chartHost = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(9, 10, 12),
            Padding = new Padding(2)
        };
        (string Name, Control Chart)[] items =
        [
            ("Prices", priceChart),
            ("Quality index", qualityChart),
            ("Supply & demand", supplyChart),
            ("Activity", activityChart),
            ("Supply vs price", listingsPriceChart)
        ];
        List<Button> buttons = [];
        foreach ((string name, Control chart) in items)
        {
            chart.Visible = false;
            chartHost.Controls.Add(chart);
            Button button = CreateButton(name);
            button.Width = 150;
            button.Height = 34;
            button.Margin = new Padding(0, 0, 2, 0);
            button.Click += (_, _) =>
                SelectChart(chart, button, items, buttons);
            buttons.Add(button);
            selector.Controls.Add(button);
        }
        SelectChart(priceChart, buttons[0], items, buttons);
        container.Controls.Add(selector, 0, 0);
        container.Controls.Add(chartHost, 0, 1);
        return container;
    }

    private Control BuildStatusBar()
    {
        TableLayoutPanel panel = new()
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0, 12, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        statusLabel.ForeColor = MutedColor;
        statusLabel.AutoSize = true;
        statusLabel.Padding = new Padding(0, 6, 12, 0);
        progress.Dock = DockStyle.Fill;
        progress.Height = 20;
        progress.Style = ProgressBarStyle.Continuous;
        panel.Controls.Add(statusLabel, 0, 0);
        panel.Controls.Add(progress, 1, 0);
        return panel;
    }

    private void ConfigureCharts()
    {
        priceChart.ConfigureSeries(
            new LiveChartSeries(
                "Average asking",
                AccentColor,
                snapshot => snapshot.RawAverageAskingPrice),
            new LiveChartSeries(
                "Average sale",
                BlueColor,
                snapshot => snapshot.RawAverageSalePrice));
        qualityChart.ConfigureSeries(
            new LiveChartSeries(
                "Overall index",
                AccentColor,
                snapshot => snapshot.ConstantQualityPriceIndex),
            new LiveChartSeries(
                "Baseline",
                Color.FromArgb(130, 145, 170),
                snapshot => snapshot.ConstantQualityPriceIndex.HasValue ? 100f : null));
        supplyChart.ConfigureSeries(
            new LiveChartSeries(
                "Active buyers",
                BlueColor,
                snapshot => snapshot.ActiveBuyers),
            new LiveChartSeries(
                "Houses on market",
                GoldColor,
                snapshot => snapshot.ActiveListings,
                System.Drawing.Drawing2D.DashStyle.Dash));
        activityChart.ConfigureSeries(
            new LiveChartSeries(
                "Bids",
                PinkColor,
                snapshot => snapshot.BidsSubmitted),
            new LiveChartSeries(
                "Transactions",
                AccentColor,
                snapshot => snapshot.Transactions));
    }

    private void ConfigureSpeedInput()
    {
        speedInput.DropDownStyle = ComboBoxStyle.DropDownList;
        speedInput.Width = 145;
        speedInput.AccessibleName = "Simulation speed";
        speedInput.BackColor = Color.FromArgb(31, 33, 38);
        speedInput.ForeColor = TextColor;
        speedInput.Items.AddRange(
        [
            new SimulationSpeed("Slow · 1.0 s", 1000),
            new SimulationSpeed("Normal · 0.5 s", 500),
            new SimulationSpeed("Fast · 0.15 s", 150),
            new SimulationSpeed("Instant · no delay", 0)
        ]);
        speedInput.SelectedIndex = 1;
    }

    private async void ToggleRunning()
    {
        if (isRunning)
        {
            Pause("Paused. Use Start to continue or Step for one month.");
            return;
        }

        EnsureSession();
        if (session!.IsComplete)
        {
            statusLabel.Text = "Simulation complete. Reset to configure another run.";
            return;
        }
        isRunning = true;
        speedInput.Enabled = false;
        startPauseButton.Text = "Pause";
        statusLabel.Text = "Running… charts update after every simulated month.";
        if (SelectedSpeed().IntervalMilliseconds == 0)
        {
            await RunInstantLoopAsync();
        }
        else
        {
            ApplySelectedSpeed();
            timer.Start();
        }
    }

    private void StepOneMonth()
    {
        Pause("Paused after one manual step.");
        EnsureSession();
        AdvanceOneMonth();
    }

    private void AdvanceOneMonth()
    {
        if (session is null || !session.AdvanceOneMonth())
        {
            CompleteSession();
            return;
        }

        RefreshDashboard();
        if (session.IsComplete) CompleteSession();
    }

    private void EnsureSession()
    {
        if (session is not null) return;
        LiveSimulationConfiguration configuration = new(
            InitialBuyers: Decimal.ToInt32(buyersInput.Value),
            InitialHouses: Decimal.ToInt32(housesInput.Value),
            DurationMonths: Decimal.ToInt32(monthsInput.Value),
            Seed: Decimal.ToInt32(seedInput.Value),
            NewBuyersPerMonth: Decimal.ToInt32(newBuyersInput.Value),
            NewHousesPerMonth: Decimal.ToInt32(newHousesInput.Value));
        session = new LiveSimulationSession(configuration);
        progress.Maximum = configuration.DurationMonths;
        SetConfigurationEnabled(false);
        statusLabel.Text = "Simulation initialized. Press Start or Step.";
        RefreshDashboard();
    }

    private void RefreshDashboard()
    {
        if (session is null) return;
        MonthlyAnalyticsSnapshot? latest = session.LatestSnapshot;
        SetKpi("Month", $"{session.CurrentMonth} / {session.Configuration.DurationMonths}");
        SetKpi("Current buyers", session.Market.Buyers.Count.ToString("N0"));
        SetKpi("Inventory", session.Market.Houses.Count.ToString("N0"));
        SetKpi("Transactions", session.Market.Transactions.Count.ToString("N0"));
        SetKpi("Quality index", Format(latest?.ConstantQualityPriceIndex));
        SetKpi("Average asking", Format(latest?.RawAverageAskingPrice, " K"));
        SetKpi("Sale / list", FormatPercent(latest?.AverageSaleToListRatio));
        SetKpi("Time on market", Format(latest?.AverageTimeOnMarket, " mo"));
        progress.Value = Math.Clamp(
            session.CurrentMonth,
            progress.Minimum,
            progress.Maximum);

        IReadOnlyList<MonthlyAnalyticsSnapshot> snapshots =
            session.Market.AnalyticsSnapshots;
        priceChart.UpdateData(snapshots);
        qualityChart.UpdateData(snapshots);
        supplyChart.UpdateData(snapshots);
        activityChart.UpdateData(snapshots);
        listingsPriceChart.UpdateData(snapshots);

        if (latest is not null)
        {
            statusLabel.Text =
                $"Month {latest.Month}: {latest.BidsSubmitted:N0} bids, " +
                $"{latest.Transactions:N0} transactions, " +
                $"{latest.EndingInventory:N0} listings before new entrants.";
        }
    }

    private void CompleteSession()
    {
        timer.Stop();
        isRunning = false;
        speedInput.Enabled = true;
        startPauseButton.Text = "Start";
        if (session is not null)
        {
            statusLabel.Text =
                $"Complete: {session.Market.Transactions.Count:N0} transactions " +
                $"across {session.CurrentMonth:N0} months. Reset to run another market.";
        }
    }

    private void Pause(string message)
    {
        timer.Stop();
        isRunning = false;
        speedInput.Enabled = true;
        startPauseButton.Text = "Start";
        statusLabel.Text = message;
    }

    private void ResetSession()
    {
        timer.Stop();
        isRunning = false;
        session = null;
        startPauseButton.Text = "Start";
        SetConfigurationEnabled(true);
        ResetView();
    }

    private void ResetView()
    {
        progress.Minimum = 0;
        progress.Maximum = Math.Max(1, Decimal.ToInt32(monthsInput.Value));
        progress.Value = 0;
        foreach (Label value in kpiValues.Values) value.Text = "—";
        SetKpi("Month", $"0 / {monthsInput.Value:0}");
        statusLabel.Text = "Configure the market, then press Start or Step one month.";
        priceChart.UpdateData([]);
        qualityChart.UpdateData([]);
        supplyChart.UpdateData([]);
        activityChart.UpdateData([]);
        listingsPriceChart.UpdateData([]);
    }

    private void ApplySelectedSpeed()
    {
        SimulationSpeed speed = SelectedSpeed();
        if (speed.IntervalMilliseconds > 0)
            timer.Interval = speed.IntervalMilliseconds;
    }

    private async Task RunInstantLoopAsync()
    {
        while (isRunning && session is { IsComplete: false })
        {
            AdvanceOneMonth();
            await Task.Yield();
        }
    }

    private SimulationSpeed SelectedSpeed() =>
        speedInput.SelectedItem as SimulationSpeed
        ?? throw new InvalidOperationException("A simulation speed must be selected.");

    private void SetConfigurationEnabled(bool enabled)
    {
        buyersInput.Enabled = enabled;
        housesInput.Enabled = enabled;
        monthsInput.Enabled = enabled;
        seedInput.Enabled = enabled;
        newBuyersInput.Enabled = enabled;
        newHousesInput.Enabled = enabled;
    }

    private Control CreateKpi(string label)
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            Height = 86,
            BackColor = PanelColor,
            Margin = new Padding(0, 0, 8, 0),
            Padding = new Padding(12)
        };
        Label name = new()
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 20,
            ForeColor = MutedColor,
            Font = new Font("Segoe UI Semibold", 8f),
            Text = label.ToUpperInvariant()
        };
        Label value = new()
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = TextColor,
            Font = new Font("Segoe UI Semibold", 17f),
            Text = "—",
            TextAlign = ContentAlignment.MiddleLeft
        };
        kpiValues[label] = value;
        panel.Controls.Add(value);
        panel.Controls.Add(name);
        PaintBorder(panel);
        return panel;
    }

    private Control CreateButtonPanel()
    {
        FlowLayoutPanel panel = new()
        {
            AutoSize = true,
            Margin = new Padding(8, 17, 0, 0)
        };
        startPauseButton.BackColor = Color.FromArgb(30, 82, 76);
        panel.Controls.AddRange([startPauseButton, stepButton, resetButton]);
        return panel;
    }

    private static Control CreateField(string label, Control input)
    {
        TableLayoutPanel field = new()
        {
            AutoSize = true,
            RowCount = 2,
            ColumnCount = 1,
            Margin = new Padding(0, 0, 12, 0)
        };
        Label caption = new()
        {
            AutoSize = true,
            ForeColor = MutedColor,
            Font = new Font("Segoe UI Semibold", 8f),
            Text = label.ToUpperInvariant(),
            Margin = new Padding(0, 0, 0, 4)
        };
        field.Controls.Add(caption, 0, 0);
        field.Controls.Add(input, 0, 1);
        return field;
    }

    private static void SelectChart(
        Control selectedChart,
        Button selectedButton,
        IReadOnlyCollection<(string Name, Control Chart)> items,
        IReadOnlyCollection<Button> buttons)
    {
        foreach ((_, Control chart) in items)
        {
            chart.Visible = chart == selectedChart;
            if (chart.Visible) chart.BringToFront();
        }
        foreach (Button button in buttons)
        {
            bool selected = button == selectedButton;
            button.BackColor = selected
                ? Color.FromArgb(30, 82, 76)
                : Color.FromArgb(17, 18, 21);
            button.ForeColor = selected ? TextColor : MutedColor;
        }
    }

    private static NumericUpDown CreateNumberInput(
        int value,
        int minimum,
        int maximum)
    {
        return new NumericUpDown
        {
            Width = 104,
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            ThousandsSeparator = true,
            BackColor = Color.FromArgb(31, 33, 38),
            ForeColor = TextColor,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static Button CreateButton(string text)
    {
        return new Button
        {
            AutoSize = true,
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(31, 33, 38),
            ForeColor = TextColor,
            FlatAppearance = { BorderColor = BorderColor },
            Padding = new Padding(8, 3, 8, 3),
            Cursor = Cursors.Hand
        };
    }

    private static void PaintBorder(Control control)
    {
        control.Paint += (_, eventArgs) =>
        {
            using Pen border = new(BorderColor);
            eventArgs.Graphics.DrawRectangle(
                border,
                0,
                0,
                control.Width - 1,
                control.Height - 1);
        };
    }

    private void SetKpi(string name, string value)
    {
        if (kpiValues.TryGetValue(name, out Label? label))
            label.Text = value;
    }

    private static string Format(float? value, string suffix = "") =>
        value.HasValue && float.IsFinite(value.Value)
            ? $"{value.Value:N2}{suffix}"
            : "—";

    private static string FormatPercent(float? value) =>
        value.HasValue && float.IsFinite(value.Value)
            ? $"{value.Value:P2}"
            : "—";

    private sealed record SimulationSpeed(string Label, int IntervalMilliseconds)
    {
        public override string ToString() => Label;
    }
}
