using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CapturaTool.Platform;

namespace CapturaTool.Output;

/// <summary>
/// Guarda la imagen como PNG (NF5) en la carpeta de capturas (R23).
/// Crea la carpeta si no existe y, si no puede guardar, lo dice con un mensaje claro (R25).
/// </summary>
public class FileSaver
{
    private readonly Logger _logger;

    public FileSaver(string folder, Logger logger)
    {
        Folder = folder;
        _logger = logger;
    }

    /// <summary>Carpeta donde se guardan las capturas (viene de la configuración).</summary>
    public string Folder { get; set; }

    /// <summary>
    /// Guarda la imagen. Devuelve true si funcionó (con la ruta del archivo en <paramref name="savedPath"/>);
    /// si no, false con un mensaje para el usuario en <paramref name="errorMessage"/>. Nunca lanza errores.
    /// </summary>
    public bool TrySave(Bitmap image, out string savedPath, out string errorMessage)
    {
        savedPath = "";
        errorMessage = "";
        string path = "";
        bool fileCreated = false;

        try
        {
            Directory.CreateDirectory(Folder); // R25: si la carpeta no existe (o se borró), se crea
            path = FileNamer.GetAvailablePath(Folder, DateTime.Now);

            // CreateNew: si por casualidad el archivo apareció justo ahora, falla en vez de pisarlo
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                fileCreated = true;
                image.Save(stream, ImageFormat.Png);
            }

            savedPath = path;
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            errorMessage = $"No hay permiso para escribir en la carpeta:\n{Folder}";
            Fail(path, fileCreated, ex);
        }
        catch (IOException ex)
        {
            errorMessage = $"No se pudo guardar en la carpeta:\n{Folder}\n\n{ex.Message}";
            Fail(path, fileCreated, ex);
        }
        catch (Exception ex) when (ex is ExternalException or ArgumentException or NotSupportedException)
        {
            // ExternalException: error de GDI+ al escribir el PNG; las otras: ruta de carpeta inválida
            errorMessage = $"No se pudo guardar la imagen en la carpeta:\n{Folder}\n\n{ex.Message}";
            Fail(path, fileCreated, ex);
        }
        return false;
    }

    /// <summary>Anota el error en el log y borra el archivo a medio escribir, si quedó alguno.</summary>
    private void Fail(string path, bool fileCreated, Exception ex)
    {
        _logger.Write($"No se pudo guardar la captura en {Folder}", ex);

        if (fileCreated)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception deleteError) when (deleteError is IOException or UnauthorizedAccessException)
            {
                // Si tampoco se puede borrar, queda anotado; no hay más que hacer
                _logger.Write($"No se pudo borrar el archivo incompleto {path}", deleteError);
            }
        }
    }
}
