using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CapturaTool.Platform;

namespace CapturaTool.Output;

/// <summary>
/// Copia una imagen al portapapeles con reintentos y verifica que quedó ahí (R21, R22).
/// Solo reporta éxito si la verificación pasa.
/// </summary>
public class ClipboardWriter
{
    private const int MaxAttempts = 10;
    private const int DelayMs = 100;

    /// <summary>Formato que usan los navegadores (GitHub) y Slack para imágenes.</summary>
    public const string PngFormat = "PNG";

    private readonly Logger _logger;
    private Exception? _lastError;

    public ClipboardWriter(Logger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Debe llamarse desde el hilo de la interfaz (el portapapeles lo exige).
    /// </summary>
    public bool TryCopy(Bitmap image)
    {
        // MemoryStream no tiene recursos de Windows; igual se libera al terminar
        using var png = new MemoryStream();
        DataObject data = CreateDataObject(image, png);
        _lastError = null;

        bool ok = Retry.Run(() => TryOnce(data, png), MaxAttempts, () => Thread.Sleep(DelayMs), out int attempts);

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
    /// Prepara la imagen en dos formatos: mapa de bits estándar y PNG.
    /// El PNG se escribe en <paramref name="png"/>.
    /// </summary>
    public static DataObject CreateDataObject(Bitmap image, MemoryStream png)
    {
        var data = new DataObject();
        data.SetImage(image);

        image.Save(png, ImageFormat.Png);
        data.SetData(PngFormat, false, png);
        return data;
    }

    private bool TryOnce(DataObject data, MemoryStream png)
    {
        try
        {
            // El PNG se lee desde la posición actual: volver al inicio en cada intento
            png.Position = 0;

            // copy: true deja la imagen en el portapapeles aunque el editor o el programa se cierren.
            // retryTimes: 0 porque WinForms reintenta 10 veces por su cuenta si no se le indica;
            // los reintentos los controla Retry, para que el editor no se congele más de un segundo.
            Clipboard.SetDataObject(data, copy: true, retryTimes: 0, retryDelay: 0);

            // Verificación: leer de vuelta y comprobar que están los dos formatos
            return Clipboard.ContainsImage() && Clipboard.ContainsData(PngFormat);
        }
        catch (ExternalException ex)
        {
            // Lo normal: otro programa tiene el portapapeles abierto en este instante
            _lastError = ex;
            return false;
        }
    }
}
