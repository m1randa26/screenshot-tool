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

    public void Info(string title, string text) =>
        _trayIcon.ShowBalloonTip(DurationMs, title, text, ToolTipIcon.Info);

    public void Error(string title, string text) =>
        _trayIcon.ShowBalloonTip(DurationMs, title, text, ToolTipIcon.Error);
}
