using System.Text.Json;
using CapturaTool.Platform;

namespace CapturaTool.Configuration;

/// <summary>
/// Configuración del programa, guardada como JSON en %APPDATA%\CapturaTool\settings.json (R31, NF7).
/// Si el archivo no existe o está dañado, se usan los valores por defecto y el programa arranca igual.
/// </summary>
public class AppSettings
{
    // camelCase: en el archivo las claves son "saveFolder" y "startWithWindows"
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        // Por si alguien edita el archivo a mano
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Carpeta inicial: Imágenes\Capturas del usuario (R29).</summary>
    public static string DefaultSaveFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Capturas");

    /// <summary>%APPDATA%\CapturaTool\settings.json (en la Insider, %APPDATA%\CapturaTool Insider\settings.json)</summary>
    public static string DefaultFilePath => Path.Combine(AppIdentity.Current.DataFolder, "settings.json");

    /// <summary>Carpeta donde se guardan las capturas (R28, R29).</summary>
    public string SaveFolder { get; set; } = DefaultSaveFolder;

    /// <summary>Si el programa arranca con Windows (R1, R30).</summary>
    public bool StartWithWindows { get; set; } = true;

    /// <summary>
    /// Lee la configuración. Nunca lanza errores: si el archivo no existe o está dañado,
    /// devuelve los valores por defecto (y, si estaba dañado, lo anota en el log).
    /// </summary>
    public static AppSettings Load(string path, Logger logger)
    {
        if (!File.Exists(path))
        {
            return new AppSettings();
        }

        try
        {
            string json = File.ReadAllText(path);

            // El texto "null" da null; un campo que falta conserva su valor por defecto
            AppSettings settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();

            if (string.IsNullOrWhiteSpace(settings.SaveFolder))
            {
                settings.SaveFolder = DefaultSaveFolder;
            }
            return settings;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // El archivo dañado no se toca: se reemplaza cuando se guarde desde Configuración
            logger.Write($"No se pudo leer la configuración de {path}; se usan los valores por defecto", ex);
            return new AppSettings();
        }
    }

    /// <summary>
    /// Guarda la configuración. Devuelve false con un mensaje para el usuario si no se pudo.
    /// Escribe primero a un archivo temporal, para que un fallo a mitad de camino no deje el archivo dañado.
    /// </summary>
    public bool TrySave(string path, Logger logger, out string errorMessage)
    {
        errorMessage = "";
        string tempPath = path + ".tmp";

        try
        {
            string? folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(tempPath, JsonSerializer.Serialize(this, JsonOptions));
            File.Move(tempPath, path, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.Write($"No se pudo guardar la configuración en {path}", ex);
            errorMessage = $"No se pudo guardar la configuración:\n{ex.Message}";
            return false;
        }
    }
}
