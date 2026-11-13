using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CapturaTool.Platform;

namespace CapturaTool.Output;

/// <summary>
/// Copia una imagen al portapapeles con reintentos y verifica que quedó ahí (R21, R22).
/// Solo reporta éxito si la verificación pasa.
/// </summary>
public sealed class ClipboardWriter : IDisposable
{
    private const int MaxAttempts = 10;
    private const int DelayMs = 100;

    /// <summary>Formato que usan los navegadores (GitHub) y Slack para imágenes.</summary>
    public const string PngFormat = "PNG";

    private readonly Logger _logger;
    private readonly NativeClipboard _clipboard = new();
    private readonly uint _pngFormat = NativeClipboard.RegisterFormat(PngFormat);
    private Win32Exception? _lastError;

    public ClipboardWriter(Logger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Debe llamarse desde el hilo de la interfaz (el portapapeles lo exige).
    /// </summary>
    public bool TryCopy(Bitmap image)
    {
        // La imagen en dos formatos: mapa de bits estándar (DIB) y PNG
        byte[] dib = CreateDib(image);
        byte[] png = CreatePng(image);
        _lastError = null;

        bool ok = Retry.Run(() => TryOnce(dib, png), MaxAttempts, () => Thread.Sleep(DelayMs), out int attempts);

        if (!ok)
        {
            string reason = _lastError == null ? " (la verificación no encontró la imagen)" : "";
            _logger.Write($"No se pudo copiar al portapapeles tras {MaxAttempts} intentos{reason}", _lastError);
        }
        else if (attempts > 1)
        {
            _logger.Write($"Copiado al portapapeles en el intento {attempts}");
        }
        return ok;
    }

    /// <summary>
    /// Mapa de bits estándar de Windows (DIB): un encabezado de 40 bytes y los píxeles
    /// en azul-verde-rojo-alfa, empezando por la fila de <b>abajo</b>.
    /// </summary>
    public static byte[] CreateDib(Bitmap image)
    {
        int width = image.Width;
        int height = image.Height;
        int rowBytes = width * 4;

        using var stream = new MemoryStream(40 + rowBytes * height);
        using var writer = new BinaryWriter(stream);

        // BITMAPINFOHEADER
        writer.Write(40);                // tamaño del encabezado
        writer.Write(width);
        writer.Write(height);            // positivo = filas de abajo hacia arriba
        writer.Write((short)1);          // planos
        writer.Write((short)32);         // bits por píxel
        writer.Write(0);                 // sin compresión (BI_RGB)
        writer.Write(rowBytes * height); // tamaño de los píxeles
        writer.Write(0);                 // resolución horizontal (no se usa)
        writer.Write(0);                 // resolución vertical (no se usa)
        writer.Write(0);                 // colores de paleta
        writer.Write(0);                 // colores importantes

        BitmapData data = image.LockBits(
            new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var row = new byte[rowBytes];
            for (int y = height - 1; y >= 0; y--)
            {
                Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, rowBytes);
                writer.Write(row);
            }
        }
        finally
        {
            image.UnlockBits(data);
        }

        writer.Flush();
        return stream.ToArray();
    }

    /// <summary>La imagen codificada como PNG.</summary>
    public static byte[] CreatePng(Bitmap image)
    {
        using var stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    private bool TryOnce(byte[] dib, byte[] png)
    {
        if (!_clipboard.TrySetImage(dib, png, _pngFormat, out Win32Exception? error))
        {
            // Lo normal: otro programa tiene el portapapeles abierto en este instante
            _lastError = error;
            return false;
        }

        // Verificación: comprobar que el portapapeles tiene los dos formatos
        return NativeClipboard.HasFormat(NativeClipboard.DibFormat) && NativeClipboard.HasFormat(_pngFormat);
    }

    public void Dispose()
    {
        _clipboard.Dispose();
    }
}
