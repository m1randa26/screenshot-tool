namespace CapturaTool.Editor;

/// <summary>
/// Ventana del editor. Es dueña de la captura: la libera al cerrarse.
/// </summary>
public class EditorForm : Form
{
    private readonly Bitmap _image;
    private readonly EditorCanvas _canvas;
    private readonly Rectangle _workArea; // área útil del monitor donde se abre

    public EditorForm(Bitmap image)
    {
        _image = image;

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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); // libera primero el lienzo, que usa la imagen
        if (disposing)
        {
            _image.Dispose();
        }
    }
}
