using System.Drawing;
using CapturaTool.Editor;
using CapturaTool.Editor.Annotations;

namespace CapturaTool.Tests;

public class ArrowTests
{
    private static readonly Color Red = Palette.Default;

    private static float Distance(PointF a, PointF b) =>
        MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    private static void AssertNear(PointF expected, PointF actual)
    {
        Assert.Equal(expected.X, actual.X, 0.01f);
        Assert.Equal(expected.Y, actual.Y, 0.01f);
    }

    // --- Geometría ---

    [Theory]
    [InlineData(10, 50, 90, 50)]  // derecha
    [InlineData(90, 50, 10, 50)]  // izquierda
    [InlineData(50, 90, 50, 10)]  // arriba
    [InlineData(50, 10, 50, 90)]  // abajo
    [InlineData(10, 10, 80, 60)]  // diagonal
    public void Tip_IsExactlyTheEndPoint(float x1, float y1, float x2, float y2)
    {
        var end = new PointF(x2, y2);

        var arrow = ArrowGeometry.Calculate(new PointF(x1, y1), end);

        Assert.Equal(end, arrow.Tip);
    }

    [Fact]
    public void HeadCorners_AreSymmetric_AndAtHeadLengthFromTip()
    {
        var arrow = ArrowGeometry.Calculate(new PointF(10, 10), new PointF(80, 60));

        // El centro de la base está a HeadLength de la punta
        var baseCenter = new PointF((arrow.Left.X + arrow.Right.X) / 2, (arrow.Left.Y + arrow.Right.Y) / 2);
        Assert.Equal(ArrowGeometry.HeadLength, Distance(baseCenter, arrow.Tip), 0.01f);

        // Las dos esquinas están a la misma distancia de la punta, y separadas HeadWidth
        Assert.Equal(Distance(arrow.Left, arrow.Tip), Distance(arrow.Right, arrow.Tip), 0.01f);
        Assert.Equal(ArrowGeometry.HeadWidth, Distance(arrow.Left, arrow.Right), 0.01f);
    }

    [Fact]
    public void ArrowPointingUp_HasHeadBelowTip()
    {
        var arrow = ArrowGeometry.Calculate(new PointF(50, 90), new PointF(50, 10));

        AssertNear(new PointF(50 - 8, 10 + 18), arrow.Right);
        AssertNear(new PointF(50 + 8, 10 + 18), arrow.Left);
    }

    [Fact]
    public void ShortArrow_HeadShrinksToArrowLength()
    {
        // Flecha de 6 px: la punta no puede medir 18
        var arrow = ArrowGeometry.Calculate(new PointF(10, 50), new PointF(16, 50));

        var baseCenter = new PointF((arrow.Left.X + arrow.Right.X) / 2, (arrow.Left.Y + arrow.Right.Y) / 2);
        AssertNear(new PointF(10, 50), baseCenter);
        Assert.Equal(ArrowGeometry.HeadWidth * 6 / 18, Distance(arrow.Left, arrow.Right), 0.01f);
    }

    [Fact]
    public void LineEnd_IsInsideTheHead_BetweenBaseAndTip()
    {
        var arrow = ArrowGeometry.Calculate(new PointF(10, 50), new PointF(90, 50));

        // Base en x = 72, punta en x = 90
        Assert.InRange(arrow.LineEnd.X, 72f, 90f);
        Assert.Equal(50f, arrow.LineEnd.Y, 0.01f);
    }

    [Fact]
    public void ZeroLengthArrow_DoesNotProduceInvalidNumbers()
    {
        var point = new PointF(30, 30);

        var arrow = ArrowGeometry.Calculate(point, point);

        Assert.Equal(point, arrow.Left);
        Assert.False(float.IsNaN(arrow.LineEnd.X));
    }

    // --- Dibujo ---

    private static Bitmap DrawOnWhite(Annotation annotation)
    {
        var image = new Bitmap(100, 100);
        using var g = Graphics.FromImage(image);
        g.Clear(Color.White);
        annotation.Draw(g);
        return image;
    }

    private static bool IsNear(Color actual, Color expected) =>
        Math.Abs(actual.R - expected.R) <= 10
        && Math.Abs(actual.G - expected.G) <= 10
        && Math.Abs(actual.B - expected.B) <= 10;

    [Fact]
    public void Arrow_KeepsPointsAndColor()
    {
        var annotation = new ArrowAnnotation(new PointF(1, 2), new PointF(3, 4), Red);

        Assert.Equal(new PointF(1, 2), annotation.Start);
        Assert.Equal(new PointF(3, 4), annotation.End);
        Assert.Equal(Red, annotation.Color);
    }

    [Fact]
    public void ArrowToTheRight_HasColorUpToTip_AndNothingBeyond()
    {
        using Bitmap image = DrawOnWhite(new ArrowAnnotation(new PointF(10, 50), new PointF(90, 50), Red));

        Assert.True(IsNear(image.GetPixel(40, 50), Red));          // línea
        Assert.True(IsNear(image.GetPixel(85, 50), Red));          // cerca de la punta
        Assert.True(IsNear(image.GetPixel(94, 50), Color.White));  // más allá de la punta
    }

    [Fact]
    public void ArrowHead_IsWiderThanTheLine()
    {
        using Bitmap image = DrawOnWhite(new ArrowAnnotation(new PointF(10, 50), new PointF(90, 50), Red));

        Assert.True(IsNear(image.GetPixel(75, 55), Red));          // la punta llega a y = 55
        Assert.True(IsNear(image.GetPixel(40, 55), Color.White));  // la línea no
    }

    [Fact]
    public void ArrowToTheLeft_HasHeadOnTheLeft()
    {
        using Bitmap image = DrawOnWhite(new ArrowAnnotation(new PointF(90, 50), new PointF(10, 50), Red));

        Assert.True(IsNear(image.GetPixel(25, 55), Red));          // punta ancha a la izquierda
        Assert.True(IsNear(image.GetPixel(60, 55), Color.White));  // a la derecha solo hay línea
        Assert.True(IsNear(image.GetPixel(6, 50), Color.White));   // más allá de la punta
    }
}
