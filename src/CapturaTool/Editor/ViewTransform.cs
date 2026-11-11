namespace CapturaTool.Editor;

/// <summary>
/// Convierte coordenadas entre la pantalla (el lienzo, donde está el mouse) y la imagen
/// (donde se guardan las anotaciones), según cómo se muestra la captura (R11).
/// Es la única pieza que hace esta conversión. No depende de ventanas.
/// Al cambiar el tamaño del lienzo se crea uno nuevo.
/// </summary>
public class ViewTransform
{
    public ViewTransform(Size imageSize, Size viewSize)
    {
        // Lienzo sin tamaño (por ejemplo, ventana minimizada): se usa tamaño real
        if (viewSize.Width <= 0 || viewSize.Height <= 0)
        {
            Scale = 1f;
        }
        else
        {
            float scaleX = (float)viewSize.Width / imageSize.Width;
            float scaleY = (float)viewSize.Height / imageSize.Height;
            // Tamaño real si cabe; si no, lo necesario para que entre completa. Nunca agranda.
            Scale = Math.Min(1f, Math.Min(scaleX, scaleY));
        }

        var displaySize = new Size(
            (int)Math.Round(imageSize.Width * Scale),
            (int)Math.Round(imageSize.Height * Scale));

        // Centrada, en píxeles enteros para que a tamaño real se vea nítida
        var offset = new Point(
            (viewSize.Width - displaySize.Width) / 2,
            (viewSize.Height - displaySize.Height) / 2);

        ImageBounds = new Rectangle(offset, displaySize);
    }

    /// <summary>Zoom con que se muestra la imagen: 1 = tamaño real, 0.5 = mitad.</summary>
    public float Scale { get; }

    /// <summary>Dónde se dibuja la imagen dentro del lienzo, en píxeles de pantalla.</summary>
    public Rectangle ImageBounds { get; }

    public PointF ScreenToImage(PointF screen)
    {
        return new PointF(
            (screen.X - ImageBounds.X) / Scale,
            (screen.Y - ImageBounds.Y) / Scale);
    }

    public PointF ImageToScreen(PointF image)
    {
        return new PointF(
            image.X * Scale + ImageBounds.X,
            image.Y * Scale + ImageBounds.Y);
    }
}
