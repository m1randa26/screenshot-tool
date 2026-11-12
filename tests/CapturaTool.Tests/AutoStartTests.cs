using CapturaTool.Platform;
using Microsoft.Win32;

namespace CapturaTool.Tests;

/// <summary>
/// Usa una clave propia del registro (no la de arranque real) y la borra al terminar.
/// </summary>
public class AutoStartTests : IDisposable
{
    private readonly string _keyPath = @"Software\CapturaToolTests_" + Guid.NewGuid();
    private readonly string _logFolder = Path.Combine(Path.GetTempPath(), "CapturaToolTests_" + Guid.NewGuid());
    private readonly AutoStart _autoStart;

    public AutoStartTests()
    {
        _autoStart = new AutoStart(_keyPath, new Logger(_logFolder));
    }

    public void Dispose()
    {
        Registry.CurrentUser.DeleteSubKeyTree(_keyPath, throwOnMissingSubKey: false);
        if (Directory.Exists(_logFolder))
        {
            Directory.Delete(_logFolder, recursive: true);
        }
    }

    [Fact]
    public void RealKey_IsTheWindowsRunKey()
    {
        Assert.Equal(@"Software\Microsoft\Windows\CurrentVersion\Run", AutoStart.WindowsRunKey);
        Assert.Equal("CapturaTool", AutoStart.ValueName);
    }

    [Fact]
    public void Command_IsThePathInQuotes()
    {
        Assert.Equal("\"C:\\Mis programas\\CapturaTool.exe\"", AutoStart.BuildCommand(@"C:\Mis programas\CapturaTool.exe"));
    }

    [Fact]
    public void Enable_WritesThePath()
    {
        bool ok = _autoStart.TryApply(true, @"C:\Herramientas\CapturaTool.exe");

        Assert.True(ok);
        Assert.Equal("\"C:\\Herramientas\\CapturaTool.exe\"", _autoStart.ReadCommand());
    }

    [Fact]
    public void Disable_RemovesTheEntry()
    {
        _autoStart.TryApply(true, @"C:\Herramientas\CapturaTool.exe");

        bool ok = _autoStart.TryApply(false, @"C:\Herramientas\CapturaTool.exe");

        Assert.True(ok);
        Assert.Null(_autoStart.ReadCommand());
    }

    [Fact]
    public void Disable_WhenThereIsNoEntry_DoesNotFail()
    {
        bool ok = _autoStart.TryApply(false, @"C:\Herramientas\CapturaTool.exe");

        Assert.True(ok);
        Assert.Null(_autoStart.ReadCommand());
    }

    [Fact]
    public void Enable_AfterTheExeMoved_ReplacesThePath()
    {
        _autoStart.TryApply(true, @"C:\Viejo\CapturaTool.exe");

        _autoStart.TryApply(true, @"D:\Nuevo\CapturaTool.exe");

        Assert.Equal("\"D:\\Nuevo\\CapturaTool.exe\"", _autoStart.ReadCommand());
    }

    [Fact]
    public void ReadCommand_KeyMissing_GivesNull()
    {
        Assert.Null(_autoStart.ReadCommand());
    }
}
