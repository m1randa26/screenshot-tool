using System.Drawing;
using System.Windows.Forms;
using CapturaTool.Output;

namespace CapturaTool.Tests;

// Solo prueba cómo se prepara la imagen; no toca el portapapeles real
public class ClipboardWriterTests
{
    [Fact]
    public void CreateDataObject_HasBitmapAndPngFormats()
    {
        using var image = new Bitmap(120, 80);
        using var png = new MemoryStream();

        DataObject data = ClipboardWriter.CreateDataObject(image, png);

        Assert.True(data.GetDataPresent(DataFormats.Bitmap));
        Assert.True(data.GetDataPresent(ClipboardWriter.PngFormat));
    }

    [Fact]
    public void CreateDataObject_PngIsReadableWithSameSize()
    {
        using var image = new Bitmap(120, 80);
        using var png = new MemoryStream();

        ClipboardWriter.CreateDataObject(image, png);

        png.Position = 0;
        using var decoded = Image.FromStream(png);
        Assert.Equal(new Size(120, 80), decoded.Size);
    }
}
