using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using IconMaker;

// Genera el logo de CapturaTool (R33): el .ico de la app y una vista previa para revisarlo.
// Uso, desde la carpeta del repo:  dotnet run --project tools/IconMaker

int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 256 };

string repo = FindRepoRoot();
string icoPath = Path.Combine(repo, "src", "CapturaTool", "Resources", "CapturaTool.ico");
string previewPath = Path.Combine(repo, "tools", "IconMaker", "preview.png");

List<Bitmap> images = sizes.Select(LogoPainter.Paint).ToList();
try
{
    Directory.CreateDirectory(Path.GetDirectoryName(icoPath)!);
    IcoWriter.Write(icoPath, images);
    Console.WriteLine($"Ícono: {icoPath}");
    Console.WriteLine($"Tamaños: {string.Join(", ", sizes)}");

    using Bitmap preview = CreatePreview(images);
    preview.Save(previewPath, ImageFormat.Png);
    Console.WriteLine($"Vista previa: {previewPath}");
}
finally
{
    foreach (Bitmap image in images)
    {
        image.Dispose();
    }
}

// Busca la carpeta que tiene CapturaTool.slnx, subiendo desde donde está el programa
static string FindRepoRoot()
{
    var folder = new DirectoryInfo(AppContext.BaseDirectory);
    while (folder != null && !File.Exists(Path.Combine(folder.FullName, "CapturaTool.slnx")))
    {
        folder = folder.Parent;
    }
    return folder?.FullName ?? throw new InvalidOperationException("No se encontró la carpeta del repo (CapturaTool.slnx).");
}

// Dos franjas (fondo claro y oscuro, como la barra de tareas). En cada una, cada tamaño a tamaño real
// y, hasta 48 px, también ampliado 8 veces para ver los píxeles.
static Bitmap CreatePreview(IReadOnlyList<Bitmap> images)
{
    const int Zoom = 8;
    const int Gap = 24;
    const int LabelHeight = 22;

    int Width(Bitmap image) => image.Width + (image.Width <= 48 ? Gap / 2 + image.Width * Zoom : 0);
    int stripHeight = Gap + LabelHeight + Math.Max(256, 48 * Zoom) + Gap;
    int totalWidth = Gap + images.Sum(image => Width(image) + Gap);

    var preview = new Bitmap(totalWidth, stripHeight * 2, PixelFormat.Format32bppArgb);
    using Graphics g = Graphics.FromImage(preview);
    using var font = new Font("Segoe UI", 11f, GraphicsUnit.Pixel);

    (Color Back, Color Text)[] themes = { (Color.FromArgb(243, 243, 243), Color.FromArgb(40, 40, 40)), (Color.FromArgb(32, 32, 32), Color.FromArgb(220, 220, 220)) };
    for (int t = 0; t < themes.Length; t++)
    {
        int top = t * stripHeight;
        using var back = new SolidBrush(themes[t].Back);
        using var text = new SolidBrush(themes[t].Text);
        g.FillRectangle(back, 0, top, totalWidth, stripHeight);

        int x = Gap;
        foreach (Bitmap image in images)
        {
            int y = top + Gap + LabelHeight;
            g.DrawString($"{image.Width} px", font, text, x, top + Gap);

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(image, x, y, image.Width, image.Height);
            if (image.Width <= 48)
            {
                g.DrawImage(image, x + image.Width + Gap / 2, y, image.Width * Zoom, image.Height * Zoom);
            }
            x += Width(image) + Gap;
        }
    }
    return preview;
}
