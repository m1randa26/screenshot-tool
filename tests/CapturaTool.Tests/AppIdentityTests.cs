using System.Windows.Forms;
using CapturaTool.Platform;

namespace CapturaTool.Tests;

public class AppIdentityTests
{
    private static readonly AppIdentity Stable = AppIdentity.Stable;
    private static readonly AppIdentity Insider = AppIdentity.Insider;

    [Fact]
    public void NormalBuild_IsTheStableVersion()
    {
        Assert.Same(AppIdentity.Stable, AppIdentity.Current);
    }

    [Fact]
    public void Stable_KeepsTheValuesItHadBefore()
    {
        // La estable no debe cambiar nada de lo que ya está instalado (carpeta, mutex, arranque)
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Assert.Equal("CapturaTool", Stable.DisplayName);
        Assert.Equal(Path.Combine(appData, "CapturaTool"), Stable.DataFolder);
        Assert.Equal("Local\\CapturaTool_SingleInstance", Stable.MutexName);
        Assert.Equal("CapturaTool", Stable.AutoStartValueName);
        Assert.Equal("CapturaTool.ico", Stable.IconResourceName);
        Assert.Equal(KeyModifiers.Control | KeyModifiers.Alt, Stable.HotkeyModifiers);
    }

    [Fact]
    public void StableAndInsider_ShareNothing()
    {
        Assert.NotEqual(Stable.DisplayName, Insider.DisplayName);
        Assert.NotEqual(Stable.DataFolder, Insider.DataFolder);
        Assert.NotEqual(Stable.MutexName, Insider.MutexName);
        Assert.NotEqual(Stable.AutoStartValueName, Insider.AutoStartValueName);
        Assert.NotEqual(Stable.IconResourceName, Insider.IconResourceName);
        Assert.NotEqual(Stable.HotkeyModifiers, Insider.HotkeyModifiers);
    }

    [Fact]
    public void Insider_DataFolderIsNotInsideTheStableOne()
    {
        // Si una carpeta estuviera dentro de la otra, borrar una afectaría a la otra
        Assert.False(Insider.DataFolder.StartsWith(Stable.DataFolder + Path.DirectorySeparatorChar));
        Assert.False(Stable.DataFolder.StartsWith(Insider.DataFolder + Path.DirectorySeparatorChar));
    }

    [Fact]
    public void Insider_AddsShiftToTheHotkey()
    {
        Assert.Equal(Stable.HotkeyModifiers | KeyModifiers.Shift, Insider.HotkeyModifiers);
    }

    [Fact]
    public void DescribeHotkey_WritesItForTheUser()
    {
        Assert.Equal("Ctrl + Alt + S", Stable.DescribeHotkey(Keys.S));
        Assert.Equal("Ctrl + Alt + Shift + S", Insider.DescribeHotkey(Keys.S));
    }
}
