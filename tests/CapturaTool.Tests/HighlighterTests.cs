using System.Drawing;
using CapturaTool.Editor;
using CapturaTool.Editor.Annotations;

namespace CapturaTool.Tests;

public class HighlighterTests
{
    private static readonly Color Red = Palette.Default; // (230, 30, 30)

    private static Bitmap DrawOnWhite(Annotation annotation, Action<Graphics>? background = null)
    {
        var image = new Bitmap(100, 100);
        using var g = Graphics.FromImage(image);
        g.Clear(Color.White);
        background?.Invoke(g);
        annotation.Draw(g);
        return image;
    }

    private static void AssertNear(Color expected, Color actual, int tolerance)
    {
        Assert.InRange(actual.R, expected.R - tolerance, expected.R + tolerance);
        Assert.InRange(actual.G, expected.G - tolerance, expected.G + tolerance);
        Assert.InRange(actual.B, expected.B - tolerance, expected.B + tolerance);
    }

    /// <summary>Color esperado de la mezcla: fondo × (1 − opacidad) + trazo × opacidad.</summary>
    private static Color Blend(Color background, Color stroke)
    {
        float a = HighlighterAnnotation.Opacity;
        return Color.FromArgb(
            (int)Math.Round(background.R * (1 - a) + stroke.R * a),
            (int)Math.Round(background.G * (1 - a) + stroke.G * a),
            (int)Math.Round(background.B * (1 - a) + stroke.B * a));
    }

    [Fact]
    public void OnWhite_StrokeIsSemiTransparentBlend()
    {
        var stroke = new HighlighterAnnotation(new[] { new PointF(10, 50), new PointF(90, 50) }, Red);

        using Bitmap image = DrawOnWhite(stroke);

        AssertNear(Blend(Color.White, Red), image.GetPixel(50, 50), 3);
    }

    [Fact]
    public void StrokeCrossingItself_HasSameToneAtCrossing()
    {
        // Horizontal y luego vertical: se cruza en (50, 50)
        var points = new[] { new PointF(10, 50), new PointF(90, 50), new PointF(50, 10), new PointF(50, 90) };
        var stroke = new HighlighterAnnotation(points, Red);

        using Bitmap image = DrawOnWhite(stroke);

        Color crossing = image.GetPixel(50, 50);
        Color single = image.GetPixel(20, 50); // solo pasa una vez
        AssertNear(single, crossing, 2);
    }

    [Fact]
    public void TextUnderneath_IsStillReadable()
    {
        var stroke = new HighlighterAnnotation(new[] { new PointF(10, 50), new PointF(90, 50) }, Red);

        // Un cuadro negro hace de "texto" bajo el trazo
        using Bitmap image = DrawOnWhite(stroke, g => g.FillRectangle(Brushes.Black, 40, 45, 20, 10));

        Color overText = image.GetPixel(50, 50);
        Color overBackground = image.GetPixel(20, 50);
        AssertNear(Blend(Color.Black, Red), overText, 3);
        Assert.True(overBackground.R - overText.R > 100, "el texto resaltado debe seguir viéndose mucho más oscuro");
    }

    [Fact]
    public void OutsideTheStroke_ImageIsUntouched()
    {
        var stroke = new HighlighterAnnotation(new[] { new PointF(10, 50), new PointF(90, 50) }, Red);

        using Bitmap image = DrawOnWhite(stroke);

        AssertNear(Color.White, image.GetPixel(50, 20), 0);
        AssertNear(Color.White, image.GetPixel(50, 80), 0);
    }

    [Fact]
    public void Points_AreACopy_AndCannotBeChangedFromOutside()
    {
        var original = new List<PointF> { new(1, 1), new(2, 2) };
        var stroke = new HighlighterAnnotation(original, Red);

        original.Add(new PointF(3, 3));

        Assert.Equal(2, stroke.Points.Count);
        Assert.False(stroke.Points is IList<PointF> list && !list.IsReadOnly);
    }

    [Fact]
    public void BoundsOf_ContainsAllPoints()
    {
        var points = new[] { new PointF(30, 40), new PointF(10, 80), new PointF(60, 20) };

        Assert.Equal(RectangleF.FromLTRB(10, 20, 60, 80), ShapeGeometry.BoundsOf(points));
    }

    [Fact]
    public void BoundsOf_NoPoints_IsEmpty()
    {
        Assert.Equal(RectangleF.Empty, ShapeGeometry.BoundsOf(Array.Empty<PointF>()));
    }

    /// <summary>Dibuja como en el editor con zoom 0,5: g trae la conversión imagen → pantalla.</summary>
    private static Bitmap DrawOnWhiteAtHalfZoom(Annotation annotation)
    {
        var image = new Bitmap(100, 100);
        using var g = Graphics.FromImage(image);
        g.Clear(Color.White);
        g.ScaleTransform(0.5f, 0.5f);
        annotation.Draw(g);
        return image;
    }

    [Fact]
    public void WithZoom_StrokeLandsInScaledPlace_WithScaledWidth()
    {
        // En la imagen: de (20,100) a (180,100), 20 px de grosor → en pantalla: y = 50, 10 px de grosor
        var stroke = new HighlighterAnnotation(new[] { new PointF(20, 100), new PointF(180, 100) }, Red);

        using Bitmap image = DrawOnWhiteAtHalfZoom(stroke);

        AssertNear(Blend(Color.White, Red), image.GetPixel(50, 50), 3);
        AssertNear(Blend(Color.White, Red), image.GetPixel(50, 53), 3);  // dentro del grosor (45–55)
        AssertNear(Color.White, image.GetPixel(50, 58), 0);              // fuera del grosor
    }

    [Fact]
    public void WithZoom_CrossingStillHasSameTone()
    {
        var points = new[] { new PointF(20, 100), new PointF(180, 100), new PointF(100, 20), new PointF(100, 180) };
        var stroke = new HighlighterAnnotation(points, Red);

        using Bitmap image = DrawOnWhiteAtHalfZoom(stroke);

        AssertNear(image.GetPixel(20, 50), image.GetPixel(50, 50), 2);
    }

    [Fact]
    public void Draw_RestoresTheCallersTransform()
    {
        var stroke = new HighlighterAnnotation(new[] { new PointF(20, 100), new PointF(180, 100) }, Red);
        using var image = new Bitmap(100, 100);
        using var g = Graphics.FromImage(image);
        g.ScaleTransform(0.5f, 0.5f);

        stroke.Draw(g);

        using var after = g.Transform;
        Assert.Equal(0.5f, after.Elements[0]);
    }
}
