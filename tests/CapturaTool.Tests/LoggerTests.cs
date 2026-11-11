using CapturaTool.Platform;

namespace CapturaTool.Tests;

public class LoggerTests : IDisposable
{
    // Carpeta temporal única para cada prueba; se borra al terminar
    private readonly string _tempFolder =
        Path.Combine(Path.GetTempPath(), "CapturaToolTests_" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(_tempFolder))
        {
            Directory.Delete(_tempFolder, recursive: true);
        }
    }

    [Fact]
    public void Write_CreatesFolderIfMissing()
    {
        string logFolder = Path.Combine(_tempFolder, "logs");
        var logger = new Logger(logFolder);

        logger.Write("hola");

        Assert.True(File.Exists(logger.CurrentFilePath));
    }

    [Fact]
    public void Write_IncludesMessageAndExceptionType()
    {
        var logger = new Logger(_tempFolder);

        logger.Write("Error de prueba", new InvalidOperationException("detalle"));

        string content = File.ReadAllText(logger.CurrentFilePath);
        Assert.Contains("Error de prueba", content);
        Assert.Contains("InvalidOperationException", content);
        Assert.Contains("detalle", content);
    }

    [Fact]
    public void Write_AppendsToSameFile()
    {
        var logger = new Logger(_tempFolder);

        logger.Write("primero");
        logger.Write("segundo");

        string content = File.ReadAllText(logger.CurrentFilePath);
        Assert.Contains("primero", content);
        Assert.Contains("segundo", content);
        Assert.Single(Directory.GetFiles(_tempFolder));
    }

    [Fact]
    public void Write_DoesNotThrowWhenFolderCannotBeCreated()
    {
        // Una "carpeta" dentro de un archivo no se puede crear
        Directory.CreateDirectory(_tempFolder);
        string file = Path.Combine(_tempFolder, "archivo.txt");
        File.WriteAllText(file, "");
        var logger = new Logger(Path.Combine(file, "logs"));

        var error = Record.Exception(() => logger.Write("no debe fallar"));

        Assert.Null(error);
    }

    [Fact]
    public void CurrentFilePath_HasDateInName()
    {
        var logger = new Logger(_tempFolder);

        string name = Path.GetFileName(logger.CurrentFilePath);

        Assert.Matches(@"^CapturaTool_\d{4}-\d{2}-\d{2}\.log$", name);
    }
}
