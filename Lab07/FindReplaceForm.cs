namespace Lab07;

internal sealed class FindReplaceForm : Form
{
    private static readonly Color Surface = Color.FromArgb(249, 250, 247);
    private static readonly Color Ink = Color.FromArgb(34, 38, 36);
    private static readonly Color Muted = Color.FromArgb(93, 103, 98);
    private static readonly Color Accent = Color.FromArgb(0, 126, 122);
    private static readonly Color AccentDeep = Color.FromArgb(0, 91, 89);

    private readonly MainForm ownerForm;
    private readonly Label findLabel = new();
    private readonly Label replaceLabel = new();
    private readonly TextBox findBox = new();
    private readonly TextBox replaceBox = new();
    private readonly CheckBox matchCaseBox = new();
    private readonly CheckBox wholeWordBox = new();
    private readonly Button findButton = new();
    private readonly Button replaceButton = new();
    private readonly Button replaceAllButton = new();
    private readonly Button closeButton = new();
    private UiLanguage language;

    public FindReplaceForm(MainForm owner, UiLanguage currentLanguage)
    {
        ownerForm = owner;
        language = currentLanguage;
        InitializeForm();
        ApplyLanguage(currentLanguage);
    }

    public void ApplyLanguage(UiLanguage currentLanguage)
    {
        language = currentLanguage;
        Text = TextCatalog.T(language, "findTitle");
        findLabel.Text = TextCatalog.T(language, "findText");
        replaceLabel.Text = TextCatalog.T(language, "replaceText");
        matchCaseBox.Text = TextCatalog.T(language, "matchCase");
        wholeWordBox.Text = TextCatalog.T(language, "wholeWord");
        findButton.Text = TextCatalog.T(language, "findNext");
        replaceButton.Text = TextCatalog.T(language, "replaceOne");
        replaceAllButton.Text = TextCatalog.T(language, "replaceAll");
        closeButton.Text = TextCatalog.T(language, "closeDialog");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnFormClosing(e);
    }

    private void InitializeForm()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(546, 214);
        BackColor = Surface;
        Font = new Font("Segoe UI", 10f);

        findLabel.SetBounds(20, 28, 132, 24);
        findBox.SetBounds(156, 25, 360, 28);
        replaceLabel.SetBounds(20, 68, 132, 24);
        replaceBox.SetBounds(156, 65, 360, 28);
        matchCaseBox.SetBounds(156, 104, 190, 26);
        wholeWordBox.SetBounds(350, 104, 170, 26);
        findButton.SetBounds(20, 154, 122, 34);
        replaceButton.SetBounds(150, 154, 122, 34);
        replaceAllButton.SetBounds(280, 154, 122, 34);
        closeButton.SetBounds(410, 154, 106, 34);

        findLabel.ForeColor = Muted;
        replaceLabel.ForeColor = Muted;
        findBox.BorderStyle = BorderStyle.FixedSingle;
        replaceBox.BorderStyle = BorderStyle.FixedSingle;
        findBox.BackColor = Color.FromArgb(255, 255, 252);
        replaceBox.BackColor = Color.FromArgb(255, 255, 252);
        findBox.ForeColor = Ink;
        replaceBox.ForeColor = Ink;
        matchCaseBox.ForeColor = Ink;
        wholeWordBox.ForeColor = Ink;
        StyleButton(findButton, true);
        StyleButton(replaceButton, false);
        StyleButton(replaceAllButton, false);
        StyleButton(closeButton, false);

        findButton.Click += (_, _) => FindNext();
        replaceButton.Click += (_, _) => ReplaceOne();
        replaceAllButton.Click += (_, _) => ReplaceAll();
        closeButton.Click += (_, _) => Hide();
        AcceptButton = findButton;
        CancelButton = closeButton;

        Controls.AddRange(new Control[]
        {
            findLabel,
            findBox,
            replaceLabel,
            replaceBox,
            matchCaseBox,
            wholeWordBox,
            findButton,
            replaceButton,
            replaceAllButton,
            closeButton
        });
    }

    private static void StyleButton(Button button, bool primary)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
        button.BackColor = primary ? Accent : Color.FromArgb(239, 244, 240);
        button.ForeColor = primary ? Color.White : Ink;
        button.FlatAppearance.MouseOverBackColor = primary ? AccentDeep : Color.FromArgb(222, 235, 229);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(188, 82, 62);
    }

    private EditorDocument? CurrentDocument()
    {
        return ownerForm.ActiveDocument;
    }

    private bool ValidateFindText()
    {
        if (!string.IsNullOrEmpty(findBox.Text))
        {
            return true;
        }

        MessageBox.Show(this, TextCatalog.T(language, "emptyFind"), TextCatalog.T(language, "findTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        findBox.Focus();
        return false;
    }

    private void FindNext()
    {
        if (!ValidateFindText())
        {
            return;
        }

        var document = CurrentDocument();

        if (document is null || !document.FindText(findBox.Text, matchCaseBox.Checked, wholeWordBox.Checked))
        {
            MessageBox.Show(this, TextCatalog.T(language, "notFound"), TextCatalog.T(language, "findTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void ReplaceOne()
    {
        if (!ValidateFindText())
        {
            return;
        }

        var document = CurrentDocument();

        if (document is null || !document.ReplaceCurrent(findBox.Text, replaceBox.Text, matchCaseBox.Checked, wholeWordBox.Checked))
        {
            MessageBox.Show(this, TextCatalog.T(language, "notFound"), TextCatalog.T(language, "findTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void ReplaceAll()
    {
        if (!ValidateFindText())
        {
            return;
        }

        var document = CurrentDocument();
        var count = document?.ReplaceAll(findBox.Text, replaceBox.Text, matchCaseBox.Checked, wholeWordBox.Checked) ?? 0;
        MessageBox.Show(this, string.Format(TextCatalog.T(language, "replaceCount"), count), TextCatalog.T(language, "findTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
