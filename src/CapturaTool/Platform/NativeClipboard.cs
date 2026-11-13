using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CapturaTool.Platform;

/// <summary>
/// Escribe en el portapapeles con las funciones de Windows directamente.
/// No se usa Clipboard.SetDataObject de WinForms porque, con un Bitmap, deja una copia
/// de la imagen sin liberar en cada copia (medido en T17: +8 MB por captura de 1920 × 1080).
/// Debe usarse desde el hilo de la interfaz.
/// </summary>
public sealed class NativeClipboard : IDisposable
{
    private const uint CF_DIB = 8;
    private const uint GMEM_MOVEABLE = 0x0002;
    private static readonly IntPtr HWND_MESSAGE = new(-3);

    // Ventana invisible que queda como "dueña" del portapapeles; Windows la pide para escribir
    private readonly NativeWindow _owner = new();

    public NativeClipboard()
    {
        _owner.CreateHandle(new CreateParams { Parent = HWND_MESSAGE });
    }

    /// <summary>Número que Windows asigna a un formato con nombre, por ejemplo "PNG".</summary>
    public static uint RegisterFormat(string name) => RegisterClipboardFormat(name);

    /// <summary>
    /// Reemplaza el contenido del portapapeles por la imagen en dos formatos: DIB (mapa de bits estándar) y PNG.
    /// Devuelve false con el error de Windows si no se pudo (lo normal: otro programa lo tiene abierto).
    /// </summary>
    public bool TrySetImage(byte[] dib, byte[] png, uint pngFormat, out Win32Exception? error)
    {
        error = null;
        if (!OpenClipboard(_owner.Handle))
        {
            error = new Win32Exception(Marshal.GetLastWin32Error());
            return false;
        }

        try
        {
            if (!EmptyClipboard() || !TrySetData(CF_DIB, dib) || !TrySetData(pngFormat, png))
            {
                error = new Win32Exception(Marshal.GetLastWin32Error());
                return false;
            }
            return true;
        }
        finally
        {
            CloseClipboard();
        }
    }

    /// <summary>true si el portapapeles tiene ese formato ahora mismo (para verificar la copia).</summary>
    public static bool HasFormat(uint format) => IsClipboardFormatAvailable(format);

    /// <summary>Formato de mapa de bits estándar (lo leen Paint, Word, Slack…).</summary>
    public static uint DibFormat => CF_DIB;

    private static bool TrySetData(uint format, byte[] bytes)
    {
        // Windows pide los datos en un bloque de memoria "global"
        IntPtr handle = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
        if (handle == IntPtr.Zero)
        {
            return false;
        }

        IntPtr pointer = GlobalLock(handle);
        Marshal.Copy(bytes, 0, pointer, bytes.Length);
        GlobalUnlock(handle);

        if (SetClipboardData(format, handle) == IntPtr.Zero)
        {
            GlobalFree(handle); // no se entregó: se libera aquí
            return false;
        }
        return true; // desde aquí el bloque es de Windows: no se libera
    }

    public void Dispose()
    {
        _owner.DestroyHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr owner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint format, IntPtr memory);

    [DllImport("user32.dll")]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint RegisterClipboardFormat(string name);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint flags, UIntPtr size);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr memory);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr memory);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalFree(IntPtr memory);
}
