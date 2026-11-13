using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using IconMaker;

// Genera los logos de CapturaTool (R33, R34): el .ico de la estable y el de la Insider,
// cada uno con una vista previa para revisarlo.
// Uso, desde la carpeta del repo:  dotnet run --project tools/IconMaker
// Para comparar los candidatos de la Insider sin escribir ningún .ico:
//   dotnet run --project tools/IconMaker -- --candidatos <carpeta>

int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 256 };

if (args.Length == 2 && args[0] == "--candidatos")
{
    SavePreview(LogoStyle.InsiderPurple, Path.Combine(args[1], "candidato-morado.png"));
    SavePreview(LogoStyle.InsiderBand, Path.Combine(args[1], "candidato-franja.png"));
    return;
}

string repo = FindRepoRoot();
string resources = Path.Combine(repo, "src", "CapturaTool", "Resources");
string tool = Path.Combine(repo, "tools", "IconMaker");

Generate(LogoStyle.Stable, Path.Combine(resources, "CapturaTool.ico"), Path.Combine(tool, "preview.png"));
Generate(LogoStyle.Insider, Path.Combine(resources, "CapturaTool.Insider.ico"), Path.Combine(tool, "preview-insider.png"));

// Escribe el .ico de un estilo y su vista previa
void Generate(LogoStyle style, string icoPath, string previewPath)
{
    List<Bitmap> images = sizes.Select(size => LogoPainter.Paint(size, style)).ToList();
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(icoPath)!);
        IcoWriter.Write(icoPath, images);
        Console.WriteLine($"Ícono: {icoPath} ({string.Join(", ", sizes)})");

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
}

// Solo la vista previa de un estilo (para comparar candidatos)
void SavePreview(LogoStyle style, string previewPath)
{
    List<Bitmap> images = sizes.Select(size => LogoPainter.Paint(size, style)).ToList();
    try
    {
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
