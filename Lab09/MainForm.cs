using System.Globalization;

namespace Lab09;

internal sealed class MainForm : Form
{
    private readonly GraphCanvas canvas = new();
    private readonly NumericUpDown pBox = new();
    private readonly NumericUpDown tMinBox = new();
    private readonly NumericUpDown tMaxBox = new();
    private readonly NumericUpDown stepBox = new();
    private readonly CheckBox lowerBranchBox = new();
    private readonly CheckBox pointsBox = new();
    private readonly Label domainLabel = new();
    private readonly ToolStripStatusLabel statusLabel = new();
    private readonly DataGridView table = new();

    public MainForm()
    {
        Text = "Лабораторна робота №9 - графік варіанта 4";
        MinimumSize = new Size(1120, 720);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10f);
        InitializeLayout();
        BuildGraph();
    }

    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.FromArgb(238, 241, 244)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        Controls.Add(root);

        var side = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            BackColor = Color.FromArgb(248, 249, 250)
        };
        root.Controls.Add(side, 0, 0);

        var title = new Label
        {
            Text = "Параметрична крива",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold)
        };
        side.Controls.Add(title);

        var formula = new Label
        {
            Text = "Варіант 4\r\nx = t\r\ny = sqrt(2pt)",
            Dock = DockStyle.Top,
            Height = 78,
            ForeColor = Color.FromArgb(35, 47, 58)
        };
        side.Controls.Add(formula);

        var controls = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 8,
            Height = 310,
            Padding = new Padding(0, 8, 0, 8)
        };
        controls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        controls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        side.Controls.Add(controls);

        ConfigureNumber(pBox, -1000, 1000, 2, 2, 0.5m);
        ConfigureNumber(tMinBox, -10000, 10000, 0, 2, 0.5m);
        ConfigureNumber(tMaxBox, -10000, 10000, 12, 2, 0.5m);
        ConfigureNumber(stepBox, 0.001m, 1000, 0.02m, 3, 0.01m);
        AddRow(controls, 0, "p", pBox);
        AddRow(controls, 1, "t початкове", tMinBox);
        AddRow(controls, 2, "t кінцеве", tMaxBox);
        AddRow(controls, 3, "крок", stepBox);

        lowerBranchBox.Text = "показати нижню гілку";
        lowerBranchBox.Dock = DockStyle.Fill;
        lowerBranchBox.CheckedChanged += (_, _) => BuildGraph();
        controls.Controls.Add(lowerBranchBox, 0, 4);
        controls.SetColumnSpan(lowerBranchBox, 2);

        pointsBox.Text = "показати точки";
        pointsBox.Dock = DockStyle.Fill;
        pointsBox.CheckedChanged += (_, _) => BuildGraph();
        controls.Controls.Add(pointsBox, 0, 5);
        controls.SetColumnSpan(pointsBox, 2);

        var buildButton = new Button
        {
            Text = "Побудувати",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(23, 113, 159),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        buildButton.FlatAppearance.BorderSize = 0;
        buildButton.Click += (_, _) => BuildGraph();
        controls.Controls.Add(buildButton, 0, 6);
        controls.SetColumnSpan(buildButton, 2);

        var saveButton = new Button
        {
            Text = "Зберегти PNG",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat
        };
        saveButton.Click += (_, _) => SaveImage();
        controls.Controls.Add(saveButton, 0, 7);
        controls.SetColumnSpan(saveButton, 2);

        domainLabel.Dock = DockStyle.Top;
        domainLabel.Height = 72;
        domainLabel.ForeColor = Color.FromArgb(80, 86, 92);
        side.Controls.Add(domainLabel);

        table.Dock = DockStyle.Fill;
        table.AllowUserToAddRows = false;
        table.AllowUserToDeleteRows = false;
        table.ReadOnly = true;
        table.RowHeadersVisible = false;
        table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        table.BackgroundColor = Color.White;
        table.BorderStyle = BorderStyle.FixedSingle;
        table.Columns.Add("t", "t");
        table.Columns.Add("x", "x");
        table.Columns.Add("y", "y");
        side.Controls.Add(table);

        canvas.Dock = DockStyle.Fill;
        canvas.Margin = new Padding(10);
        root.Controls.Add(canvas, 1, 0);

        var status = new StatusStrip
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        status.Items.Add(statusLabel);
        root.Controls.Add(status, 0, 1);
        root.SetColumnSpan(status, 2);
    }

    private void ConfigureNumber(NumericUpDown box, decimal min, decimal max, decimal value, int decimals, decimal increment)
    {
        box.Minimum = min;
        box.Maximum = max;
        box.Value = value;
        box.DecimalPlaces = decimals;
        box.Increment = increment;
        box.Dock = DockStyle.Fill;
        box.ValueChanged += (_, _) => BuildGraph();
    }

    private static void AddRow(TableLayoutPanel panel, int row, string text, Control control)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(label, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private void BuildGraph()
    {
        var settings = new GraphSettings
        {
            P = (double)pBox.Value,
            TMin = (double)tMinBox.Value,
            TMax = (double)tMaxBox.Value,
            Step = (double)stepBox.Value,
            ShowLowerBranch = lowerBranchBox.Checked,
            ShowPoints = pointsBox.Checked
        };

        canvas.Build(settings);
        UpdateDomain(settings);
        FillTable();
        statusLabel.Text = $"Побудовано точок: {canvas.Points.Count}";
    }

    private void UpdateDomain(GraphSettings settings)
    {
        var domain = settings.P switch
        {
            > 0 => "Область визначення: t >= 0, бо 2pt >= 0",
            < 0 => "Область визначення: t <= 0, бо 2pt >= 0",
            _ => "Область визначення: усі t, графік лежить на осі x"
        };

        domainLabel.Text = $"{domain}\r\nНедійсні точки не малюються.";
    }

    private void FillTable()
    {
        table.Rows.Clear();

        foreach (var point in canvas.Points.Take(120))
        {
            table.Rows.Add(Format(point.T), Format(point.X), Format(point.Y));
        }
    }

    private static string Format(double value)
    {
        return value.ToString("0.###", CultureInfo.CurrentCulture);
    }

    private void SaveImage()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PNG зображення|*.png",
            DefaultExt = "png",
            AddExtension = true,
            FileName = "lab09_variant4.png"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        canvas.SaveImage(dialog.FileName);
        statusLabel.Text = $"Збережено: {dialog.FileName}";
    }
}
