using System.Drawing;
using CapturaTool.Editor;

namespace CapturaTool.Tests;

public class ViewTransformTests
{
    [Fact]
    public void ImageThatFits_IsShownAtRealSizeAndCentered()
    {
        var view = new ViewTransform(new Size(800, 600), new Size(1000, 700));

        Assert.Equal(1f, view.Scale);
        Assert.Equal(new Rectangle(100, 50, 800, 600), view.ImageBounds);
    }

    [Fact]
    public void WideImage_IsFittedByWidth()
    {
        var view = new ViewTransform(new Size(2000, 500), new Size(1000, 800));

        Assert.Equal(0.5f, view.Scale);
        Assert.Equal(new Rectangle(0, 275, 1000, 250), view.ImageBounds);
    }

    [Fact]
    public void TallImage_IsFittedByHeight()
    {
        var view = new ViewTransform(new Size(500, 2000), new Size(800, 1000));

        Assert.Equal(0.5f, view.Scale);
        Assert.Equal(new Rectangle(275, 0, 250, 1000), view.ImageBounds);
    }

    [Fact]
    public void SmallImage_IsNeverEnlarged()
    {
        var view = new ViewTransform(new Size(50, 30), new Size(1920, 1080));

        Assert.Equal(1f, view.Scale);
        Assert.Equal(new Size(50, 30), view.ImageBounds.Size);
    }

    [Fact]
    public void Image4KInFullHdView_HasHalfScale_AndCentersMatch()
    {
        var view = new ViewTransform(new Size(3840, 2160), new Size(1920, 1080));

        Assert.Equal(0.5f, view.Scale);
        Assert.Equal(new PointF(1920, 1080), view.ScreenToImage(new PointF(960, 540)));
        Assert.Equal(new PointF(960, 540), view.ImageToScreen(new PointF(1920, 1080)));
    }

    [Fact]
    public void ScreenToImage_AtRealSize_OnlySubtractsOffset()
    {
        var view = new ViewTransform(new Size(800, 600), new Size(1000, 700));

        Assert.Equal(new PointF(0, 0), view.ScreenToImage(new PointF(100, 50)));
        Assert.Equal(new PointF(10, 20), view.ScreenToImage(new PointF(110, 70)));
    }

    [Theory]
    [InlineData(800, 600, 1000, 700)]    // tamaño real
    [InlineData(3840, 2160, 1920, 1080)] // mitad
    [InlineData(2560, 1440, 1000, 700)]  // zoom no redondo
    [InlineData(1920, 1080, 1366, 650)]  // proporciones distintas
    public void RoundTrip_ScreenToImageToScreen_GivesSamePoint(int imgW, int imgH, int viewW, int viewH)
    {
        var view = new ViewTransform(new Size(imgW, imgH), new Size(viewW, viewH));
        var screen = new PointF(437.5f, 321.25f);

        PointF back = view.ImageToScreen(view.ScreenToImage(screen));

        Assert.Equal(screen.X, back.X, 0.01f);
        Assert.Equal(screen.Y, back.Y, 0.01f);
    }

    [Theory]
    [InlineData(800, 600, 1001, 701)]
    [InlineData(2560, 1440, 1000, 700)]
    [InlineData(1921, 1081, 1366, 650)]
    public void ImageBounds_IsInsideView(int imgW, int imgH, int viewW, int viewH)
    {
        var view = new ViewTransform(new Size(imgW, imgH), new Size(viewW, viewH));

        Assert.True(new Rectangle(0, 0, viewW, viewH).Contains(view.ImageBounds));
    }

    [Fact]
    public void EmptyView_UsesRealSize_WithoutDividingByZero()
    {
        var view = new ViewTransform(new Size(800, 600), new Size(0, 0));

        Assert.Equal(1f, view.Scale);
        Assert.False(float.IsInfinity(view.ScreenToImage(new PointF(10, 10)).X));
    }

    [Theory]
    [InlineData(800, 600, 1000, 700)]
    [InlineData(3840, 2160, 1920, 1080)]
    [InlineData(2560, 1440, 1000, 700)]
    public void Matrix_ConvertsLikeImageToScreen(int imgW, int imgH, int viewW, int viewH)
    {
        var view = new ViewTransform(new Size(imgW, imgH), new Size(viewW, viewH));
        var image = new PointF(123.5f, 456.25f);
        PointF[] points = { image };

        using var matrix = view.CreateImageToScreenMatrix();
        matrix.TransformPoints(points);

        PointF expected = view.ImageToScreen(image);
        Assert.Equal(expected.X, points[0].X, 0.01f);
        Assert.Equal(expected.Y, points[0].Y, 0.01f);
    }

    [Fact]
    public void ClampToImage_PointInside_StaysTheSame()
    {
        var view = new ViewTransform(new Size(800, 600), new Size(1000, 700));

        Assert.Equal(new PointF(100, 200), view.ClampToImage(new PointF(100, 200)));
    }

    [Fact]
    public void ClampToImage_PointOutside_MovesToNearestEdge()
    {
        var view = new ViewTransform(new Size(800, 600), new Size(1000, 700));

        Assert.Equal(new PointF(0, 0), view.ClampToImage(new PointF(-30, -5)));
        Assert.Equal(new PointF(800, 600), view.ClampToImage(new PointF(900, 650)));
        Assert.Equal(new PointF(800, 300), view.ClampToImage(new PointF(1200, 300)));
    }
}
