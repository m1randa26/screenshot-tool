using CapturaTool.Platform;

namespace CapturaTool.Configuration;

/// <summary>
/// Ventana de configuración: carpeta de guardado (R28) y arranque con Windows (R30).
/// Nada cambia hasta presionar Guardar.
/// </summary>
public class SettingsForm : Form
{
    private readonly string _settingsPath;
    private readonly AutoStart _autoStart;
    private readonly string _exePath;
    private readonly Logger _logger;

    private readonly TextBox _folderBox;
    private readonly CheckBox _startWithWindowsBox;

    /// <summary>Se dispara cuando la configuración nueva quedó guardada en el archivo.</summary>
    public event EventHandler<AppSettings>? Saved;

    public SettingsForm(AppSettings current, string settingsPath, AutoStart autoStart, string exePath, Logger logger)
    {
        _settingsPath = settingsPath;
        _autoStart = autoStart;
        _exePath = exePath;
        _logger = logger;

        SuspendLayout();

        Text = "CapturaTool: Configuración";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
        };

        var folderLabel = new Label
        {
            Text = "Carpeta de guardado:",
            AutoSize = true,
            Margin = new Padding(3, 3, 3, 6),
        };

        _folderBox = new TextBox
        {
            Text = current.SaveFolder,
            ReadOnly = true,
            TabStop = false, // solo muestra la ruta; el foco empieza en "Cambiar…"
            Width = 380,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
        };

        var changeButton = new Button { Text = "Cambiar…", AutoSize = true };
        changeButton.Click += (sender, e) => ChooseFolder();

        _startWithWindowsBox = new CheckBox
        {
            Text = "Iniciar con Windows",
            Checked = current.StartWithWindows,
            AutoSize = true,
            Margin = new Padding(3, 12, 3, 3),
        };

        var saveButton = new Button { Text = "Guardar", AutoSize = true };
        saveButton.Click += (sender, e) => Save();

        var cancelButton = new Button { Text = "Cancelar", AutoSize = true, DialogResult = DialogResult.Cancel };

        // Botones abajo a la derecha: Guardar, Cancelar
        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Right,
            Margin = new Padding(0, 16, 0, 0),
        };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        layout.Controls.Add(folderLabel, 0, 0);
        layout.SetColumnSpan(folderLabel, 2);
        layout.Controls.Add(_folderBox, 0, 1);
        layout.Controls.Add(changeButton, 1, 1);
        layout.Controls.Add(_startWithWindowsBox, 0, 2);
        layout.SetColumnSpan(_startWithWindowsBox, 2);
        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);

        AcceptButton = saveButton;   // Enter
        CancelButton = cancelButton; // Esc

        // Las medidas de arriba están pensadas a 96 DPI; WinForms las escala al monitor real
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ResumeLayout();
    }

    /// <summary>Abre el selector de carpetas de Windows (flujo F4).</summary>
    private void ChooseFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Elige la carpeta donde se guardan las capturas",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
        };
        if (Directory.Exists(_folderBox.Text))
        {
            dialog.InitialDirectory = _folderBox.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _folderBox.Text = dialog.SelectedPath;
        }
    }

    private void Save()
    {
        var updated = new AppSettings
        {
            SaveFolder = _folderBox.Text,
            StartWithWindows = _startWithWindowsBox.Checked,
        };

        if (!updated.TrySave(_settingsPath, _logger, out string error))
        {
            // La ventana sigue abierta para intentar de nuevo
            MessageBox.Show(this, error, "CapturaTool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Saved?.Invoke(this, updated);

        if (!_autoStart.TryApply(updated.StartWithWindows, _exePath))
        {
            MessageBox.Show(
                this,
                "La configuración se guardó, pero no se pudo cambiar el arranque con Windows.\n\nEl detalle quedó en el log.",
                "CapturaTool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        Close();
    }
}
