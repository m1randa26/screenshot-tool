namespace CapturaTool.Capture;

/// <summary>
/// La imagen congelada de un monitor y su posición en el escritorio.
/// Es dueña de su Bitmap: lo libera en Dispose.
/// </summary>
public sealed class MonitorCapture : IDisposable
{
    /// <summary>Posición y tamaño del monitor en el escritorio, en píxeles físicos.</summary>
    public Rectangle Bounds { get; }

    /// <summary>Imagen del monitor completo, en resolución nativa.</summary>
    public Bitmap Image { get; }

    public MonitorCapture(Rectangle bounds, Bitmap image)
    {
        Bounds = bounds;
        Image = image;
    }

    /// <summary>
    /// Copia una zona de la imagen a un Bitmap nuevo e independiente.
    /// Quien lo recibe es su dueño y debe liberarlo.
    /// </summary>
    public Bitmap Crop(Rectangle area)
    {
        var result = new Bitmap(area.Width, area.Height);
        using var g = Graphics.FromImage(result);
        g.DrawImage(Image, new Rectangle(Point.Empty, area.Size), area, GraphicsUnit.Pixel);
        return result;
    }

    public void Dispose()
    {
        Image.Dispose();
    }
}
