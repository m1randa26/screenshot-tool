using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CapturaTool.Editor;

/// <summary>
/// Lienzo del editor: dibuja la captura ajustada al espacio disponible (R11).
/// La imagen es de EditorForm; el lienzo solo la usa.
/// </summary>
public class EditorCanvas : Control
{
    private readonly Bitmap _image;

    public EditorCanvas(Bitmap image)
    {
        _image = image;
        View = new ViewTransform(image.Size, ClientSize);

        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(45, 45, 48); // gris oscuro: marca dónde termina la captura
    }

    /// <summary>Cómo se muestra la imagen ahora mismo. Cambia al cambiar el tamaño del lienzo.</summary>
    public ViewTransform View { get; private set; }

    protected override void OnResize(EventArgs e)
    {
        View = new ViewTransform(_image.Size, ClientSize);
        base.OnResize(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        var source = new Rectangle(Point.Empty, _image.Size);

        if (View.Scale < 1f)
        {
            // Reducida: alta calidad. TileFlipXY evita un borde semitransparente en las orillas.
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using var attributes = new ImageAttributes();
            attributes.SetWrapMode(WrapMode.TileFlipXY);
            Rectangle dest = View.ImageBounds;
            g.DrawImage(_image, dest, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
        }
        else
        {
            // Tamaño real: píxel por píxel, sin suavizar
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(_image, View.ImageBounds, source, GraphicsUnit.Pixel);
        }
    }
}
