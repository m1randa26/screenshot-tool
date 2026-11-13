using System.Drawing;
using CapturaTool.Platform;

namespace CapturaTool.Tests;

public class AppIconTests
{
    [Fact]
    public void Logo_IsInsideTheProgram()
    {
        using Stream stream = AppIcon.OpenResource();

        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void Logo_HasAllTheSizesWindowsAsksFor()
    {
        using Stream stream = AppIcon.OpenResource();

        IReadOnlyList<int> sizes = AppIcon.ReadSizes(stream);

        Assert.Equal(new[] { 16, 20, 24, 32, 40, 48, 64, 256 }, sizes);
    }

    [Fact]
    public void InsiderLogo_IsInsideTheProgram_WithAllTheSizes()
    {
        using Stream stream = AppIcon.OpenResource(AppIdentity.Insider.IconResourceName);

        Assert.Equal(new[] { 16, 20, 24, 32, 40, 48, 64, 256 }, AppIcon.ReadSizes(stream));
    }

    [Fact]
    public void InsiderLogo_IsNotTheStableOne()
    {
        // Un punto del fondo (borde izquierdo, a media altura): azul en la estable, morado en la Insider
        using var stableStream = AppIcon.OpenResource(AppIdentity.Stable.IconResourceName);
        using var insiderStream = AppIcon.OpenResource(AppIdentity.Insider.IconResourceName);
        using var stable = new Icon(stableStream, new Size(32, 32));
        using var insider = new Icon(insiderStream, new Size(32, 32));
        using Bitmap stableBitmap = stable.ToBitmap();
        using Bitmap insiderBitmap = insider.ToBitmap();

        Assert.NotEqual(stableBitmap.GetPixel(4, 16).ToArgb(), insiderBitmap.GetPixel(4, 16).ToArgb());
    }

    [Theory]
    [InlineData(16)] // bandeja al 100 %
    [InlineData(24)] // bandeja al 150 %
    [InlineData(32)] // barra de tareas al 100 %
    public void Load_WithSize_GivesExactlyThatSize(int size)
    {
        using Icon icon = AppIcon.Load(new Size(size, size));

        Assert.Equal(new Size(size, size), icon.Size);
    }

    [Fact]
    public void Load_WithoutSize_GivesAUsableIcon()
    {
        using Icon icon = AppIcon.Load();
        using Bitmap bitmap = icon.ToBitmap();

        // El centro del logo es parte de la flecha amarilla: el dibujo se cargó, no un ícono vacío
        Color center = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);
        Assert.Equal(255, center.A);
        Assert.True(center.R > 200 && center.G > 180 && center.B < 80, $"color del centro: {center}");
    }

    [Fact]
    public void ReadSizes_FileThatIsNotAnIcon_Fails()
    {
        using var stream = new MemoryStream(new byte[] { 0, 0, 2, 0, 1, 0 }); // tipo 2 = cursor

        Assert.Throws<InvalidDataException>(() => AppIcon.ReadSizes(stream));
    }
}
