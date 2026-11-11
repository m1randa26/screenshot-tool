using System.Runtime.InteropServices;

namespace CapturaTool.Platform;

/// <summary>Teclas modificadoras de un atajo global. Se pueden combinar con |.</summary>
[Flags]
public enum KeyModifiers
{
    Alt = 0x1,
    Control = 0x2,
    Shift = 0x4,
}

/// <summary>
/// Atajo de teclado global: funciona aunque el foco esté en otro programa (R4).
/// Windows avisa de cada pulsación con un mensaje a una ventana; por eso esta clase
/// es una ventana invisible (NativeWindow) que solo escucha ese mensaje.
/// </summary>
public sealed class GlobalHotkey : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_NOREPEAT = 0x4000; // mantener las teclas apretadas no repite el atajo
    private const int HotkeyId = 1;

    private bool _registered;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>Se dispara cada vez que se presiona el atajo.</summary>
    public event EventHandler? Pressed;

    public GlobalHotkey()
    {
        CreateHandle(new CreateParams());
    }

    /// <summary>
    /// Registra el atajo. Devuelve false si no se pudo, por ejemplo porque otro programa ya lo usa (R5).
    /// </summary>
    public bool Register(KeyModifiers modifiers, Keys key)
    {
        _registered = RegisterHotKey(Handle, HotkeyId, (int)modifiers | MOD_NOREPEAT, (int)key);
        return _registered;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam == HotkeyId)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(Handle, HotkeyId);
            _registered = false;
        }
        DestroyHandle();
    }
}
