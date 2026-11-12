using System.Security;
using Microsoft.Win32;

namespace CapturaTool.Platform;

/// <summary>
/// Activa o desactiva el arranque con Windows (R1, R30) con una entrada en el registro del usuario.
/// No necesita permisos de administrador.
/// </summary>
public class AutoStart
{
    /// <summary>Clave del registro que Windows lee al iniciar sesión.</summary>
    public const string WindowsRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>Nombre de la entrada dentro de la clave.</summary>
    public const string ValueName = "CapturaTool";

    private readonly string _keyPath;
    private readonly Logger _logger;

    /// <param name="keyPath">Clave dentro de HKEY_CURRENT_USER (las pruebas usan una propia).</param>
    public AutoStart(string keyPath, Logger logger)
    {
        _keyPath = keyPath;
        _logger = logger;
    }

    /// <summary>Texto que se guarda en el registro: la ruta entre comillas, por si tiene espacios.</summary>
    public static string BuildCommand(string exePath) => $"\"{exePath}\"";

    /// <summary>
    /// Activado: escribe (o reescribe) la ruta del .exe, por si se movió de carpeta.
    /// Desactivado: borra la entrada si existe. Devuelve false si no se pudo; nunca lanza errores.
    /// </summary>
    public bool TryApply(bool enabled, string exePath)
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(_keyPath);
            if (enabled)
            {
                key.SetValue(ValueName, BuildCommand(exePath), RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException or IOException)
        {
            _logger.Write("No se pudo cambiar el arranque con Windows", ex);
            return false;
        }
    }

    /// <summary>Lo que hay guardado en el registro, o null si no hay entrada.</summary>
    public string? ReadCommand()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(_keyPath);
        return key?.GetValue(ValueName) as string;
    }
}
