using System.Globalization;

namespace CapturaTool.Output;

/// <summary>
/// Nombre automático de las capturas guardadas: captura_AAAA-MM-DD_HH-MM-SS.png (R24).
/// Si ya existe, agrega un sufijo: _2, _3…
/// </summary>
public static class FileNamer
{
    /// <summary>Nombre sin extensión para una hora dada, por ejemplo captura_2026-11-12_10-30-05.</summary>
    public static string CreateBaseName(DateTime time)
    {
        return "captura_" + time.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Ruta completa de un archivo que todavía no existe en la carpeta.
    /// Si la carpeta no existe, devuelve el nombre sin sufijo (no hay con qué chocar).
    /// </summary>
    public static string GetAvailablePath(string folder, DateTime time)
    {
        string baseName = CreateBaseName(time);
        string path = Path.Combine(folder, baseName + ".png");

        int suffix = 2;
        while (File.Exists(path))
        {
            path = Path.Combine(folder, $"{baseName}_{suffix}.png");
            suffix++;
        }
        return path;
    }
}
