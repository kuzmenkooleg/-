namespace Lab07;

internal sealed class MainForm : Form
{
    private readonly MenuStrip menuStrip = new();
    private readonly StatusStrip statusStrip = new();
    private readonly ToolStripStatusLabel statusLabel = new();
    private readonly ToolStripStatusLabel positionLabel = new();
    private readonly ToolStripStatusLabel infoLabel = new();
    private readonly Panel leftPanel = new();
    private readonly FlowLayoutPanel buttonPanel = new();
    private readonly ComboBox languageBox = new();
    private readonly CheckBox syntaxBox = new();
    private readonly CheckBox wordWrapBox = new();
    private readonly Dictionary<ToolStripItem, string> menuKeys = new();
    private readonly Dictionary<Control, string> controlKeys = new();
    private readonly Dictionary<Control, bool> documentControls = new();
    private FindReplaceForm? findForm;
    private UiLanguage language = UiLanguage.Ukrainian;
    private bool updatingLanguage;
    private int documentCounter;

    public MainForm()
    {
        IsMdiContainer = true;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(980, 640);
        Font = new Font("Segoe UI", 10f);
        BackColor = Color.FromArgb(230, 232, 235);
        InitializeMenu();
        InitializeLeftPanel();
        InitializeStatus();
        ApplyLanguage();
        MdiChildActivate += (_, _) => UpdateState();
        Shown += (_, _) =>
        {
            ConfigureMdiClient();

            if (MdiChildren.Length == 0)
            {
                CreateDocument();
            }
        };
    }

    public EditorDocument? ActiveDocument => ActiveMdiChild as EditorDocument;

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.N))
        {
            CreateDocument();
            return true;
        }

        if (keyData == (Keys.Control | Keys.O))
        {
            OpenDocument();
            return true;
        }

        if (keyData == (Keys.Control | Keys.S))
        {
            SaveActiveDocument();
            return true;
        }

        if (keyData == (Keys.Control | Keys.Shift | Keys.S))
        {
            SaveActiveDocumentAs();
            return true;
        }

        if (keyData == (Keys.Control | Keys.P))
        {
            PrintActiveDocument(false);
            return true;
        }

        if (keyData == (Keys.Control | Keys.W))
        {
            ActiveDocument?.Close();
            return true;
        }

        if (keyData == (Keys.Control | Keys.F) || keyData == (Keys.Control | Keys.H))
        {
            ShowFindDialog();
            return true;
        }

        if (keyData == (Keys.Control | Keys.B))
        {
            ActiveDocument?.ToggleStyle(FontStyle.Bold);
            return true;
        }

        if (keyData == (Keys.Control | Keys.I))
        {
            ActiveDocument?.ToggleStyle(FontStyle.Italic);
            return true;
        }

        if (keyData == (Keys.Control | Keys.U))
        {
            ActiveDocument?.ToggleStyle(FontStyle.Underline);
            return true;
        }

        if (keyData == (Keys.Control | Keys.L))
        {
            ActiveDocument?.SetAlignment(HorizontalAlignment.Left);
            return true;
        }

        if (keyData == (Keys.Control | Keys.E))
        {
            ActiveDocument?.SetAlignment(HorizontalAlignment.Center);
            return true;
        }

        if (keyData == (Keys.Control | Keys.R))
        {
            ActiveDocument?.SetAlignment(HorizontalAlignment.Right);
            return true;
        }

        if (keyData == (Keys.Control | Keys.Shift | Keys.I))
        {
            InsertImage();
            return true;
        }

        if (keyData == (Keys.Control | Keys.Oemplus) || keyData == (Keys.Control | Keys.Add))
        {
            ActiveDocument?.ZoomIn();
            return true;
        }

        if (keyData == (Keys.Control | Keys.OemMinus) || keyData == (Keys.Control | Keys.Subtract))
        {
            ActiveDocument?.ZoomOut();
            return true;
        }

        if (keyData == (Keys.Control | Keys.D0) || keyData == (Keys.Control | Keys.NumPad0))
        {
            ActiveDocument?.ZoomReset();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private string T(string key)
    {
        return TextCatalog.T(language, key);
    }

    private void InitializeMenu()
    {
        MainMenuStrip = menuStrip;
        menuStrip.Dock = DockStyle.Top;
        menuStrip.BackColor = Color.White;
        Controls.Add(menuStrip);

        var fileMenu = Menu("file");
        fileMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("new", (_, _) => CreateDocument(), Keys.Control | Keys.N),
            Item("open", (_, _) => OpenDocument(), Keys.Control | Keys.O),
            new ToolStripSeparator(),
            Item("save", (_, _) => SaveActiveDocument(), Keys.Control | Keys.S),
            Item("saveAs", (_, _) => SaveActiveDocumentAs(), Keys.Control | Keys.Shift | Keys.S),
            Item("saveAll", (_, _) => SaveAllDocuments()),
            new ToolStripSeparator(),
            Item("print", (_, _) => PrintActiveDocument(false), Keys.Control | Keys.P),
            Item("printPreview", (_, _) => PrintActiveDocument(true)),
            new ToolStripSeparator(),
            Item("close", (_, _) => ActiveDocument?.Close(), Keys.Control | Keys.W),
            Item("exit", (_, _) => Close(), Keys.Alt | Keys.F4)
        });

        var editMenu = Menu("edit");
        editMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("undo", (_, _) => ActiveDocument?.Editor.Undo(), Keys.Control | Keys.Z),
            Item("redo", (_, _) => ActiveDocument?.Editor.Redo(), Keys.Control | Keys.Y),
            new ToolStripSeparator(),
            Item("cut", (_, _) => ActiveDocument?.Editor.Cut(), Keys.Control | Keys.X),
            Item("copy", (_, _) => ActiveDocument?.Editor.Copy(), Keys.Control | Keys.C),
            Item("paste", (_, _) => ActiveDocument?.Editor.Paste(), Keys.Control | Keys.V),
            Item("selectAll", (_, _) => ActiveDocument?.Editor.SelectAll(), Keys.Control | Keys.A),
            new ToolStripSeparator(),
            Item("find", (_, _) => ShowFindDialog(), Keys.Control | Keys.F),
            Item("replace", (_, _) => ShowFindDialog(), Keys.Control | Keys.H)
        });

        var alignMenu = Menu("align");
        alignMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("left", (_, _) => ActiveDocument?.SetAlignment(HorizontalAlignment.Left), Keys.Control | Keys.L),
            Item("center", (_, _) => ActiveDocument?.SetAlignment(HorizontalAlignment.Center), Keys.Control | Keys.E),
            Item("right", (_, _) => ActiveDocument?.SetAlignment(HorizontalAlignment.Right), Keys.Control | Keys.R)
        });

        var formatMenu = Menu("format");
        formatMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("font", (_, _) => ActiveDocument?.ApplyFontToSelection(), Keys.Control | Keys.Shift | Keys.F),
            Item("color", (_, _) => ActiveDocument?.ApplyColorToSelection()),
            new ToolStripSeparator(),
            Item("bold", (_, _) => ActiveDocument?.ToggleStyle(FontStyle.Bold), Keys.Control | Keys.B),
            Item("italic", (_, _) => ActiveDocument?.ToggleStyle(FontStyle.Italic), Keys.Control | Keys.I),
            Item("underline", (_, _) => ActiveDocument?.ToggleStyle(FontStyle.Underline), Keys.Control | Keys.U),
            new ToolStripSeparator(),
            alignMenu,
            Item("bullets", (_, _) => ActiveDocument?.ToggleBullets())
        });

        var insertMenu = Menu("insert");
        insertMenu.DropDownItems.Add(Item("image", (_, _) => InsertImage(), Keys.Control | Keys.Shift | Keys.I));

        var toolsMenu = Menu("tools");
        toolsMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("syntax", (_, _) => ToggleSyntax()),
            Item("wordWrap", (_, _) => SetWordWrap(!wordWrapBox.Checked)),
            new ToolStripSeparator(),
            Item("zoomIn", (_, _) => ActiveDocument?.ZoomIn(), Keys.Control | Keys.Oemplus),
            Item("zoomOut", (_, _) => ActiveDocument?.ZoomOut(), Keys.Control | Keys.OemMinus),
            Item("zoomReset", (_, _) => ActiveDocument?.ZoomReset(), Keys.Control | Keys.D0)
        });

        var languageMenu = Menu("language");
        languageMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("ukrainian", (_, _) => ChangeLanguage(UiLanguage.Ukrainian)),
            Item("english", (_, _) => ChangeLanguage(UiLanguage.English)),
            Item("polish", (_, _) => ChangeLanguage(UiLanguage.Polish))
        });

        var windowMenu = Menu("window");
        windowMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("cascade", (_, _) => LayoutMdi(MdiLayout.Cascade)),
            Item("tileHorizontal", (_, _) => LayoutMdi(MdiLayout.TileHorizontal)),
            Item("tileVertical", (_, _) => LayoutMdi(MdiLayout.TileVertical)),
            Item("arrangeIcons", (_, _) => LayoutMdi(MdiLayout.ArrangeIcons))
        });

        menuStrip.MdiWindowListItem = windowMenu;
        menuStrip.Items.AddRange(new ToolStripItem[]
        {
            fileMenu,
            editMenu,
            formatMenu,
            insertMenu,
            toolsMenu,
            languageMenu,
            windowMenu
        });
    }

    private void InitializeLeftPanel()
    {
        leftPanel.Dock = DockStyle.Left;
        leftPanel.Width = 178;
        leftPanel.BackColor = Color.FromArgb(245, 246, 248);
        leftPanel.Padding = new Padding(12);
        Controls.Add(leftPanel);

        buttonPanel.Dock = DockStyle.Fill;
        buttonPanel.FlowDirection = FlowDirection.TopDown;
        buttonPanel.WrapContents = false;
        buttonPanel.AutoScroll = true;
        buttonPanel.BackColor = Color.Transparent;
        leftPanel.Controls.Add(buttonPanel);

        buttonPanel.Controls.Add(ActionButton("new", (_, _) => CreateDocument(), false, true));
        buttonPanel.Controls.Add(ActionButton("open", (_, _) => OpenDocument(), false, true));
        buttonPanel.Controls.Add(ActionButton("save", (_, _) => SaveActiveDocument()));
        buttonPanel.Controls.Add(ActionButton("saveAs", (_, _) => SaveActiveDocumentAs()));
        buttonPanel.Controls.Add(Spacer());
        buttonPanel.Controls.Add(ActionButton("find", (_, _) => ShowFindDialog()));
        buttonPanel.Controls.Add(ActionButton("image", (_, _) => InsertImage()));
        buttonPanel.Controls.Add(ActionButton("font", (_, _) => ActiveDocument?.ApplyFontToSelection()));
        buttonPanel.Controls.Add(ActionButton("color", (_, _) => ActiveDocument?.ApplyColorToSelection()));
        buttonPanel.Controls.Add(Spacer());
        buttonPanel.Controls.Add(ActionButton("bold", (_, _) => ActiveDocument?.ToggleStyle(FontStyle.Bold)));
        buttonPanel.Controls.Add(ActionButton("italic", (_, _) => ActiveDocument?.ToggleStyle(FontStyle.Italic)));
        buttonPanel.Controls.Add(ActionButton("underline", (_, _) => ActiveDocument?.ToggleStyle(FontStyle.Underline)));
        buttonPanel.Controls.Add(ActionButton("left", (_, _) => ActiveDocument?.SetAlignment(HorizontalAlignment.Left)));
        buttonPanel.Controls.Add(ActionButton("center", (_, _) => ActiveDocument?.SetAlignment(HorizontalAlignment.Center)));
        buttonPanel.Controls.Add(ActionButton("right", (_, _) => ActiveDocument?.SetAlignment(HorizontalAlignment.Right)));
        buttonPanel.Controls.Add(Spacer());

        syntaxBox.Appearance = Appearance.Button;
        syntaxBox.Checked = true;
        syntaxBox.Width = 145;
        syntaxBox.Height = 34;
        syntaxBox.Margin = new Padding(0, 2, 0, 6);
        syntaxBox.FlatStyle = FlatStyle.Flat;
        syntaxBox.TextAlign = ContentAlignment.MiddleCenter;
        syntaxBox.CheckedChanged += SyntaxChanged;
        Register(syntaxBox, "syntax");
        buttonPanel.Controls.Add(syntaxBox);

        wordWrapBox.Appearance = Appearance.Button;
        wordWrapBox.Checked = true;
        wordWrapBox.Width = 145;
        wordWrapBox.Height = 34;
        wordWrapBox.Margin = new Padding(0, 2, 0, 6);
        wordWrapBox.FlatStyle = FlatStyle.Flat;
        wordWrapBox.TextAlign = ContentAlignment.MiddleCenter;
        wordWrapBox.CheckedChanged += WordWrapChanged;
        Register(wordWrapBox, "wordWrap");
        buttonPanel.Controls.Add(wordWrapBox);

        languageBox.DropDownStyle = ComboBoxStyle.DropDownList;
        languageBox.Width = 145;
        languageBox.Margin = new Padding(0, 8, 0, 0);
        languageBox.SelectedIndexChanged += (_, _) =>
        {
            if (updatingLanguage)
            {
                return;
            }

            ChangeLanguage(languageBox.SelectedIndex switch
            {
                1 => UiLanguage.English,
                2 => UiLanguage.Polish,
                _ => UiLanguage.Ukrainian
            });
        };
        buttonPanel.Controls.Add(languageBox);
    }

    private void InitializeStatus()
    {
        statusStrip.Dock = DockStyle.Bottom;
        statusStrip.BackColor = Color.White;
        statusLabel.Spring = true;
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        positionLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        infoLabel.BorderSides = ToolStripStatusLabelBorderSides.Left;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, positionLabel, infoLabel });
        Controls.Add(statusStrip);
    }

    private ToolStripMenuItem Menu(string key)
    {
        var item = new ToolStripMenuItem();
        Register(item, key);
        return item;
    }

    private ToolStripMenuItem Item(string key, EventHandler handler, Keys shortcut = Keys.None)
    {
        var item = new ToolStripMenuItem
        {
            ShortcutKeys = shortcut
        };
        item.Click += handler;
        Register(item, key);
        return item;
    }

    private Button ActionButton(string key, EventHandler handler, bool needsDocument = true, bool main = false)
    {
        var button = new Button
        {
            Width = 145,
            Height = 34,
            Margin = new Padding(0, 2, 0, 6),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand,
            Tag = main
        };

        button.FlatAppearance.BorderSize = 1;
        button.Click += handler;
        Register(button, key);
        documentControls[button] = needsDocument;
        StyleButton(button);
        return button;
    }

    private Label Spacer()
    {
        return new Label
        {
            Width = 145,
            Height = 8,
            Margin = new Padding(0),
            BackColor = Color.Transparent
        };
    }

    private void Register(ToolStripItem item, string key)
    {
        menuKeys[item] = key;
    }

    private void Register(Control control, string key)
    {
        controlKeys[control] = key;
    }

    private void ApplyLanguage()
    {
        Text = T("appTitle");

        foreach (var pair in menuKeys)
        {
            pair.Key.Text = T(pair.Value);
        }

        foreach (var pair in controlKeys)
        {
            pair.Key.Text = T(pair.Value);
        }

        updatingLanguage = true;
        languageBox.Items.Clear();
        languageBox.Items.Add(T("ukrainian"));
        languageBox.Items.Add(T("english"));
        languageBox.Items.Add(T("polish"));
        languageBox.SelectedIndex = language switch
        {
            UiLanguage.English => 1,
            UiLanguage.Polish => 2,
            _ => 0
        };
        updatingLanguage = false;

        foreach (var document in MdiChildren.OfType<EditorDocument>())
        {
            document.ApplyLanguage(language);
        }

        findForm?.ApplyLanguage(language);
        UpdateState();
    }

    private EditorDocument CreateEditor()
    {
        documentCounter++;
        var document = new EditorDocument(documentCounter, language)
        {
            MdiParent = this,
            MinimumSize = new Size(420, 300)
        };
        document.Editor.WordWrap = wordWrapBox.Checked;
        document.SyntaxEnabled = syntaxBox.Checked;
        document.DocumentStateChanged += (_, _) => UpdateState();
        document.FormClosed += (_, _) => UpdateState();
        return document;
    }

    private void CreateDocument()
    {
        var document = CreateEditor();
        document.NewDocument();
        document.Show();
        document.Editor.Focus();
        UpdateState();
    }

    private void OpenDocument()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = T("openFilter"),
            Multiselect = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        foreach (var path in dialog.FileNames)
        {
            try
            {
                var document = CreateEditor();
                document.LoadDocument(path);
                document.Show();
            }
            catch
            {
                MessageBox.Show(this, T("openError"), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        UpdateState();
    }

    private bool SaveActiveDocument()
    {
        var document = ActiveDocument;

        if (document is null)
        {
            return false;
        }

        if (document.FilePath is null)
        {
            return SaveActiveDocumentAs();
        }

        try
        {
            document.Save();
            return true;
        }
        catch
        {
            MessageBox.Show(this, T("saveError"), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private bool SaveActiveDocumentAs()
    {
        var document = ActiveDocument;

        if (document is null)
        {
            return false;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = T("saveFilter"),
            FilterIndex = 1,
            DefaultExt = "rtf",
            AddExtension = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        try
        {
            document.SaveAs(dialog.FileName);
            return true;
        }
        catch
        {
            MessageBox.Show(this, T("saveError"), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void SaveAllDocuments()
    {
        foreach (var document in MdiChildren.OfType<EditorDocument>())
        {
            document.Activate();

            if (!SaveActiveDocument())
            {
                break;
            }
        }
    }

    private void InsertImage()
    {
        var document = ActiveDocument;

        if (document is null)
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = T("imageFilter")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            document.InsertImage(dialog.FileName);
        }
        catch
        {
            MessageBox.Show(this, T("imageError"), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintActiveDocument(bool preview)
    {
        var document = ActiveDocument;

        if (document is null)
        {
            return;
        }

        try
        {
            document.PrintDocument(preview);
        }
        catch
        {
            MessageBox.Show(this, T("printError"), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowFindDialog()
    {
        findForm ??= new FindReplaceForm(this, language);
        findForm.ApplyLanguage(language);
        findForm.Show(this);
        findForm.Activate();
    }

    private void ToggleSyntax()
    {
        syntaxBox.CheckedChanged -= SyntaxChanged;
        syntaxBox.Checked = !syntaxBox.Checked;
        syntaxBox.CheckedChanged += SyntaxChanged;
        ApplySyntaxState();
    }

    private void SyntaxChanged(object? sender, EventArgs e)
    {
        ApplySyntaxState();
    }

    private void ApplySyntaxState()
    {
        foreach (var document in MdiChildren.OfType<EditorDocument>())
        {
            document.SyntaxEnabled = syntaxBox.Checked;

            if (syntaxBox.Checked)
            {
                document.ApplySyntaxHighlighting();
            }
        }

        StyleToggle(syntaxBox);
    }

    private void SetWordWrap(bool enabled)
    {
        wordWrapBox.CheckedChanged -= WordWrapChanged;
        wordWrapBox.Checked = enabled;
        wordWrapBox.CheckedChanged += WordWrapChanged;

        foreach (var document in MdiChildren.OfType<EditorDocument>())
        {
            document.Editor.WordWrap = enabled;
        }

        StyleToggle(wordWrapBox);
    }

    private void WordWrapChanged(object? sender, EventArgs e)
    {
        SetWordWrap(wordWrapBox.Checked);
    }

    private void ChangeLanguage(UiLanguage nextLanguage)
    {
        language = nextLanguage;
        ApplyLanguage();
    }

    private void UpdateState()
    {
        var document = ActiveDocument;
        var hasDocument = document is not null;

        foreach (var pair in documentControls)
        {
            pair.Key.Enabled = !pair.Value || hasDocument;
            StyleButton(pair.Key);
        }

        foreach (var pair in menuKeys)
        {
            pair.Key.Enabled = hasDocument || CanUseWithoutDocument(pair.Value);
        }

        StyleToggle(syntaxBox);
        StyleToggle(wordWrapBox);

        if (!hasDocument)
        {
            statusLabel.Text = T("statusNoDocument");
            positionLabel.Text = string.Empty;
            infoLabel.Text = string.Empty;
            return;
        }

        var position = document!.GetCaretPosition();
        statusLabel.Text = document.FilePath ?? T("untitled");
        positionLabel.Text = string.Format(T("lineColumn"), position.line, position.column);
        infoLabel.Text = string.Format(T("formatInfo"), document.Editor.TextLength);
    }

    private static bool CanUseWithoutDocument(string key)
    {
        return key is "file"
            or "new"
            or "open"
            or "exit"
            or "tools"
            or "syntax"
            or "wordWrap"
            or "language"
            or "ukrainian"
            or "english"
            or "polish"
            or "window"
            or "cascade"
            or "tileHorizontal"
            or "tileVertical"
            or "arrangeIcons";
    }

    private static void ConfigureMdiClient()
    {
        foreach (Form form in Application.OpenForms)
        {
            foreach (var control in form.Controls)
            {
                if (control is MdiClient client)
                {
                    client.BackColor = Color.FromArgb(230, 232, 235);
                }
            }
        }
    }

    private static void StyleButton(Control control)
    {
        if (control is not Button button)
        {
            return;
        }

        var main = button.Tag is true;
        button.FlatAppearance.BorderColor = Color.FromArgb(190, 196, 202);

        if (!button.Enabled)
        {
            button.BackColor = Color.FromArgb(226, 229, 232);
            button.ForeColor = Color.FromArgb(125, 132, 138);
            return;
        }

        button.BackColor = main ? Color.FromArgb(40, 110, 175) : Color.White;
        button.ForeColor = main ? Color.White : Color.FromArgb(25, 30, 35);
        button.FlatAppearance.MouseOverBackColor = main ? Color.FromArgb(32, 92, 150) : Color.FromArgb(235, 241, 248);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(207, 223, 241);
    }

    private static void StyleToggle(CheckBox box)
    {
        box.FlatAppearance.BorderColor = Color.FromArgb(190, 196, 202);
        box.BackColor = box.Checked ? Color.FromArgb(40, 110, 175) : Color.White;
        box.ForeColor = box.Checked ? Color.White : Color.FromArgb(25, 30, 35);
    }
}
