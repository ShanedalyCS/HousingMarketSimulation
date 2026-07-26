using System.Drawing.Drawing2D;

namespace HousingMarketSimulation.Desktop;

internal sealed class LiveScatterChart : Control
{
    private static readonly Color AccentColor = Color.FromArgb(98, 234, 213);
    private static readonly Color MutedColor = Color.FromArgb(162, 171, 185);
    private readonly ToolTip tooltip = new();
    private IReadOnlyList<MonthlyAnalyticsSnapshot> snapshots = [];
    private int lastTooltipMonth = -1;

    public LiveScatterChart()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(9, 10, 12);
        ForeColor = Color.FromArgb(241, 244, 249);
        Font = new Font("Segoe UI", 9f);
        MinimumSize = new Size(360, 280);
        AccessibleRole = AccessibleRole.Chart;
    }

    public void UpdateData(IReadOnlyList<MonthlyAnalyticsSnapshot> monthlySnapshots)
    {
        snapshots = monthlySnapshots;
        lastTooltipMonth = -1;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint =
            System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using SolidBrush textBrush = new(ForeColor);
        using SolidBrush mutedBrush = new(MutedColor);
        using Font titleFont = new("Segoe UI Semibold", 14f);
        graphics.DrawString(
            "Houses on market vs average sale price",
            titleFont,
            textBrush,
            20,
            15);
        graphics.DrawString(
            "Each point is a simulated month with at least one sale",
            Font,
            mutedBrush,
            22,
            49);

        RectangleF plot = PlotBounds();
        MonthlyAnalyticsSnapshot[] points = ValidSnapshots();
        if (points.Length == 0)
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
            DrawAxisTitles(graphics, plot, mutedBrush);
            return;
        }

        (float xMinimum, float xMaximum) = ExpandedRange(
            points.Min(snapshot => (float)snapshot.ActiveListings),
            points.Max(snapshot => (float)snapshot.ActiveListings),
            includeZeroWhenClose: true);
        (float yMinimum, float yMaximum) = ExpandedRange(
            points.Min(snapshot => snapshot.RawAverageSalePrice!.Value),
            points.Max(snapshot => snapshot.RawAverageSalePrice!.Value),
            includeZeroWhenClose: false);

        DrawGrid(
            graphics,
            plot,
            xMinimum,
            xMaximum,
            yMinimum,
            yMaximum,
            mutedBrush);
        DrawTrail(
            graphics,
            plot,
            points,
            xMinimum,
            xMaximum,
            yMinimum,
            yMaximum);
        DrawAxisTitles(graphics, plot, mutedBrush);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        MonthlyAnalyticsSnapshot[] points = ValidSnapshots();
        RectangleF plot = PlotBounds();
        if (points.Length == 0 || !plot.Contains(e.Location))
        {
            HideTooltip();
            return;
        }

        (float xMinimum, float xMaximum) = ExpandedRange(
            points.Min(snapshot => (float)snapshot.ActiveListings),
            points.Max(snapshot => (float)snapshot.ActiveListings),
            includeZeroWhenClose: true);
        (float yMinimum, float yMaximum) = ExpandedRange(
            points.Min(snapshot => snapshot.RawAverageSalePrice!.Value),
            points.Max(snapshot => snapshot.RawAverageSalePrice!.Value),
            includeZeroWhenClose: false);

        MonthlyAnalyticsSnapshot nearest = points
            .OrderBy(snapshot =>
            {
                PointF point = ToPlotPoint(
                    snapshot,
                    plot,
                    xMinimum,
                    xMaximum,
                    yMinimum,
                    yMaximum);
                return MathF.Pow(point.X - e.X, 2) + MathF.Pow(point.Y - e.Y, 2);
            })
            .First();
        PointF nearestPoint = ToPlotPoint(
            nearest,
            plot,
            xMinimum,
            xMaximum,
            yMinimum,
            yMaximum);
        float distance = MathF.Sqrt(
            MathF.Pow(nearestPoint.X - e.X, 2)
            + MathF.Pow(nearestPoint.Y - e.Y, 2));
        if (distance > 22 || nearest.Month == lastTooltipMonth)
        {
            if (distance > 22) HideTooltip();
            return;
        }

        tooltip.Show(
            $"Month {nearest.Month}{Environment.NewLine}" +
            $"Houses on market: {nearest.ActiveListings:N0}{Environment.NewLine}" +
            $"Average sale: {nearest.RawAverageSalePrice:0.##} K",
            this,
            Math.Min(e.X + 14, Math.Max(0, Width - 220)),
            Math.Min(e.Y + 14, Math.Max(0, Height - 110)),
            4000);
        lastTooltipMonth = nearest.Month;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        HideTooltip();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) tooltip.Dispose();
        base.Dispose(disposing);
    }

    private RectangleF PlotBounds() => new(
        76,
        78,
        Math.Max(40, ClientSize.Width - 102),
        Math.Max(40, ClientSize.Height - 148));

    private MonthlyAnalyticsSnapshot[] ValidSnapshots() => snapshots
        .Where(snapshot =>
            snapshot.RawAverageSalePrice is { } price
            && float.IsFinite(price))
        .ToArray();

    private static (float Minimum, float Maximum) ExpandedRange(
        float minimum,
        float maximum,
        bool includeZeroWhenClose)
    {
        float spread = maximum - minimum;
        if (spread <= 0) spread = Math.Max(Math.Abs(maximum) * 0.1f, 1f);
        minimum -= spread * 0.12f;
        maximum += spread * 0.12f;
        if (includeZeroWhenClose && minimum > 0 && minimum < spread * 0.35f)
            minimum = 0;
        return (minimum, maximum);
    }

    private static void DrawGrid(
        Graphics graphics,
        RectangleF plot,
        float xMinimum,
        float xMaximum,
        float yMinimum,
        float yMaximum,
        Brush mutedBrush)
    {
        using Pen gridPen = new(Color.FromArgb(35, 255, 255, 255));
        using Pen axisPen = new(Color.FromArgb(90, 255, 255, 255));
        for (int line = 0; line <= 4; line++)
        {
            float proportion = line / 4f;
            float x = plot.Left + plot.Width * proportion;
            float y = plot.Bottom - plot.Height * proportion;
            graphics.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);
            graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);

            string xLabel = FormatCountAxisValue(
                xMinimum + (xMaximum - xMinimum) * proportion);
            SizeF xSize = graphics.MeasureString(xLabel, SystemFonts.DefaultFont);
            graphics.DrawString(
                xLabel,
                SystemFonts.DefaultFont,
                mutedBrush,
                Math.Clamp(x - xSize.Width / 2, plot.Left, plot.Right - xSize.Width),
                plot.Bottom + 8);

            string yLabel = FormatAxisValue(
                yMinimum + (yMaximum - yMinimum) * proportion);
            SizeF ySize = graphics.MeasureString(yLabel, SystemFonts.DefaultFont);
            graphics.DrawString(
                yLabel,
                SystemFonts.DefaultFont,
                mutedBrush,
                plot.Left - ySize.Width - 8,
                y - ySize.Height / 2);
        }
        graphics.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        graphics.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
    }

    private static void DrawTrail(
        Graphics graphics,
        RectangleF plot,
        IReadOnlyList<MonthlyAnalyticsSnapshot> points,
        float xMinimum,
        float xMaximum,
        float yMinimum,
        float yMaximum)
    {
        PointF[] plotPoints = points
            .Select(snapshot => ToPlotPoint(
                snapshot,
                plot,
                xMinimum,
                xMaximum,
                yMinimum,
                yMaximum))
            .ToArray();
        if (plotPoints.Length > 1)
        {
            using Pen trailPen = new(Color.FromArgb(90, AccentColor), 1.5f)
            {
                LineJoin = LineJoin.Round
            };
            graphics.DrawLines(trailPen, plotPoints);
        }

        using SolidBrush pointBrush = new(Color.FromArgb(190, AccentColor));
        foreach (PointF point in plotPoints)
            graphics.FillEllipse(pointBrush, point.X - 3, point.Y - 3, 6, 6);

        PointF latest = plotPoints[^1];
        using SolidBrush latestBrush = new(AccentColor);
        using Pen latestOutline = new(Color.White, 1.5f);
        graphics.FillEllipse(latestBrush, latest.X - 5, latest.Y - 5, 10, 10);
        graphics.DrawEllipse(latestOutline, latest.X - 5, latest.Y - 5, 10, 10);
    }

    private static PointF ToPlotPoint(
        MonthlyAnalyticsSnapshot snapshot,
        RectangleF plot,
        float xMinimum,
        float xMaximum,
        float yMinimum,
        float yMaximum) => new(
            plot.Left
                + (snapshot.ActiveListings - xMinimum)
                / (xMaximum - xMinimum)
                * plot.Width,
            plot.Bottom
                - (snapshot.RawAverageSalePrice!.Value - yMinimum)
                / (yMaximum - yMinimum)
                * plot.Height);

    private static void DrawPlotBorder(Graphics graphics, RectangleF plot)
    {
        using Pen pen = new(Color.FromArgb(65, 255, 255, 255));
        graphics.DrawRectangle(pen, plot.X, plot.Y, plot.Width, plot.Height);
    }

    private static void DrawAxisTitles(
        Graphics graphics,
        RectangleF plot,
        Brush mutedBrush)
    {
        const string xTitle = "Houses on market (active listings)";
        SizeF xSize = graphics.MeasureString(xTitle, SystemFonts.DefaultFont);
        graphics.DrawString(
            xTitle,
            SystemFonts.DefaultFont,
            mutedBrush,
            plot.Left + (plot.Width - xSize.Width) / 2,
            plot.Bottom + 34);

        GraphicsState state = graphics.Save();
        graphics.TranslateTransform(17, plot.Top + plot.Height / 2);
        graphics.RotateTransform(-90);
        const string yTitle = "Average sale price (K)";
        SizeF ySize = graphics.MeasureString(yTitle, SystemFonts.DefaultFont);
        graphics.DrawString(
            yTitle,
            SystemFonts.DefaultFont,
            mutedBrush,
            -ySize.Width / 2,
            0);
        graphics.Restore(state);
    }

    private void HideTooltip()
    {
        tooltip.Hide(this);
        lastTooltipMonth = -1;
    }

    private static string FormatAxisValue(float value) =>
        Math.Abs(value) >= 1000
            ? $"{value / 1000f:0.#}k"
            : $"{value:0.#}";

    private static string FormatCountAxisValue(float value) =>
        MathF.Round(value).ToString("N0");
}
