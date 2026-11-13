using CapturaTool.Output;
using CapturaTool.Platform;

namespace CapturaTool.Editor;

/// <summary>
/// Ventana del editor: barra de herramientas, paleta y lienzo.
/// Es dueña del documento (que a su vez es dueño de la captura): lo libera al cerrarse.
/// </summary>
public class EditorForm : Form
{
    private readonly AnnotationDocument _document;
    private readonly EditorCanvas _canvas;
    private readonly ToolStrip _toolStrip;
    private readonly Rectangle _workArea; // área útil del monitor donde se abre
    private readonly ClipboardWriter _clipboard;
    private readonly FileSaver _saver;
    private readonly Notifier _notifier;

    // true después de copiar (o guardar) con éxito: cerrar ya no pierde nada (R27)
    private bool _outputDone;

    // Para restaurar la ventana como estaba (normal o maximizada) si se minimizó (R10)
    private FormWindowState _stateBeforeMinimize = FormWindowState.Normal;

    // Botones de la barra, para marcar cuál está elegido
    private readonly Dictionary<Tool, ToolStripButton> _toolButtons = new();
    private readonly Dictionary<Color, ToolStripButton> _colorButtons = new();
    private readonly List<Bitmap> _swatches = new(); // cuadritos de color; dueño: este formulario

    public EditorForm(Bitmap image, ClipboardWriter clipboard, FileSaver saver, Notifier notifier, Icon appIcon)
    {
        _document = new AnnotationDocument(image); // el documento pasa a ser dueño de la imagen
        _clipboard = clipboard;
        _saver = saver;
        _notifier = notifier;

        Text = AppIdentity.Current.DisplayName;
        Icon = appIcon; // prestado: lo libera TrayApp (R33)
        StartPosition = FormStartPosition.Manual;
        MinimumSize = new Size(400, 300);

        _canvas = new EditorCanvas(_document) { Dock = DockStyle.Fill };
        _toolStrip = CreateToolStrip();

        // El lienzo se agrega primero para que la barra quede arriba y el lienzo ocupe el resto
        Controls.Add(_canvas);
        Controls.Add(_toolStrip);
        UpdateToolStrip();

        // Se abre en el monitor donde está el mouse, que es donde se acaba de seleccionar.
        // La posición se fija ya para que la ventana se cree en ese monitor (con su escalado).
        _workArea = Screen.FromPoint(Cursor.Position).WorkingArea;
        Location = _workArea.Location;
    }

    private ToolStrip CreateToolStrip()
    {
        var strip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };

        // NF2: sin esto la barra se reacomoda con cada botón agregado (medido: 50–160 ms en total)
        strip.SuspendLayout();

        AddToolButton(strip, Tool.Rectangle, "Rectángulo");
        AddToolButton(strip, Tool.Ellipse, "Círculo");
        AddToolButton(strip, Tool.Arrow, "Flecha");
        AddToolButton(strip, Tool.Highlighter, "Resaltador");
        AddToolButton(strip, Tool.Text, "Texto");
        strip.Items.Add(new ToolStripSeparator());

        // Paleta (R13)
        foreach ((string name, Color color) in Palette.Colors)
        {
            Bitmap swatch = CreateSwatch(color);
            _swatches.Add(swatch);

            var button = new ToolStripButton
            {
                Image = swatch,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                ToolTipText = name,
            };
            button.Click += (sender, e) => SelectColor(color);
            strip.Items.Add(button);
            _colorButtons[color] = button;
        }

        strip.ResumeLayout(); // se acomoda una sola vez, con todos los botones
        return strip;
    }

    private void AddToolButton(ToolStrip strip, Tool tool, string text)
    {
        var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        button.Click += (sender, e) => SelectTool(tool);
        strip.Items.Add(button);
        _toolButtons[tool] = button;
    }

    /// <summary>Cuadrito de 16 × 16 con el color y un borde gris (para que el blanco se vea).</summary>
    private static Bitmap CreateSwatch(Color color)
    {
        var swatch = new Bitmap(16, 16);
        using Graphics g = Graphics.FromImage(swatch);
        using var fill = new SolidBrush(color);
        using var border = new Pen(Color.Gray);
        g.FillRectangle(fill, 0, 0, 16, 16);
        g.DrawRectangle(border, 0, 0, 15, 15);
        return swatch;
    }

    private void SelectTool(Tool tool)
    {
        _canvas.ConfirmText(); // cambiar de herramienta confirma el texto en curso (R14)
        _canvas.CurrentTool = tool;
        UpdateToolStrip();
    }

    private void SelectColor(Color color)
    {
        _canvas.CurrentColor = color;
        UpdateToolStrip();
    }

    /// <summary>Marca en la barra la herramienta y el color elegidos.</summary>
    private void UpdateToolStrip()
    {
        foreach ((Tool tool, ToolStripButton button) in _toolButtons)
        {
            button.Checked = tool == _canvas.CurrentTool;
        }
        foreach ((Color color, ToolStripButton button) in _colorButtons)
        {
            button.Checked = color == _canvas.CurrentColor;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Aquí la ventana ya existe en su monitor, así que el tamaño de los bordes es el correcto
        PlaceOnScreen();
    }

    private void PlaceOnScreen()
    {
        // Tamaño de ventana necesario para mostrar la captura a tamaño real, más la barra
        Size image = _document.BaseImage.Size;
        Size needed = SizeFromClientSize(new Size(image.Width, image.Height + _toolStrip.Height));

        if (needed.Width > _workArea.Width || needed.Height > _workArea.Height)
        {
            // No cabe: maximizada, y la captura se ajusta (R11)
            WindowState = FormWindowState.Maximized;
            return;
        }

        var size = new Size(
            Math.Max(needed.Width, MinimumSize.Width),
            Math.Max(needed.Height, MinimumSize.Height));
        Bounds = new Rectangle(
            _workArea.X + (_workArea.Width - size.Width) / 2,
            _workArea.Y + (_workArea.Height - size.Height) / 2,
            size.Width,
            size.Height);
    }

    /// <summary>
    /// Dibuja la ventana y el lienzo de inmediato, en lugar de esperar a que Windows lo pida.
    /// Se usa para medir cuándo el editor se ve de verdad (NF2).
    /// </summary>
    public void RenderNow()
    {
        Update();
        _canvas.Update();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_canvas.IsTyping)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    _canvas.ConfirmText(); // no se pierde lo escrito
                    CopyToClipboard();
                    return true;

                case Keys.Control | Keys.S:
                    _canvas.ConfirmText(); // no se pierde lo escrito
                    SaveToFile();
                    return true;

                case Keys.Control | Keys.Z:
                case Keys.Control | Keys.Y:
                    return true; // mientras se escribe no deshacen otras anotaciones; para letras, Retroceso
            }
        }

        switch (keyData)
        {
            case Keys.Escape:
                if (_canvas.IsTyping)
                {
                    break; // Esc es para el lienzo: cancela solo el texto (R14)
                }
                Close(); // igual que la X de la ventana: pregunta si hay anotaciones (R27)
                return true;

            case Keys.Control | Keys.C:
                CopyToClipboard();
                return true;

            case Keys.Control | Keys.S:
                SaveToFile();
                return true;

            case Keys.Control | Keys.Z: // R18
                if (_document.Undo())
                {
                    _canvas.DocumentChanged();
                }
                return true;

            case Keys.Control | Keys.Y: // R19
                if (_document.Redo())
                {
                    _canvas.DocumentChanged();
                }
                return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>Guarda la imagen final como PNG en la carpeta de capturas, sin copiarla (R23).</summary>
    private void SaveToFile()
    {
        bool saved;
        string path;
        string error;
        using (Bitmap finalImage = ImageRenderer.Render(_document))
        {
            saved = _saver.TrySave(finalImage, out path, out error);
        }

        if (saved)
        {
            // R26: aviso con el nombre del archivo
            _notifier.Info(AppIdentity.Current.DisplayName, "Guardado como " + Path.GetFileName(path));
            _outputDone = true;
            Close();
        }
        else
        {
            // R25: no se cierra el editor, para no perder la captura
            MessageBox.Show(
                this,
                error + "\n\nLa captura sigue abierta: puedes intentar de nuevo o copiarla con Ctrl + C.",
                AppIdentity.Current.DisplayName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void CopyToClipboard()
    {
        // R21: se copia la imagen final (captura + anotaciones) a resolución original.
        // El portapapeles guarda su propia copia, así que la imagen se libera al terminar.
        bool copied;
        using (Bitmap finalImage = ImageRenderer.Render(_document))
        {
            copied = _clipboard.TryCopy(finalImage);
        }

        if (copied)
        {
            // R21: el aviso aparece solo si la copia se verificó
            _notifier.Info(AppIdentity.Current.DisplayName, "Copiado al portapapeles");
            _outputDone = true;
            Close();
        }
        else
        {
            // R22: no se cierra el editor, para no perder la captura
            MessageBox.Show(
                this,
                "No se pudo copiar al portapapeles porque otro programa lo está usando.\n\nIntenta de nuevo con Ctrl + C, o guarda la captura con Ctrl + S.",
                AppIdentity.Current.DisplayName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    /// <summary>
    /// Trae el editor al frente (R10): lo restaura si estaba minimizado y lo activa.
    /// </summary>
    public void ShowAgain()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = _stateBeforeMinimize;
        }
        Activate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState != FormWindowState.Minimized)
        {
            _stateBeforeMinimize = WindowState;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // Solo cuando el usuario cierra (Esc o la X); no al apagar Windows, por ejemplo
        if (e.CloseReason != CloseReason.UserClosing)
        {
            return;
        }

        if (DiscardRule.MustAsk(_document.HasAnnotations, _canvas.HasUnconfirmedText, _outputDone))
        {
            // "No" viene marcado: un Enter sin pensar no hace perder el trabajo
            DialogResult answer = MessageBox.Show(
                this,
                "¿Descartar la captura?\n\nSe perderán las anotaciones.",
                AppIdentity.Current.DisplayName,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (answer != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); // libera primero el lienzo y la barra, que usan la imagen y los cuadritos
        if (disposing)
        {
            _document.Dispose();
            foreach (Bitmap swatch in _swatches)
            {
                swatch.Dispose();
            }
        }
    }
}
