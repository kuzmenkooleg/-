namespace Lab10;

internal sealed class MainForm : Form
{
    private readonly TextBox inputBox = new();
    private readonly TextBox keyBox = new();
    private readonly TextBox newDesOutputBox = new();
    private readonly TextBox md5OutputBox = new();
    private readonly TextBox elGamalOutputBox = new();
    private readonly ProgressBar newDesProgress = new();
    private readonly ProgressBar md5Progress = new();
    private readonly ProgressBar elGamalProgress = new();
    private readonly ToolStripStatusLabel statusLabel = new();
    private readonly Button runAllButton = new();
    private readonly Button runNewDesButton = new();
    private readonly Button runMd5Button = new();
    private readonly Button runElGamalButton = new();
    private readonly Button cancelButton = new();
    private CancellationTokenSource? cancellation;
    private bool busy;

    public MainForm()
    {
        Text = "Лабораторна робота №10 - варіант 4";
        MinimumSize = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10f);
        InitializeLayout();
    }

    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            BackColor = Color.FromArgb(239, 242, 244)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 146));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        Controls.Add(root);

        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 36, 42),
            Padding = new Padding(22, 12, 22, 12)
        };
        root.Controls.Add(header, 0, 0);

        var title = new Label
        {
            Text = "Crypto Threads",
            Dock = DockStyle.Top,
            Height = 32,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold)
        };
        header.Controls.Add(title);

        var subtitle = new Label
        {
            Text = "Варіант 4: NewDES, MD5, ElGamal. Кожен метод виконується асинхронно в окремому завданні.",
            Dock = DockStyle.Top,
            Height = 26,
            ForeColor = Color.FromArgb(196, 211, 219)
        };
        header.Controls.Add(subtitle);

        var inputPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(16, 12, 16, 6),
            BackColor = Color.FromArgb(248, 250, 251)
        };
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.Controls.Add(inputPanel, 0, 1);

        inputPanel.Controls.Add(Label("Текст"), 0, 0);
        inputBox.Dock = DockStyle.Fill;
        inputBox.Text = "Приклад повідомлення для лабораторної роботи №10";
        inputPanel.Controls.Add(inputBox, 1, 0);
        inputPanel.SetColumnSpan(inputBox, 3);

        inputPanel.Controls.Add(Label("Ключ NewDES"), 0, 1);
        keyBox.Dock = DockStyle.Fill;
        keyBox.Text = "variant-four-secret-key";
        inputPanel.Controls.Add(keyBox, 1, 1);
        inputPanel.SetColumnSpan(keyBox, 3);

        runAllButton.Text = "Запустити всі";
        runAllButton.Dock = DockStyle.Fill;
        runAllButton.BackColor = Color.FromArgb(0, 119, 136);
        runAllButton.ForeColor = Color.White;
        runAllButton.FlatStyle = FlatStyle.Flat;
        runAllButton.FlatAppearance.BorderSize = 0;
        runAllButton.Click += async (_, _) => await RunAllAsync();
        inputPanel.Controls.Add(runAllButton, 1, 2);

        cancelButton.Text = "Скасувати";
        cancelButton.Dock = DockStyle.Fill;
        cancelButton.Enabled = false;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.Click += (_, _) => cancellation?.Cancel();
        inputPanel.Controls.Add(cancelButton, 3, 2);

        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(239, 242, 244)
        };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
        root.Controls.Add(cards, 0, 2);

        cards.Controls.Add(Card("Метод 1: NewDES", "Блокове шифрування, 64-бітний блок, 120-бітний ключ", runNewDesButton, newDesProgress, newDesOutputBox), 0, 0);
        cards.Controls.Add(Card("Метод 2: MD5", "Алгоритм хешування повідомлення", runMd5Button, md5Progress, md5OutputBox), 1, 0);
        cards.Controls.Add(Card("Метод 3: ElGamal", "Асиметричне шифрування та відновлення тексту", runElGamalButton, elGamalProgress, elGamalOutputBox), 2, 0);

        runNewDesButton.Text = "Запустити NewDES";
        runNewDesButton.Click += async (_, _) => await RunSingleAsync(RunNewDesAsync);
        runMd5Button.Text = "Запустити MD5";
        runMd5Button.Click += async (_, _) => await RunSingleAsync(RunMd5Async);
        runElGamalButton.Text = "Запустити ElGamal";
        runElGamalButton.Click += async (_, _) => await RunSingleAsync(RunElGamalAsync);

        var status = new StatusStrip
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        statusLabel.Text = "Готово";
        status.Items.Add(statusLabel);
        root.Controls.Add(status, 0, 3);
    }

    private static Label Label(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(43, 50, 56)
        };
    }

    private static Panel Card(string title, string description, Button button, ProgressBar progress, TextBox output)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8),
            BackColor = Color.White
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 1,
            Padding = new Padding(14)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(layout);

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 39, 45)
        };
        layout.Controls.Add(titleLabel, 0, 0);

        var descriptionLabel = new Label
        {
            Text = description,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(87, 96, 104)
        };
        layout.Controls.Add(descriptionLabel, 0, 1);

        button.Dock = DockStyle.Fill;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.FromArgb(223, 232, 236);
        button.ForeColor = Color.FromArgb(31, 39, 45);
        layout.Controls.Add(button, 0, 2);

        progress.Dock = DockStyle.Fill;
        progress.Margin = new Padding(0, 8, 0, 8);
        layout.Controls.Add(progress, 0, 3);

        output.Dock = DockStyle.Fill;
        output.Multiline = true;
        output.ScrollBars = ScrollBars.Vertical;
        output.ReadOnly = true;
        output.BorderStyle = BorderStyle.FixedSingle;
        output.Font = new Font("Consolas", 9.5f);
        layout.Controls.Add(output, 0, 4);

        return panel;
    }

    private async Task RunAllAsync()
    {
        if (busy)
        {
            return;
        }

        SetBusy(true);
        cancellation = new CancellationTokenSource();
        ResetProgress();
        statusLabel.Text = "Працюють три асинхронні методи...";

        try
        {
            await Task.WhenAll(
                RunNewDesAsync(cancellation.Token),
                RunMd5Async(cancellation.Token),
                RunElGamalAsync(cancellation.Token));
            statusLabel.Text = "Усі методи завершено";
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Виконання скасовано";
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Помилка: {ex.Message}";
        }
        finally
        {
            cancellation.Dispose();
            cancellation = null;
            SetBusy(false);
        }
    }

    private async Task RunSingleAsync(Func<CancellationToken, Task> operation)
    {
        if (busy)
        {
            return;
        }

        SetBusy(true);
        cancellation = new CancellationTokenSource();
        statusLabel.Text = "Метод виконується асинхронно...";

        try
        {
            await operation(cancellation.Token);
            statusLabel.Text = "Метод завершено";
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Виконання скасовано";
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Помилка: {ex.Message}";
        }
        finally
        {
            cancellation.Dispose();
            cancellation = null;
            SetBusy(false);
        }
    }

    private async Task RunNewDesAsync(CancellationToken token)
    {
        var text = inputBox.Text;
        var key = keyBox.Text;
        newDesOutputBox.Text = "Виконується...";
        newDesProgress.Value = 0;
        var progress = new Progress<int>(value => newDesProgress.Value = Math.Clamp(value, 0, 100));
        var result = await Task.Run(() => CryptoHelpers.RunNewDes(text, key, token, progress), token);
        newDesOutputBox.Text =
            $"Потік: {result.ThreadId}\r\n" +
            $"Час: {result.Duration.TotalMilliseconds:0} мс\r\n" +
            $"Блоків: {result.Blocks}\r\n" +
            $"Ключ 120 біт: {result.KeyHex}\r\n\r\n" +
            $"Шифротекст:\r\n{result.CipherHex}\r\n\r\n" +
            $"Розшифровано:\r\n{result.PlainText}";
    }

    private async Task RunMd5Async(CancellationToken token)
    {
        var text = inputBox.Text;
        md5OutputBox.Text = "Виконується...";
        md5Progress.Value = 0;
        var progress = new Progress<int>(value => md5Progress.Value = Math.Clamp(value, 0, 100));
        var result = await Task.Run(() => CryptoHelpers.RunMd5(text, token, progress), token);
        md5OutputBox.Text =
            $"Потік: {result.ThreadId}\r\n" +
            $"Час: {result.Duration.TotalMilliseconds:0} мс\r\n" +
            $"Байтів: {result.Bytes}\r\n\r\n" +
            $"MD5:\r\n{result.HashHex}";
    }

    private async Task RunElGamalAsync(CancellationToken token)
    {
        var text = inputBox.Text;
        elGamalOutputBox.Text = "Виконується...";
        elGamalProgress.Value = 0;
        var progress = new Progress<int>(value => elGamalProgress.Value = Math.Clamp(value, 0, 100));
        var result = await Task.Run(() => CryptoHelpers.RunElGamal(text, token, progress), token);
        elGamalOutputBox.Text =
            $"Потік: {result.ThreadId}\r\n" +
            $"Час: {result.Duration.TotalMilliseconds:0} мс\r\n" +
            $"p: {result.P}\r\n" +
            $"g: {result.G}\r\n" +
            $"Закритий ключ x: {result.PrivateKey}\r\n" +
            $"Відкритий ключ y: {result.PublicKey}\r\n\r\n" +
            $"Шифротекст:\r\n{result.CipherText}\r\n\r\n" +
            $"Розшифровано:\r\n{result.PlainText}";
    }

    private void ResetProgress()
    {
        newDesProgress.Value = 0;
        md5Progress.Value = 0;
        elGamalProgress.Value = 0;
    }

    private void SetBusy(bool value)
    {
        busy = value;
        runAllButton.Enabled = !value;
        runNewDesButton.Enabled = !value;
        runMd5Button.Enabled = !value;
        runElGamalButton.Enabled = !value;
        inputBox.ReadOnly = value;
        keyBox.ReadOnly = value;
        cancelButton.Enabled = value;
    }
}
