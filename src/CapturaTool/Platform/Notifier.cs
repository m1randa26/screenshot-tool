namespace CapturaTool.Platform;

/// <summary>
/// Muestra avisos breves de Windows usando el ícono de la bandeja.
/// </summary>
public class Notifier
{
    private const int DurationMs = 3000;

    private readonly NotifyIcon _trayIcon;

    public Notifier(NotifyIcon trayIcon)
    {
        _trayIcon = trayIcon;
    }

    // Sin ícono propio: Windows 11 muestra el logo en el encabezado del aviso (R33).
    // Con ToolTipIcon.Info ponía una "i" genérica grande en el cuerpo.
    public void Info(string title, string text) =>
        _trayIcon.ShowBalloonTip(DurationMs, title, text, ToolTipIcon.None);

    // Los errores mantienen la X roja, para que se distingan de un vistazo

    public void Error(string title, string text) =>
        _trayIcon.ShowBalloonTip(DurationMs, title, text, ToolTipIcon.Error);
}
