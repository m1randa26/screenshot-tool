using System.Globalization;

namespace CapturaTool.Platform;

/// <summary>
/// Escribe mensajes y errores en un archivo de log por día (R32).
/// </summary>
public class Logger
{
    private readonly string _folder;
    private readonly object _lock = new();

    public Logger(string folder)
    {
        _folder = folder;
    }

    /// <summary>%APPDATA%\CapturaTool\logs</summary>
    public static string DefaultFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CapturaTool",
            "logs");

    /// <summary>Archivo del día de hoy, por ejemplo CapturaTool_2026-11-10.log</summary>
    public string CurrentFilePath =>
        Path.Combine(_folder, $"CapturaTool_{Today()}.log");

    public void Write(string message, Exception? exception = null)
    {
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string text = $"{time} | {message}{Environment.NewLine}";
        if (exception != null)
        {
            text += exception + Environment.NewLine;
        }

        // El lock evita que dos errores simultáneos mezclen sus líneas
        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(_folder);
                File.AppendAllText(CurrentFilePath, text);
            }
            catch
            {
                // Si no se puede escribir el log, no hay a dónde reportarlo:
                // se ignora para no provocar un error nuevo mientras se maneja otro.
            }
        }
    }

    private static string Today() =>
        DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
