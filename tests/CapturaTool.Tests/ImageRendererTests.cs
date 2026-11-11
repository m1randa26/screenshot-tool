using System.Drawing;
using CapturaTool.Editor;
using CapturaTool.Editor.Annotations;
using CapturaTool.Output;

namespace CapturaTool.Tests;

public class ImageRendererTests
{
    private static readonly Color Red = Palette.Default;
    private static readonly Color Blue = Palette.Colors[4].Color;

    /// <summary>Captura de prueba con un patrón (para notar cualquier cambio de un píxel).</summary>
    private static Bitmap PatternImage(int width, int height)
    {
        var image = new Bitmap(width, height);
        for (int y = 0; y < height; y += 1)
        {
            for (int x = 0; x < width; x += 1)
            {
                image.SetPixel(x, y, Color.FromArgb((x * 7) % 256, (y * 5) % 256, ((x + y) * 3) % 256));
            }
        }
        return image;
    }

    private static bool IsNear(Color actual, Color expected, int tolerance = 10) =>
        Math.Abs(actual.R - expected.R) <= tolerance
        && Math.Abs(actual.G - expected.G) <= tolerance
        && Math.Abs(actual.B - expected.B) <= tolerance;

    [Fact]
    public void Result_HasSameSizeAsCapture()
    {
        using var document = new AnnotationDocument(new Bitmap(320, 180));

        using Bitmap result = ImageRenderer.Render(document);

        Assert.Equal(new Size(320, 180), result.Size);
    }

    [Fact]
    public void WithoutAnnotations_ResultIsIdenticalToCapture()
    {
        using var document = new AnnotationDocument(PatternImage(120, 80));

        using Bitmap result = ImageRenderer.Render(document);

        for (int y = 0; y < 80; y++)
        {
            for (int x = 0; x < 120; x++)
            {
                Assert.Equal(document.BaseImage.GetPixel(x, y).ToArgb(), result.GetPixel(x, y).ToArgb());
            }
        }
    }

    [Fact]
    public void Annotations_AppearAtTheirImageCoordinates()
    {
        var base2000 = new Bitmap(2000, 1000);
        using (var g = Graphics.FromImage(base2000))
        {
            g.Clear(Color.White);
        }
        using var document = new AnnotationDocument(base2000);
        document.Add(new RectangleAnnotation(new RectangleF(1000, 500, 200, 100), Red));
        document.Add(new ArrowAnnotation(new PointF(100, 100), new PointF(300, 100), Blue));
        document.Add(new TextAnnotation(new PointF(1500, 800), "bug", Red));

        using Bitmap result = ImageRenderer.Render(document);

        Assert.True(IsNear(result.GetPixel(1000, 550), Red));          // borde izquierdo del rectángulo
        Assert.True(IsNear(result.GetPixel(1100, 550), Color.White));  // interior sin relleno
        Assert.True(IsNear(result.GetPixel(295, 100), Blue));          // punta de la flecha
        Assert.True(IsNear(result.GetPixel(305, 100), Color.White));   // más allá de la punta

        // El texto: hay tinta roja en la zona del texto y nada justo antes de su posición
        bool inkInText = false;
        for (int x = 1500; x < 1560 && !inkInText; x++)
        {
            for (int y = 800; y < 830 && !inkInText; y++)
            {
                inkInText = IsNear(result.GetPixel(x, y), Red, 40);
            }
        }
        Assert.True(inkInText);
        Assert.True(IsNear(result.GetPixel(1495, 815), Color.White));
    }

    [Fact]
    public void OverlappingAnnotations_NewestIsOnTop()
    {
        using var document = new AnnotationDocument(new Bitmap(100, 100));
        document.Add(new RectangleAnnotation(new RectangleF(20, 20, 60, 60), Red));
        document.Add(new RectangleAnnotation(new RectangleF(20, 20, 60, 60), Blue)); // mismo lugar, después

        using Bitmap result = ImageRenderer.Render(document);

        Assert.True(IsNear(result.GetPixel(20, 50), Blue));
    }

    [Fact]
    public void Highlighter_IsBlendedWithTheCapture()
    {
        var white = new Bitmap(100, 100);
        using (var g = Graphics.FromImage(white))
        {
            g.Clear(Color.White);
        }
        using var document = new AnnotationDocument(white);
        document.Add(new HighlighterAnnotation(new[] { new PointF(10, 50), new PointF(90, 50) }, Red));

        using Bitmap result = ImageRenderer.Render(document);

        Color pixel = result.GetPixel(50, 50);
        Assert.False(IsNear(pixel, Red, 5));          // no es opaco
        Assert.False(IsNear(pixel, Color.White, 5));  // pero sí se nota
        Assert.Equal(255, pixel.A);                   // la imagen final es opaca
    }

    [Fact]
    public void OriginalCapture_IsNotModified()
    {
        using var document = new AnnotationDocument(PatternImage(100, 100));
        int before = document.BaseImage.GetPixel(20, 50).ToArgb();
        document.Add(new RectangleAnnotation(new RectangleF(20, 20, 60, 60), Red));

        using Bitmap result = ImageRenderer.Render(document);

        Assert.Equal(before, document.BaseImage.GetPixel(20, 50).ToArgb());
        Assert.NotEqual(before, result.GetPixel(20, 50).ToArgb());
    }

    [Fact]
    public void Result_StaysValidAfterDocumentIsReleased()
    {
        var document = new AnnotationDocument(new Bitmap(50, 40));
        Bitmap result = ImageRenderer.Render(document);

        document.Dispose();

        Assert.Equal(new Size(50, 40), result.Size);
        result.Dispose();
    }
}
