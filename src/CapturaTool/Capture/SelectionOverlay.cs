namespace CapturaTool.Capture;

/// <summary>
/// Ventana que cubre un monitor con su imagen congelada oscurecida (R6).
/// Arrastrar = zona (R7), clic = monitor completo (R8), Esc = cancelar (R9).
/// Hay una por monitor; la imagen la sigue siendo de quien creó el overlay.
/// </summary>
public class SelectionOverlay : Form
{
    private readonly MonitorCapture _capture;
    private readonly Bitmap _darkened; // copia oscurecida, preparada una sola vez

    private bool _dragging;
    private Point _start;
    private Rectangle _selection = Rectangle.Empty; // en coordenadas del monitor

    /// <summary>Zona elegida, en coordenadas del monitor.</summary>
    public event EventHandler<Rectangle>? SelectionCompleted;

    /// <summary>El usuario canceló (Esc o cerró la ventana).</summary>
    public event EventHandler? Cancelled;

    public SelectionOverlay(MonitorCapture capture)
    {
        _capture = capture;
        _darkened = CreateDarkened(capture.Image);

        Text = "CapturaTool: selección";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.None;
        Cursor = Cursors.Cross;
        Bounds = capture.Bounds;
    }

    private static Bitmap CreateDarkened(Bitmap source)
    {
        var result = new Bitmap(source.Width, source.Height);
        using var g = Graphics.FromImage(result);
        var all = new Rectangle(Point.Empty, source.Size);
        g.DrawImage(source, all, all, GraphicsUnit.Pixel);
        using var shade = new SolidBrush(Color.FromArgb(120, Color.Black));
        g.FillRectangle(shade, all);
        return result;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Volver a fijar posición y tamaño exactos por si WinForms los ajustó al mostrarla
        Bounds = _capture.Bounds;
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        // Con monitores de escalado distinto, Windows sugiere cambiar el tamaño de la ventana.
        // Aquí no queremos eso: la ventana debe medir exactamente lo que mide el monitor.
        e.Cancel = true;
        base.OnDpiChanged(e);
        Bounds = _capture.Bounds;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Nada: OnPaint cubre toda la ventana (evita parpadeo)
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        // Solo se redibuja la parte que cambió (e.ClipRectangle), a escala 1:1
        g.DrawImage(_darkened, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);

        if (!_selection.IsEmpty)
        {
            // La zona elegida se ve sin oscurecer (R7)
            g.DrawImage(_capture.Image, _selection, _selection, GraphicsUnit.Pixel);
            using var border = new Pen(Color.White, 1);
            g.DrawRectangle(border, _selection.X, _selection.Y, _selection.Width - 1, _selection.Height - 1);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _dragging = true;
            _start = e.Location;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging)
        {
            return;
        }

        Rectangle old = _selection;
        _selection = SelectionGeometry.FromPoints(_start, e.Location);
        InvalidateChange(old, _selection);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || !_dragging)
        {
            return;
        }

        _dragging = false;
        Rectangle dragged = SelectionGeometry.FromPoints(_start, e.Location);
        Rectangle area = SelectionGeometry.Resolve(dragged, _capture.Image.Size);
        SelectionCompleted?.Invoke(this, area);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        // Cerrar la ventana a mano (por ejemplo Alt + F4) también cancela
        if (e.CloseReason == CloseReason.UserClosing)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Pide redibujar solo la zona que ocupaban la selección anterior y la nueva.</summary>
    private void InvalidateChange(Rectangle old, Rectangle current)
    {
        Rectangle changed = old.IsEmpty ? current : Rectangle.Union(old, current);
        changed.Inflate(2, 2); // incluye el borde
        Invalidate(changed);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _darkened.Dispose();
        }
        base.Dispose(disposing);
    }
}
