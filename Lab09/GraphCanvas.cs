using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Lab09;

internal sealed class GraphCanvas : Control
{
    private readonly Font labelFont = new("Segoe UI", 9f);
    private readonly Font titleFont = new("Segoe UI Semibold", 11f, FontStyle.Bold);
    private List<GraphPoint> points = new();
    private GraphSettings settings = new();
    private Rectangle plotArea;
    private double minX = -1;
    private double maxX = 1;
    private double minY = -1;
    private double maxY = 1;

    public GraphCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        ResizeRedraw = true;
    }

    public IReadOnlyList<GraphPoint> Points => points;

    public void Build(GraphSettings nextSettings)
    {
        settings = nextSettings;
        points = CalculatePoints(nextSettings);
        UpdateGraphBounds();
        Invalidate();
    }

    public void SaveImage(string path)
    {
        using var bitmap = new Bitmap(Math.Max(1, Width), Math.Max(1, Height));
        DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
        bitmap.Save(path, ImageFormat.Png);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        e.Graphics.Clear(Color.White);
        plotArea = new Rectangle(72, 34, Math.Max(10, Width - 106), Math.Max(10, Height - 94));
        DrawBackground(e.Graphics);
        DrawGrid(e.Graphics);
        DrawAxes(e.Graphics);
        DrawCurve(e.Graphics);
        DrawHeader(e.Graphics);
    }

    private static List<GraphPoint> CalculatePoints(GraphSettings graphSettings)
    {
        var result = new List<GraphPoint>();
        var start = Math.Min(graphSettings.TMin, graphSettings.TMax);
        var end = Math.Max(graphSettings.TMin, graphSettings.TMax);
        var step = Math.Abs(graphSettings.Step);

        if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
        {
            step = Math.Max(0.01, (end - start) / 800);
        }

        if (Math.Abs(end - start) < 1e-12)
        {
            end = start + step;
        }

        var guard = 0;

        for (var t = start; t <= end + step * 0.5 && guard < 200000; t += step, guard++)
        {
            var value = 2 * graphSettings.P * t;

            if (value < -1e-10)
            {
                continue;
            }

            if (value < 0)
            {
                value = 0;
            }

            result.Add(new GraphPoint(t, t, Math.Sqrt(value)));
        }

        return result;
    }

    private void UpdateGraphBounds()
    {
        if (points.Count == 0)
        {
            minX = -1;
            maxX = 1;
            minY = -1;
            maxY = 1;
            return;
        }

        minX = Math.Min(0, points.Min(point => point.X));
        maxX = Math.Max(0, points.Max(point => point.X));
        minY = settings.ShowLowerBranch ? -points.Max(point => point.Y) : Math.Min(0, points.Min(point => point.Y));
        maxY = Math.Max(0, points.Max(point => point.Y));

        ExpandRange(ref minX, ref maxX);
        ExpandRange(ref minY, ref maxY);
    }

    private static void ExpandRange(ref double min, ref double max)
    {
        if (Math.Abs(max - min) < 1e-9)
        {
            min -= 1;
            max += 1;
            return;
        }

        var margin = (max - min) * 0.12;
        min -= margin;
        max += margin;
    }

    private void DrawBackground(Graphics graphics)
    {
        using var fill = new SolidBrush(Color.FromArgb(250, 251, 252));
        using var border = new Pen(Color.FromArgb(205, 213, 222));
        graphics.FillRectangle(fill, plotArea);
        graphics.DrawRectangle(border, plotArea);
    }

    private void DrawGrid(Graphics graphics)
    {
        var xStep = NiceStep((maxX - minX) / 10);
        var yStep = NiceStep((maxY - minY) / 8);

        using var gridPen = new Pen(Color.FromArgb(224, 229, 235));
        using var textBrush = new SolidBrush(Color.FromArgb(70, 78, 86));

        for (var x = Math.Ceiling(minX / xStep) * xStep; x <= maxX; x += xStep)
        {
            var sx = ToScreenX(x);
            graphics.DrawLine(gridPen, sx, plotArea.Top, sx, plotArea.Bottom);
            graphics.DrawString(FormatNumber(x), labelFont, textBrush, sx - 18, plotArea.Bottom + 8);
        }

        for (var y = Math.Ceiling(minY / yStep) * yStep; y <= maxY; y += yStep)
        {
            var sy = ToScreenY(y);
            graphics.DrawLine(gridPen, plotArea.Left, sy, plotArea.Right, sy);
            graphics.DrawString(FormatNumber(y), labelFont, textBrush, 10, sy - 8);
        }
    }

    private void DrawAxes(Graphics graphics)
    {
        using var axisPen = new Pen(Color.FromArgb(30, 35, 40), 2f);
        using var arrowBrush = new SolidBrush(Color.FromArgb(30, 35, 40));
        using var textBrush = new SolidBrush(Color.FromArgb(30, 35, 40));
        var xAxisY = ToScreenY(0);
        var yAxisX = ToScreenX(0);

        if (xAxisY >= plotArea.Top && xAxisY <= plotArea.Bottom)
        {
            graphics.DrawLine(axisPen, plotArea.Left, xAxisY, plotArea.Right, xAxisY);
            DrawArrow(graphics, arrowBrush, new PointF(plotArea.Right, xAxisY), true);
            graphics.DrawString("x", titleFont, textBrush, plotArea.Right - 14, xAxisY - 24);
        }

        if (yAxisX >= plotArea.Left && yAxisX <= plotArea.Right)
        {
            graphics.DrawLine(axisPen, yAxisX, plotArea.Bottom, yAxisX, plotArea.Top);
            DrawArrow(graphics, arrowBrush, new PointF(yAxisX, plotArea.Top), false);
            graphics.DrawString("y", titleFont, textBrush, yAxisX + 8, plotArea.Top + 2);
        }
    }

    private static void DrawArrow(Graphics graphics, Brush brush, PointF tip, bool horizontal)
    {
        PointF[] polygon = horizontal
            ? new[] { tip, new PointF(tip.X - 9, tip.Y - 5), new PointF(tip.X - 9, tip.Y + 5) }
            : new[] { tip, new PointF(tip.X - 5, tip.Y + 9), new PointF(tip.X + 5, tip.Y + 9) };
        graphics.FillPolygon(brush, polygon);
    }

    private void DrawCurve(Graphics graphics)
    {
        if (points.Count < 2)
        {
            DrawEmptyMessage(graphics);
            return;
        }

        using var curvePen = new Pen(Color.FromArgb(11, 113, 159), 3f)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using var lowerPen = new Pen(Color.FromArgb(185, 83, 67), 2.4f)
        {
            DashStyle = DashStyle.Dash,
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        var upper = points.Select(point => new PointF(ToScreenX(point.X), ToScreenY(point.Y))).ToArray();
        graphics.DrawLines(curvePen, upper);

        if (settings.ShowLowerBranch)
        {
            var lower = points.Select(point => new PointF(ToScreenX(point.X), ToScreenY(-point.Y))).ToArray();
            graphics.DrawLines(lowerPen, lower);
        }

        if (settings.ShowPoints)
        {
            using var brush = new SolidBrush(Color.FromArgb(11, 113, 159));

            foreach (var point in upper.Where((_, index) => index % Math.Max(1, upper.Length / 80) == 0))
            {
                graphics.FillEllipse(brush, point.X - 2.5f, point.Y - 2.5f, 5, 5);
            }
        }
    }

    private void DrawHeader(Graphics graphics)
    {
        using var titleBrush = new SolidBrush(Color.FromArgb(28, 33, 39));
        using var textBrush = new SolidBrush(Color.FromArgb(85, 92, 100));
        graphics.DrawString("Варіант 4: x = t, y = sqrt(2pt)", titleFont, titleBrush, plotArea.Left, 8);
        graphics.DrawString($"p = {FormatNumber(settings.P)}, t [{FormatNumber(settings.TMin)}; {FormatNumber(settings.TMax)}], точок: {points.Count}", labelFont, textBrush, plotArea.Left + 260, 11);
    }

    private void DrawEmptyMessage(Graphics graphics)
    {
        using var brush = new SolidBrush(Color.FromArgb(160, 70, 55));
        var text = "Немає точок у дійсній області: потрібно 2pt >= 0";
        var size = graphics.MeasureString(text, titleFont);
        graphics.DrawString(text, titleFont, brush, plotArea.Left + (plotArea.Width - size.Width) / 2, plotArea.Top + (plotArea.Height - size.Height) / 2);
    }

    private float ToScreenX(double x)
    {
        return (float)(plotArea.Left + (x - minX) / (maxX - minX) * plotArea.Width);
    }

    private float ToScreenY(double y)
    {
        return (float)(plotArea.Bottom - (y - minY) / (maxY - minY) * plotArea.Height);
    }

    private static double NiceStep(double raw)
    {
        if (raw <= 0 || double.IsNaN(raw) || double.IsInfinity(raw))
        {
            return 1;
        }

        var power = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        var fraction = raw / power;
        var nice = fraction switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 5 => 5,
            _ => 10
        };
        return nice * power;
    }

    private static string FormatNumber(double value)
    {
        if (Math.Abs(value) < 1e-9)
        {
            value = 0;
        }

        return Math.Abs(value) >= 1000 || Math.Abs(value) < 0.01 && value != 0
            ? value.ToString("0.##E+0")
            : value.ToString("0.##");
    }
}
