using System.Drawing;
using CapturaTool.Editor;
using CapturaTool.Editor.Annotations;

namespace CapturaTool.Tests;

public class TextTests
{
    private static readonly Color Red = Palette.Default;

    // --- TextInput ---

    [Fact]
    public void TextInput_StartsEmpty()
    {
        var input = new TextInput();

        Assert.True(input.IsEmpty);
        Assert.Equal("", input.Text);
    }

    [Fact]
    public void TextInput_TypingAddsLettersInOrder()
    {
        var input = new TextInput();

        input.Type('b');
        input.Type('u');
        input.Type('g');

        Assert.Equal("bug", input.Text);
        Assert.False(input.IsEmpty);
    }

    [Fact]
    public void TextInput_BackspaceRemovesLastLetter()
    {
        var input = new TextInput();
        input.Type('o');
        input.Type('k');

        input.Backspace();

        Assert.Equal("o", input.Text);
    }

    [Fact]
    public void TextInput_BackspaceOnEmpty_DoesNothing()
    {
        var input = new TextInput();

        input.Backspace();

        Assert.True(input.IsEmpty);
    }

    [Fact]
    public void TextInput_ControlKeysAreIgnored()
    {
        var input = new TextInput();

        input.Type('\r');   // Enter
        input.Type('\t');   // Tab
        input.Type('\x1b'); // Esc
        input.Type('\b');   // Retroceso como carácter

        Assert.True(input.IsEmpty);
    }

    [Fact]
    public void TextInput_AcceptsAccentsAndEñe()
    {
        var input = new TextInput();

        foreach (char c in "Aquí falla ñ")
        {
            input.Type(c);
        }

        Assert.Equal("Aquí falla ñ", input.Text);
    }

    [Fact]
    public void TextInput_BackspaceRemovesWholeEmoji()
    {
        var input = new TextInput();
        input.Type('a');
        foreach (char c in "😀")
        {
            input.Type(c);
        }

        input.Backspace();

        Assert.Equal("a", input.Text);
    }

    // --- TextAnnotation ---

    [Fact]
    public void TextAnnotation_KeepsItsData()
    {
        var annotation = new TextAnnotation(new PointF(10, 20), "bug", Red);

        Assert.Equal(new PointF(10, 20), annotation.Position);
        Assert.Equal("bug", annotation.Text);
        Assert.Equal(Red, annotation.Color);
    }

    /// <summary>Rectángulo que ocupan los píxeles que no son blancos.</summary>
    private static Rectangle InkBounds(Bitmap image)
    {
        int left = int.MaxValue, top = int.MaxValue, right = -1, bottom = -1;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color c = image.GetPixel(x, y);
                if (c.R < 250 || c.G < 250 || c.B < 250)
                {
                    left = Math.Min(left, x);
                    top = Math.Min(top, y);
                    right = Math.Max(right, x);
                    bottom = Math.Max(bottom, y);
                }
            }
        }
        return right < 0 ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
    }

    private static Bitmap DrawOnWhite(Annotation annotation, float zoom = 1f)
    {
        var image = new Bitmap(300, 100);
        using var g = Graphics.FromImage(image);
        g.Clear(Color.White);
        g.ScaleTransform(zoom, zoom);
        annotation.Draw(g);
        return image;
    }

    [Fact]
    public void TextAnnotation_DrawsAfterItsPosition_AndNothingBefore()
    {
        using Bitmap image = DrawOnWhite(new TextAnnotation(new PointF(40, 30), "bug", Red));

        Rectangle ink = InkBounds(image);

        Assert.False(ink.IsEmpty);
        Assert.True(ink.Left >= 38, $"el texto empieza en x = {ink.Left}");
        Assert.True(ink.Top >= 28, $"el texto empieza en y = {ink.Top}");
    }

    [Fact]
    public void TextAnnotation_HasFixedSizeInTheImage_SoItShrinksWithZoom()
    {
        var annotation = new TextAnnotation(new PointF(10, 10), "Bug", Red);

        using Bitmap real = DrawOnWhite(annotation, 1f);
        using Bitmap half = DrawOnWhite(annotation, 0.5f);

        int realHeight = InkBounds(real).Height;
        int halfHeight = InkBounds(half).Height;
        Assert.InRange(realHeight, 14, 30); // letras de 24 px
        Assert.InRange((double)halfHeight / realHeight, 0.35, 0.65);
    }

    [Fact]
    public void TextAnnotation_MeasuredWidthGrowsWithText_IncludingTrailingSpaces()
    {
        using var image = new Bitmap(10, 10);
        using var g = Graphics.FromImage(image);

        float empty = new TextAnnotation(PointF.Empty, "", Red).Measure(g).Width;
        float one = new TextAnnotation(PointF.Empty, "a", Red).Measure(g).Width;
        float four = new TextAnnotation(PointF.Empty, "aaaa", Red).Measure(g).Width;
        float withSpace = new TextAnnotation(PointF.Empty, "aaaa ", Red).Measure(g).Width;

        Assert.Equal(0f, empty);
        Assert.True(one > 0);
        Assert.True(four > one);
        Assert.True(withSpace > four);
    }

    [Fact]
    public void TextAnnotation_LineHeightMatchesFontSize()
    {
        using var image = new Bitmap(10, 10);
        using var g = Graphics.FromImage(image);

        float height = new TextAnnotation(PointF.Empty, "", Red).Measure(g).Height;

        Assert.InRange(height, TextAnnotation.FontSize, TextAnnotation.FontSize * 1.6f);
    }
}
