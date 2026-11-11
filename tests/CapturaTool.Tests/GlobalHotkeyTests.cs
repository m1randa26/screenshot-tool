using System.Windows.Forms;
using CapturaTool.Platform;

namespace CapturaTool.Tests;

public class GlobalHotkeyTests
{
    // Combinación que nadie usa, para no chocar con el programa real (Ctrl + Alt + S)
    private const KeyModifiers TestModifiers = KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift;
    private const Keys TestKey = Keys.F24;

    [Fact]
    public void Register_FreeHotkey_Succeeds()
    {
        using var hotkey = new GlobalHotkey();

        Assert.True(hotkey.Register(TestModifiers, TestKey));
    }

    [Fact]
    public void Register_HotkeyAlreadyTaken_ReturnsFalse()
    {
        using var first = new GlobalHotkey();
        using var second = new GlobalHotkey();
        first.Register(TestModifiers, TestKey);

        Assert.False(second.Register(TestModifiers, TestKey));
    }

    [Fact]
    public void Register_AfterDispose_CanRegisterAgain()
    {
        var first = new GlobalHotkey();
        first.Register(TestModifiers, TestKey);
        first.Dispose();

        using var again = new GlobalHotkey();

        Assert.True(again.Register(TestModifiers, TestKey));
    }
}
