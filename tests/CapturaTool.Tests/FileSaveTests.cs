using System.Drawing;
using System.Security.AccessControl;
using System.Security.Principal;
using CapturaTool.Output;
using CapturaTool.Platform;

namespace CapturaTool.Tests;

public class FileSaveTests : IDisposable
{
    // Carpeta temporal única para cada prueba; se borra al terminar
    private readonly string _tempFolder =
        Path.Combine(Path.GetTempPath(), "CapturaToolTests_" + Guid.NewGuid());

    private readonly Logger _logger;

    public FileSaveTests()
    {
        _logger = new Logger(Path.Combine(_tempFolder, "logs"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempFolder))
        {
            Directory.Delete(_tempFolder, recursive: true);
        }
    }

    private static readonly DateTime SampleTime = new(2026, 11, 12, 9, 5, 7);

    // --- FileNamer ---

    [Fact]
    public void BaseName_HasTheRequiredFormat()
    {
        Assert.Equal("captura_2026-11-12_09-05-07", FileNamer.CreateBaseName(SampleTime));
    }

    [Fact]
    public void AvailablePath_WhenFree_HasNoSuffix()
    {
        Directory.CreateDirectory(_tempFolder);

        string path = FileNamer.GetAvailablePath(_tempFolder, SampleTime);

        Assert.Equal(Path.Combine(_tempFolder, "captura_2026-11-12_09-05-07.png"), path);
    }

    [Fact]
    public void AvailablePath_SameSecondTwice_AddsSuffix2()
    {
        Directory.CreateDirectory(_tempFolder);
        File.WriteAllText(Path.Combine(_tempFolder, "captura_2026-11-12_09-05-07.png"), "");

        string path = FileNamer.GetAvailablePath(_tempFolder, SampleTime);

        Assert.Equal(Path.Combine(_tempFolder, "captura_2026-11-12_09-05-07_2.png"), path);
    }

    [Fact]
    public void AvailablePath_WithNameAndSuffix2Taken_Gives3()
    {
        Directory.CreateDirectory(_tempFolder);
        File.WriteAllText(Path.Combine(_tempFolder, "captura_2026-11-12_09-05-07.png"), "");
        File.WriteAllText(Path.Combine(_tempFolder, "captura_2026-11-12_09-05-07_2.png"), "");

        string path = FileNamer.GetAvailablePath(_tempFolder, SampleTime);

        Assert.Equal(Path.Combine(_tempFolder, "captura_2026-11-12_09-05-07_3.png"), path);
    }

    [Fact]
    public void AvailablePath_FolderMissing_DoesNotFail()
    {
        string missing = Path.Combine(_tempFolder, "no-existe");

        string path = FileNamer.GetAvailablePath(missing, SampleTime);

        Assert.EndsWith("captura_2026-11-12_09-05-07.png", path);
    }

    // --- FileSaver ---

    private static Bitmap PatternImage()
    {
        var image = new Bitmap(40, 30);
        for (int y = 0; y < 30; y++)
        {
            for (int x = 0; x < 40; x++)
            {
                image.SetPixel(x, y, Color.FromArgb(x * 6, y * 8, (x + y) * 3));
            }
        }
        return image;
    }

    [Fact]
    public void Save_CreatesMissingFolder_AndWritesPng()
    {
        string folder = Path.Combine(_tempFolder, "Capturas");
        var saver = new FileSaver(folder, _logger);
        using Bitmap image = PatternImage();

        bool ok = saver.TrySave(image, out string path, out string error);

        Assert.True(ok, error);
        Assert.True(File.Exists(path));
        Assert.StartsWith(folder, path);
        Assert.Matches(@"captura_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}(_\d+)?\.png$", path);
    }

    [Fact]
    public void Save_PngIsIdenticalPixelByPixel()
    {
        var saver = new FileSaver(_tempFolder, _logger);
        using Bitmap image = PatternImage();

        saver.TrySave(image, out string path, out _);

        using var loaded = new Bitmap(path);
        Assert.Equal(image.Size, loaded.Size);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Assert.Equal(image.GetPixel(x, y).ToArgb(), loaded.GetPixel(x, y).ToArgb());
            }
        }
    }

    [Fact]
    public void Save_TwiceInARow_DoesNotOverwrite()
    {
        var saver = new FileSaver(_tempFolder, _logger);
        using Bitmap image = PatternImage();

        saver.TrySave(image, out string first, out _);
        saver.TrySave(image, out string second, out _);

        Assert.NotEqual(first, second);
        Assert.True(File.Exists(first));
        Assert.True(File.Exists(second));
    }

    [Fact]
    public void Save_FolderCannotBeCreated_ReturnsFalseWithMessage()
    {
        // Una "carpeta" dentro de un archivo no se puede crear
        Directory.CreateDirectory(_tempFolder);
        string file = Path.Combine(_tempFolder, "archivo.txt");
        File.WriteAllText(file, "");
        var saver = new FileSaver(Path.Combine(file, "Capturas"), _logger);
        using Bitmap image = PatternImage();

        bool ok = saver.TrySave(image, out string path, out string error);

        Assert.False(ok);
        Assert.Equal("", path);
        Assert.Contains("No se pudo guardar", error);
    }

    [Fact]
    public void Save_FolderWithoutWritePermission_ReturnsFalseWithPermissionMessage()
    {
        string folder = Path.Combine(_tempFolder, "SinPermiso");
        Directory.CreateDirectory(folder);
        var directory = new DirectoryInfo(folder);
        SecurityIdentifier me = WindowsIdentity.GetCurrent().User!;
        var denyWrite = new FileSystemAccessRule(me, FileSystemRights.CreateFiles, AccessControlType.Deny);

        DirectorySecurity security = directory.GetAccessControl();
        security.AddAccessRule(denyWrite);
        directory.SetAccessControl(security);
        try
        {
            var saver = new FileSaver(folder, _logger);
            using Bitmap image = PatternImage();

            bool ok = saver.TrySave(image, out _, out string error);

            Assert.False(ok);
            Assert.Contains("No hay permiso", error);
            Assert.Empty(Directory.GetFiles(folder));
        }
        finally
        {
            // Devolver el permiso para poder borrar la carpeta temporal
            security = directory.GetAccessControl();
            security.RemoveAccessRule(denyWrite);
            directory.SetAccessControl(security);
        }
    }

    [Fact]
    public void Save_Failure_IsWrittenToTheLog()
    {
        Directory.CreateDirectory(_tempFolder);
        string file = Path.Combine(_tempFolder, "archivo.txt");
        File.WriteAllText(file, "");
        var saver = new FileSaver(Path.Combine(file, "Capturas"), _logger);
        using Bitmap image = PatternImage();

        saver.TrySave(image, out _, out _);

        Assert.Contains("No se pudo guardar la captura", File.ReadAllText(_logger.CurrentFilePath));
    }
}
