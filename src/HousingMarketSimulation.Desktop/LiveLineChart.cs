using System.Drawing.Drawing2D;

namespace HousingMarketSimulation.Desktop;

internal sealed record LiveChartSeries(
    string Name,
    Color Color,
    Func<MonthlyAnalyticsSnapshot, float?> ValueSelector);

internal sealed class LiveLineChart : Control
{
    private readonly ToolTip tooltip = new();
    private IReadOnlyList<MonthlyAnalyticsSnapshot> snapshots = [];
    private IReadOnlyList<LiveChartSeries> series = [];
    private int latestMonth = 1;
    private int lastTooltipMonth = -1;
    private readonly string chartTitle;
    private readonly string valueUnit;

    public LiveLineChart(string chartTitle, string valueUnit = "")
    {
        this.chartTitle = chartTitle;
        this.valueUnit = valueUnit;
        DoubleBuffered = true;
        BackColor = Color.FromArgb(9, 10, 12);
        ForeColor = Color.FromArgb(241, 244, 249);
        Font = new Font("Segoe UI", 9f);
        MinimumSize = new Size(360, 280);
        AccessibleRole = AccessibleRole.Chart;
    }

    public void ConfigureSeries(params LiveChartSeries[] definitions)
    {
        series = definitions;
        Invalidate();
    }

    public void UpdateData(IReadOnlyList<MonthlyAnalyticsSnapshot> monthlySnapshots)
    {
        snapshots = monthlySnapshots;
        latestMonth = Math.Max(1, snapshots.LastOrDefault()?.Month ?? 1);
        lastTooltipMonth = -1;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using SolidBrush textBrush = new(ForeColor);
        using SolidBrush mutedBrush = new(Color.FromArgb(162, 171, 185));
        using Font titleFont = new("Segoe UI Semibold", 14f);
        graphics.DrawString(chartTitle, titleFont, textBrush, 20, 15);

        DrawLegend(graphics, mutedBrush);

        RectangleF plot = new(
            66,
            84,
            Math.Max(40, ClientSize.Width - 88),
            Math.Max(40, ClientSize.Height - 132));
        if (snapshots.Count == 0 || !HasAnyValue())
        {
            using Font emptyFont = new("Segoe UI", 11f);
            const string empty = "Run or step the simulation to populate this chart.";
            SizeF size = graphics.MeasureString(empty, emptyFont);
            graphics.DrawString(
                empty,
                emptyFont,
                mutedBrush,
                plot.Left + Math.Max(0, (plot.Width - size.Width) / 2),
                plot.Top + Math.Max(0, (plot.Height - size.Height) / 2));
            DrawPlotBorder(graphics, plot);
            return;
        }

        float[] allValues = snapshots
            .SelectMany(snapshot => series.Select(item => item.ValueSelector(snapshot)))
            .Where(value => value.HasValue && float.IsFinite(value.Value))
            .Select(value => value!.Value)
            .ToArray();
        float minimum = allValues.Min();
        float maximum = allValues.Max();
        float spread = maximum - minimum;
        if (spread <= 0) spread = Math.Max(Math.Abs(maximum) * 0.1f, 1f);
        minimum -= spread * 0.12f;
        maximum += spread * 0.12f;
        if (minimum > 0 && minimum < spread * 0.35f) minimum = 0;

        DrawGrid(graphics, plot, minimum, maximum, mutedBrush);
        foreach (LiveChartSeries item in series)
        {
            DrawSeries(graphics, plot, item, minimum, maximum);
        }
        DrawXAxis(graphics, plot, mutedBrush);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (snapshots.Count == 0) return;
        RectangleF plot = new(
            66,
            84,
            Math.Max(40, ClientSize.Width - 88),
            Math.Max(40, ClientSize.Height - 132));
        if (!plot.Contains(e.Location))
        {
            tooltip.Hide(this);
            lastTooltipMonth = -1;
            return;
        }

        float relative = Math.Clamp((e.X - plot.Left) / plot.Width, 0, 1);
        int month = Math.Clamp(
            (int)MathF.Round(1 + relative * (latestMonth - 1)),
            1,
            latestMonth);
        MonthlyAnalyticsSnapshot? snapshot = snapshots
            .OrderBy(item => Math.Abs(item.Month - month))
            .FirstOrDefault();
        if (snapshot is null || snapshot.Month == lastTooltipMonth) return;

        string[] rows =
        [
            $"Month {snapshot.Month}",
            .. series.Select(item =>
            {
                float? value = item.ValueSelector(snapshot);
                return $"{item.Name}: {FormatValue(value)}";
            })
        ];
        tooltip.Show(
            string.Join(Environment.NewLine, rows),
            this,
            Math.Min(e.X + 14, Math.Max(0, Width - 220)),
            Math.Min(e.Y + 14, Math.Max(0, Height - 110)),
            4000);
        lastTooltipMonth = snapshot.Month;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        tooltip.Hide(this);
        lastTooltipMonth = -1;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) tooltip.Dispose();
        base.Dispose(disposing);
    }

    private void DrawLegend(Graphics graphics, Brush mutedBrush)
    {
        float x = 20;
        const float y = 51;
        foreach (LiveChartSeries item in series)
        {
            using Pen pen = new(item.Color, 3);
            graphics.DrawLine(pen, x, y + 7, x + 18, y + 7);
            x += 24;
            float? latest = snapshots.LastOrDefault() is { } snapshot
                ? item.ValueSelector(snapshot)
                : null;
            string label = $"{item.Name}  {FormatValue(latest)}";
            graphics.DrawString(label, Font, mutedBrush, x, y);
            x += graphics.MeasureString(label, Font).Width + 24;
        }
    }

    private void DrawGrid(
        Graphics graphics,
        RectangleF plot,
        float minimum,
        float maximum,
        Brush mutedBrush)
    {
        using Pen gridPen = new(Color.FromArgb(35, 255, 255, 255));
        using Pen axisPen = new(Color.FromArgb(90, 255, 255, 255));
        for (int line = 0; line <= 4; line++)
        {
            float proportion = line / 4f;
            float y = plot.Bottom - plot.Height * proportion;
            graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            float value = minimum + (maximum - minimum) * proportion;
            string label = FormatAxisValue(value);
            SizeF size = graphics.MeasureString(label, Font);
            graphics.DrawString(
                label,
                Font,
                mutedBrush,
                plot.Left - size.Width - 8,
                y - size.Height / 2);
        }
        graphics.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        graphics.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
    }

    private void DrawXAxis(Graphics graphics, RectangleF plot, Brush mutedBrush)
    {
        int[] months =
        [
            1,
            Math.Max(1, latestMonth / 2),
            latestMonth
        ];
        foreach (int month in months.Distinct())
        {
            float x = latestMonth == 1
                ? plot.Left
                : plot.Left + (month - 1f) / (latestMonth - 1f) * plot.Width;
            string label = $"M{month}";
            SizeF size = graphics.MeasureString(label, Font);
            graphics.DrawString(
                label,
                Font,
                mutedBrush,
                Math.Clamp(x - size.Width / 2, plot.Left, plot.Right - size.Width),
                plot.Bottom + 8);
        }
    }

    private void DrawSeries(
        Graphics graphics,
        RectangleF plot,
        LiveChartSeries item,
        float minimum,
        float maximum)
    {
        using Pen pen = new(item.Color, 2.4f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        List<PointF> segment = [];
        foreach (MonthlyAnalyticsSnapshot snapshot in snapshots)
        {
            float? value = item.ValueSelector(snapshot);
            if (!value.HasValue || !float.IsFinite(value.Value))
            {
                DrawSegment(graphics, pen, segment);
                segment.Clear();
                continue;
            }
            float x = latestMonth == 1
                ? plot.Left
                : plot.Left
                    + (snapshot.Month - 1f) / (latestMonth - 1f) * plot.Width;
            float y = plot.Bottom
                - (value.Value - minimum) / (maximum - minimum) * plot.Height;
            segment.Add(new PointF(x, y));
        }
        DrawSegment(graphics, pen, segment);
    }

    private static void DrawSegment(
        Graphics graphics,
        Pen pen,
        IReadOnlyList<PointF> segment)
    {
        if (segment.Count > 1)
            graphics.DrawLines(pen, segment.ToArray());
        else if (segment.Count == 1)
        {
            using SolidBrush brush = new(pen.Color);
            graphics.FillEllipse(
                brush,
                segment[0].X - 2,
                segment[0].Y - 2,
                4,
                4);
        }
    }

    private static void DrawPlotBorder(Graphics graphics, RectangleF plot)
    {
        using Pen pen = new(Color.FromArgb(65, 255, 255, 255));
        graphics.DrawRectangle(pen, plot.X, plot.Y, plot.Width, plot.Height);
    }

    private bool HasAnyValue() => snapshots.Any(snapshot =>
        series.Any(item => item.ValueSelector(snapshot) is { } value
            && float.IsFinite(value)));

    private string FormatValue(float? value) =>
        value.HasValue && float.IsFinite(value.Value)
            ? $"{value.Value:0.##}{valueUnit}"
            : "—";

    private string FormatAxisValue(float value) =>
        Math.Abs(value) >= 1000
            ? $"{value / 1000f:0.#}k"
            : $"{value:0.#}";
}
