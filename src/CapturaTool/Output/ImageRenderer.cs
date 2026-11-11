using System.Drawing.Drawing2D;
using CapturaTool.Editor;
using CapturaTool.Editor.Annotations;

namespace CapturaTool.Output;

/// <summary>
/// Genera la imagen final: la captura original con sus anotaciones encima,
/// a resolución original (R21, R23). La captura original no se modifica.
/// </summary>
public static class ImageRenderer
{
    /// <summary>
    /// Devuelve una imagen nueva. Quien la recibe es su dueño y debe liberarla.
    /// </summary>
    public static Bitmap Render(AnnotationDocument document)
    {
        Bitmap baseImage = document.BaseImage;
        var result = new Bitmap(baseImage.Width, baseImage.Height);

        using Graphics g = Graphics.FromImage(result);

        // 1. La captura, píxel por píxel (sin suavizar ni escalar)
        var all = new Rectangle(Point.Empty, baseImage.Size);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(baseImage, all, all, GraphicsUnit.Pixel);

        // 2. Las anotaciones en el orden en que se crearon (la más reciente queda encima),
        //    con el mismo código que en pantalla pero sin zoom: coordenadas de imagen tal cual
        foreach (Annotation annotation in document.Annotations)
        {
            annotation.Draw(g);
        }

        return result;
    }
}
