using System.Drawing;
using CapturaTool.Editor;
using CapturaTool.Editor.Annotations;

namespace CapturaTool.Tests;

public class ShapeAnnotationTests
{
    private static readonly Color Red = Palette.Default;

    /// <summary>Imagen blanca de prueba con una anotación dibujada encima.</summary>
    private static Bitmap DrawOnWhite(Annotation annotation)
    {
        var image = new Bitmap(100, 100);
        using var g = Graphics.FromImage(image);
        g.Clear(Color.White);
        annotation.Draw(g);
        return image;
    }

    private static bool IsWhite(Color c) => c.R == 255 && c.G == 255 && c.B == 255;

    private static bool IsColor(Color actual, Color expected) => actual.ToArgb() == expected.ToArgb();

    [Fact]
    public void Rectangle_KeepsBoundsAndColor()
    {
        var bounds = new RectangleF(10, 20, 30, 40);

        var annotation = new RectangleAnnotation(bounds, Red);

        Assert.Equal(bounds, annotation.Bounds);
        Assert.Equal(Red, annotation.Color);
    }

    [Fact]
    public void Rectangle_DrawsOutlineInItsColor_WithoutFill()
    {
        using Bitmap image = DrawOnWhite(new RectangleAnnotation(new RectangleF(20, 20, 60, 40), Red));

        Assert.True(IsColor(image.GetPixel(20, 40), Red));  // borde izquierdo
        Assert.True(IsColor(image.GetPixel(50, 20), Red));  // borde superior
        Assert.True(IsWhite(image.GetPixel(50, 40)));       // interior sin relleno
        Assert.True(IsWhite(image.GetPixel(5, 5)));         // fuera
    }

    [Fact]
    public void Ellipse_DrawsOutlineInItsColor_WithoutFill()
    {
        using Bitmap image = DrawOnWhite(new EllipseAnnotation(new RectangleF(10, 10, 80, 60), Red));

        Assert.True(IsColor(image.GetPixel(10, 40), Red));  // extremo izquierdo
        Assert.True(IsColor(image.GetPixel(50, 10), Red));  // extremo superior
        Assert.True(IsWhite(image.GetPixel(50, 40)));       // centro sin relleno
        Assert.True(IsWhite(image.GetPixel(12, 12)));       // esquina del rectángulo, fuera del óvalo
    }

    [Fact]
    public void FromPoints_AnyDirection_GivesSameRectangle()
    {
        var expected = RectangleF.FromLTRB(10, 20, 50, 80);

        Assert.Equal(expected, ShapeGeometry.FromPoints(new PointF(10, 20), new PointF(50, 80)));
        Assert.Equal(expected, ShapeGeometry.FromPoints(new PointF(50, 80), new PointF(10, 20)));
        Assert.Equal(expected, ShapeGeometry.FromPoints(new PointF(10, 80), new PointF(50, 20)));
    }

    [Fact]
    public void IsTooSmall_ClickWithoutDrag_IsTooSmall()
    {
        Assert.True(ShapeGeometry.IsTooSmall(new RectangleF(10, 10, 0, 0)));
        Assert.True(ShapeGeometry.IsTooSmall(new RectangleF(10, 10, 2, 2)));
    }

    [Fact]
    public void IsTooSmall_ThinButLongShape_IsKept()
    {
        Assert.False(ShapeGeometry.IsTooSmall(new RectangleF(10, 10, 100, 1)));
        Assert.False(ShapeGeometry.IsTooSmall(new RectangleF(10, 10, 1, 100)));
    }

    [Fact]
    public void Palette_HasEightColors_AndStartsWithRed()
    {
        Assert.Equal(8, Palette.Colors.Length);
        Assert.Equal("Rojo", Palette.Colors[0].Name);
        Assert.Equal(Palette.Colors[0].Color, Palette.Default);
        Assert.Equal(8, Palette.Colors.Select(c => c.Color.ToArgb()).Distinct().Count());
    }
}
