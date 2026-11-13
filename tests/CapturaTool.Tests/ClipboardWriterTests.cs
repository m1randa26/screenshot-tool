using System.Drawing;
using CapturaTool.Output;

namespace CapturaTool.Tests;

// Solo prueba cómo se prepara la imagen; no toca el portapapeles real
public class ClipboardWriterTests
{
    private static Bitmap SampleImage()
    {
        // 3 × 2: arriba a la izquierda rojo, abajo a la izquierda azul
        var image = new Bitmap(3, 2);
        image.SetPixel(0, 0, Color.FromArgb(255, 200, 10, 20));
        image.SetPixel(0, 1, Color.FromArgb(255, 30, 40, 220));
        return image;
    }

    [Fact]
    public void Dib_HeaderDescribesTheImage()
    {
        using Bitmap image = SampleImage();

        byte[] dib = ClipboardWriter.CreateDib(image);

        Assert.Equal(40, BitConverter.ToInt32(dib, 0));   // tamaño del encabezado
        Assert.Equal(3, BitConverter.ToInt32(dib, 4));    // ancho
        Assert.Equal(2, BitConverter.ToInt32(dib, 8));    // alto (positivo: de abajo hacia arriba)
        Assert.Equal(1, BitConverter.ToInt16(dib, 12));   // planos
        Assert.Equal(32, BitConverter.ToInt16(dib, 14));  // bits por píxel
        Assert.Equal(0, BitConverter.ToInt32(dib, 16));   // sin compresión
        Assert.Equal(3 * 2 * 4, BitConverter.ToInt32(dib, 20));
        Assert.Equal(40 + 3 * 2 * 4, dib.Length);
    }

    [Fact]
    public void Dib_FirstRowIsTheBottomOfTheImage_InBlueGreenRedOrder()
    {
        using Bitmap image = SampleImage();

        byte[] dib = ClipboardWriter.CreateDib(image);

        // Primera fila guardada = fila de abajo (azul); azul-verde-rojo-alfa
        Assert.Equal(new byte[] { 220, 40, 30, 255 }, dib[40..44]);
        // Segunda fila guardada = fila de arriba (rojo)
        int secondRow = 40 + 3 * 4;
        Assert.Equal(new byte[] { 20, 10, 200, 255 }, dib[secondRow..(secondRow + 4)]);
    }

    [Fact]
    public void Png_IsReadableWithSameSizeAndPixels()
    {
        using Bitmap image = SampleImage();

        byte[] png = ClipboardWriter.CreatePng(image);

        using var stream = new MemoryStream(png);
        using var decoded = new Bitmap(stream);
        Assert.Equal(new Size(3, 2), decoded.Size);
        Assert.Equal(image.GetPixel(0, 0).ToArgb(), decoded.GetPixel(0, 0).ToArgb());
        Assert.Equal(image.GetPixel(0, 1).ToArgb(), decoded.GetPixel(0, 1).ToArgb());
    }
}
