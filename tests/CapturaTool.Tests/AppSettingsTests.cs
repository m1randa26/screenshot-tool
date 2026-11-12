using CapturaTool.Configuration;
using CapturaTool.Platform;

namespace CapturaTool.Tests;

public class AppSettingsTests : IDisposable
{
    // Carpeta temporal única para cada prueba; se borra al terminar
    private readonly string _tempFolder =
        Path.Combine(Path.GetTempPath(), "CapturaToolTests_" + Guid.NewGuid());

    private readonly Logger _logger;
    private readonly string _path;

    public AppSettingsTests()
    {
        _logger = new Logger(Path.Combine(_tempFolder, "logs"));
        _path = Path.Combine(_tempFolder, "settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempFolder))
        {
            Directory.Delete(_tempFolder, recursive: true);
        }
    }

    private void WriteSettingsFile(string text)
    {
        Directory.CreateDirectory(_tempFolder);
        File.WriteAllText(_path, text);
    }

    private string LogText() =>
        File.Exists(_logger.CurrentFilePath) ? File.ReadAllText(_logger.CurrentFilePath) : "";

    // --- Valores por defecto ---

    [Fact]
    public void DefaultSaveFolder_IsCapturasInsidePictures()
    {
        string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        Assert.Equal(Path.Combine(pictures, "Capturas"), AppSettings.DefaultSaveFolder);
    }

    [Fact]
    public void NewSettings_HaveTheDefaultValues()
    {
        var settings = new AppSettings();

        Assert.Equal(AppSettings.DefaultSaveFolder, settings.SaveFolder);
        Assert.True(settings.StartWithWindows);
    }

    [Fact]
    public void DefaultFilePath_IsInsideAppData()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        Assert.Equal(Path.Combine(appData, "CapturaTool", "settings.json"), AppSettings.DefaultFilePath);
    }

    // --- Archivo inexistente ---

    [Fact]
    public void Load_FileMissing_GivesDefaults()
    {
        AppSettings settings = AppSettings.Load(_path, _logger);

        Assert.Equal(AppSettings.DefaultSaveFolder, settings.SaveFolder);
        Assert.True(settings.StartWithWindows);
    }

    // --- Archivo dañado ---

    [Theory]
    [InlineData("esto no es JSON")]
    [InlineData("")]
    [InlineData("{ \"saveFolder\": ")]
    [InlineData("{ \"startWithWindows\": \"tal vez\" }")]
    public void Load_DamagedFile_GivesDefaults_AndWritesToLog(string content)
    {
        WriteSettingsFile(content);

        AppSettings settings = AppSettings.Load(_path, _logger);

        Assert.Equal(AppSettings.DefaultSaveFolder, settings.SaveFolder);
        Assert.True(settings.StartWithWindows);
        Assert.Contains("No se pudo leer la configuración", LogText());
    }

    [Fact]
    public void Load_DamagedFile_IsNotOverwritten()
    {
        WriteSettingsFile("esto no es JSON");

        AppSettings.Load(_path, _logger);

        Assert.Equal("esto no es JSON", File.ReadAllText(_path));
    }

    [Fact]
    public void Load_NullJson_GivesDefaults()
    {
        WriteSettingsFile("null");

        AppSettings settings = AppSettings.Load(_path, _logger);

        Assert.Equal(AppSettings.DefaultSaveFolder, settings.SaveFolder);
        Assert.True(settings.StartWithWindows);
    }

    // --- Campos que faltan o vienen vacíos ---

    [Fact]
    public void Load_MissingField_KeepsItsDefault()
    {
        WriteSettingsFile("{ \"startWithWindows\": false }");

        AppSettings settings = AppSettings.Load(_path, _logger);

        Assert.Equal(AppSettings.DefaultSaveFolder, settings.SaveFolder);
        Assert.False(settings.StartWithWindows);
    }

    [Theory]
    [InlineData("{ \"saveFolder\": \"\" }")]
    [InlineData("{ \"saveFolder\": \"   \" }")]
    [InlineData("{ \"saveFolder\": null }")]
    public void Load_EmptyFolder_UsesDefaultFolder(string content)
    {
        WriteSettingsFile(content);

        AppSettings settings = AppSettings.Load(_path, _logger);

        Assert.Equal(AppSettings.DefaultSaveFolder, settings.SaveFolder);
    }

    // --- Guardar y volver a leer (R31) ---

    [Fact]
    public void SaveAndLoad_KeepsTheValues()
    {
        var original = new AppSettings { SaveFolder = @"D:\Mis capturas", StartWithWindows = false };

        bool ok = original.TrySave(_path, _logger, out string error);
        AppSettings loaded = AppSettings.Load(_path, _logger);

        Assert.True(ok, error);
        Assert.Equal(@"D:\Mis capturas", loaded.SaveFolder);
        Assert.False(loaded.StartWithWindows);
    }

    [Fact]
    public void Save_CreatesMissingFolder()
    {
        string path = Path.Combine(_tempFolder, "sub", "settings.json");

        bool ok = new AppSettings().TrySave(path, _logger, out string error);

        Assert.True(ok, error);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void Save_ReplacesExistingFile_AndLeavesNoTempFile()
    {
        WriteSettingsFile("esto no es JSON");

        bool ok = new AppSettings { StartWithWindows = false }.TrySave(_path, _logger, out string error);

        Assert.True(ok, error);
        Assert.False(AppSettings.Load(_path, _logger).StartWithWindows);
        Assert.Equal(new[] { _path }, Directory.GetFiles(_tempFolder));
    }

    [Fact]
    public void Save_UsesTheDocumentedKeys()
    {
        new AppSettings().TrySave(_path, _logger, out _);

        string json = File.ReadAllText(_path);
        Assert.Contains("\"saveFolder\"", json);
        Assert.Contains("\"startWithWindows\"", json);
    }

    [Fact]
    public void Save_Failure_ReturnsFalseWithMessage_AndWritesToLog()
    {
        // Una "carpeta" dentro de un archivo no se puede crear
        Directory.CreateDirectory(_tempFolder);
        string file = Path.Combine(_tempFolder, "archivo.txt");
        File.WriteAllText(file, "");

        bool ok = new AppSettings().TrySave(Path.Combine(file, "settings.json"), _logger, out string error);

        Assert.False(ok);
        Assert.Contains("No se pudo guardar la configuración", error);
        Assert.Contains("No se pudo guardar la configuración", LogText());
    }
}
