using CapturaTool.Output;
using CapturaTool.Platform;

namespace CapturaTool.Editor;

/// <summary>
/// Ventana del editor. Es dueña de la captura: la libera al cerrarse.
/// </summary>
public class EditorForm : Form
{
    private readonly Bitmap _image;
    private readonly EditorCanvas _canvas;
    private readonly Rectangle _workArea; // área útil del monitor donde se abre
    private readonly ClipboardWriter _clipboard;
    private readonly Notifier _notifier;

    public EditorForm(Bitmap image, ClipboardWriter clipboard, Notifier notifier)
    {
        _image = image;
        _clipboard = clipboard;
        _notifier = notifier;

        Text = "CapturaTool";
        StartPosition = FormStartPosition.Manual;
        MinimumSize = new Size(400, 300);

        _canvas = new EditorCanvas(image) { Dock = DockStyle.Fill };
        Controls.Add(_canvas);

        // Se abre en el monitor donde está el mouse, que es donde se acaba de seleccionar.
        // La posición se fija ya para que la ventana se cree en ese monitor (con su escalado).
        _workArea = Screen.FromPoint(Cursor.Position).WorkingArea;
        Location = _workArea.Location;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Aquí la ventana ya existe en su monitor, así que el tamaño de los bordes es el correcto
        PlaceOnScreen();
    }

    private void PlaceOnScreen()
    {
        // Tamaño de ventana necesario para mostrar la captura a tamaño real
        Size needed = SizeFromClientSize(_image.Size);

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

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.C))
        {
            CopyToClipboard();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void CopyToClipboard()
    {
        // Por ahora se copia la captura sin anotaciones; en T12 se copiará con ellas
        if (_clipboard.TryCopy(_image))
        {
            // R21: el aviso aparece solo si la copia se verificó
            _notifier.Info("CapturaTool", "Copiado al portapapeles");
            Close();
        }
        else
        {
            // R22: no se cierra el editor, para no perder la captura
            MessageBox.Show(
                this,
                "No se pudo copiar al portapapeles porque otro programa lo está usando.\n\nIntenta de nuevo con Ctrl + C.",
                "CapturaTool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); // libera primero el lienzo, que usa la imagen
        if (disposing)
        {
            _image.Dispose();
        }
    }
}
