using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using CapturaTool.Editor.Annotations;

namespace CapturaTool.Editor;

/// <summary>
/// Lienzo del editor: muestra la captura ajustada al espacio disponible (R11)
/// y permite dibujar figuras arrastrando el mouse (R12, R15).
///
/// Para que dibujar no se trabe, usa una caché: una imagen del tamaño del lienzo con
/// la captura ya reducida y las anotaciones terminadas. Se regenera solo al agregar,
/// deshacer o rehacer, y al cambiar el tamaño. Al mover el mouse se copia la caché tal
/// cual y encima se dibuja la figura en curso.
/// El documento es de EditorForm; el lienzo solo lo usa.
/// </summary>
public class EditorCanvas : Control
{
    private readonly AnnotationDocument _document;
    private Bitmap? _cache; // dueño: este lienzo

    // Figura que se está dibujando ahora (coordenadas de imagen)
    private bool _drawing;
    private PointF _start;
    private PointF _current;
    private readonly List<PointF> _stroke = new(); // puntos del resaltador en curso

    // Texto que se está escribiendo (R14): null cuando no se está escribiendo
    private TextInput? _typing;
    private PointF _textPosition; // coordenadas de imagen

    private Tool _currentTool = Tool.Rectangle;
    private Color _currentColor = Palette.Default;

    public EditorCanvas(AnnotationDocument document)
    {
        _document = document;
        View = new ViewTransform(document.BaseImage.Size, ClientSize);

        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, true); // puede recibir el teclado (para escribir texto)
        BackColor = Color.FromArgb(45, 45, 48); // gris oscuro: marca dónde termina la captura
        Cursor = Cursors.Cross;
    }

    /// <summary>Cómo se muestra la imagen ahora mismo. Cambia al cambiar el tamaño del lienzo.</summary>
    public ViewTransform View { get; private set; }

    // DesignerSerializationVisibility.Hidden: WinForms lo exige en propiedades públicas de un
    // control; indica que el diseñador visual no debe guardarlas (aquí no se usa el diseñador).
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Tool CurrentTool
    {
        get => _currentTool;
        set
        {
            _currentTool = value;
            Cursor = value == Tool.Text ? Cursors.IBeam : Cursors.Cross;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color CurrentColor
    {
        get => _currentColor;
        set
        {
            _currentColor = value;
            Invalidate(); // si se está escribiendo, el texto cambia de color al instante
        }
    }

    /// <summary>true mientras se está escribiendo un texto (antes de confirmarlo o cancelarlo).</summary>
    public bool IsTyping => _typing != null;

    /// <summary>true si se está escribiendo un texto que no está vacío (cuenta como anotación al cerrar).</summary>
    public bool HasUnconfirmedText => _typing != null && !_typing.IsEmpty;

    /// <summary>Confirma el texto que se está escribiendo (R14). Un texto vacío no crea nada.</summary>
    public void ConfirmText()
    {
        if (_typing == null)
        {
            return;
        }

        if (!_typing.IsEmpty)
        {
            _document.Add(new TextAnnotation(_textPosition, _typing.Text, CurrentColor));
            RebuildCache();
        }
        _typing = null;
        Invalidate();
    }

    /// <summary>Descarta solo el texto que se está escribiendo (Esc), no la captura.</summary>
    public void CancelText()
    {
        _typing = null;
        Invalidate();
    }

    /// <summary>Avisar al lienzo de que el documento cambió por fuera (deshacer, rehacer).</summary>
    public void DocumentChanged()
    {
        RebuildCache();
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        View = new ViewTransform(_document.BaseImage.Size, ClientSize);
        RebuildCache();
        Invalidate();
        base.OnResize(e);
    }

    private void RebuildCache()
    {
        _cache?.Dispose();
        _cache = null;
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return; // ventana minimizada
        }

        _cache = new Bitmap(ClientSize.Width, ClientSize.Height);
        using Graphics g = Graphics.FromImage(_cache);
        g.Clear(BackColor);
        DrawBaseImage(g);

        using Matrix toScreen = View.CreateImageToScreenMatrix();
        g.Transform = toScreen;
        foreach (Annotation annotation in _document.Annotations)
        {
            annotation.Draw(g);
        }
    }

    private void DrawBaseImage(Graphics g)
    {
        Bitmap image = _document.BaseImage;
        var source = new Rectangle(Point.Empty, image.Size);

        if (View.Scale < 1f)
        {
            // Reducida: bilineal de alta calidad (la bicúbica tardaba demasiado: NF2).
            // TileFlipXY evita un borde semitransparente en las orillas.
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using var attributes = new ImageAttributes();
            attributes.SetWrapMode(WrapMode.TileFlipXY);
            g.DrawImage(image, View.ImageBounds, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        }
        else
        {
            // Tamaño real: píxel por píxel, sin suavizar
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(image, View.ImageBounds, source, GraphicsUnit.Pixel);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (_cache == null)
        {
            base.OnPaintBackground(e);
        }
        // Con caché no hace falta: OnPaint cubre todo el lienzo (evita parpadeo)
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_cache == null)
        {
            return;
        }

        Graphics g = e.Graphics;

        // La caché ya está al tamaño del lienzo: se copia tal cual (rápido)
        g.CompositingMode = CompositingMode.SourceCopy;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.DrawImage(_cache, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
        g.CompositingMode = CompositingMode.SourceOver;

        // Encima, la figura que se está dibujando, con la misma conversión que la imagen final
        Annotation? inProgress = _drawing ? CreateShape() : null;
        if (inProgress != null)
        {
            using Matrix toScreen = View.CreateImageToScreenMatrix();
            g.Transform = toScreen;
            inProgress.Draw(g);
        }

        if (_typing != null)
        {
            DrawTypingText(g);
        }
    }

    /// <summary>El texto que se está escribiendo, tal como quedará, con un cursor al final.</summary>
    private void DrawTypingText(Graphics g)
    {
        using Matrix toScreen = View.CreateImageToScreenMatrix();
        g.Transform = toScreen;

        var text = new TextAnnotation(_textPosition, _typing!.Text, CurrentColor);
        text.Draw(g);

        SizeF size = text.Measure(g);
        float x = _textPosition.X + size.Width + 2;
        using var caret = new Pen(CurrentColor, 2);
        g.DrawLine(caret, x, _textPosition.Y, x, _textPosition.Y + size.Height);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        // Un clic en cualquier lugar mientras se escribe confirma el texto (R14)
        if (_typing != null)
        {
            ConfirmText();
            return;
        }

        // Solo se empieza a dibujar dentro de la captura
        if (e.Button != MouseButtons.Left || !View.ImageBounds.Contains(e.Location))
        {
            return;
        }

        if (CurrentTool == Tool.Text)
        {
            // Empezar a escribir donde se hizo clic
            _typing = new TextInput();
            _textPosition = View.ClampToImage(View.ScreenToImage(e.Location));
            Focus();
            Invalidate();
            return;
        }

        _drawing = true;
        _start = View.ClampToImage(View.ScreenToImage(e.Location));
        _current = _start;
        _stroke.Clear();
        _stroke.Add(_start);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_drawing)
        {
            return;
        }

        // Si el mouse sale de la captura, la figura se queda en el borde
        _current = View.ClampToImage(View.ScreenToImage(e.Location));
        AddStrokePoint(_current);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || !_drawing)
        {
            return;
        }

        _drawing = false;
        _current = View.ClampToImage(View.ScreenToImage(e.Location));
        AddStrokePoint(_current);

        Annotation? shape = CreateShape();
        if (shape != null)
        {
            _document.Add(shape);
            RebuildCache();
        }
        Invalidate();
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        // Si se pierde el mouse a mitad de un arrastre (por ejemplo, Alt + Tab), se descarta la figura
        if (_drawing && !Capture)
        {
            _drawing = false;
            Invalidate();
        }
    }

    // Mientras se escribe, Enter, Esc y Retroceso son para el texto (no para la ventana)
    protected override bool IsInputKey(Keys keyData)
    {
        if (_typing != null && (keyData == Keys.Enter || keyData == Keys.Escape || keyData == Keys.Back))
        {
            return true;
        }
        return base.IsInputKey(keyData);
    }

    protected override bool IsInputChar(char charCode)
    {
        return _typing != null || base.IsInputChar(charCode);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_typing == null)
        {
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Enter:
                ConfirmText();
                e.SuppressKeyPress = true;
                break;
            case Keys.Escape:
                CancelText();
                e.SuppressKeyPress = true;
                break;
            case Keys.Back:
                _typing.Backspace();
                Invalidate();
                e.SuppressKeyPress = true;
                break;
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (_typing == null)
        {
            return;
        }

        // Las letras (con acentos ya compuestos por Windows) se agregan al texto
        _typing.Type(e.KeyChar);
        Invalidate();
        e.Handled = true;
    }

    /// <summary>Agrega un punto al trazo del resaltador, si el mouse se movió.</summary>
    private void AddStrokePoint(PointF point)
    {
        if (CurrentTool == Tool.Highlighter && point != _stroke[^1])
        {
            _stroke.Add(point);
        }
    }

    /// <summary>La figura entre el punto inicial y el actual, o null si es diminuta (un clic).</summary>
    private Annotation? CreateShape()
    {
        if (CurrentTool == Tool.Highlighter)
        {
            // El resaltador es de trazo libre: se mide todo el recorrido, no solo inicio y fin
            return ShapeGeometry.IsTooSmall(ShapeGeometry.BoundsOf(_stroke))
                ? null
                : new HighlighterAnnotation(_stroke, CurrentColor);
        }

        RectangleF bounds = ShapeGeometry.FromPoints(_start, _current);
        if (ShapeGeometry.IsTooSmall(bounds))
        {
            return null;
        }

        return CurrentTool switch
        {
            Tool.Ellipse => new EllipseAnnotation(bounds, CurrentColor),
            // La flecha usa los puntos tal cual: la dirección importa (la punta va donde se soltó)
            Tool.Arrow => new ArrowAnnotation(_start, _current, CurrentColor),
            _ => new RectangleAnnotation(bounds, CurrentColor),
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cache?.Dispose();
            _cache = null;
        }
        base.Dispose(disposing);
    }
}
