using CapturaTool.Platform;

namespace CapturaTool;

/// <summary>
/// Núcleo del programa: ícono y menú de la bandeja (R2).
/// No tiene ventana principal; el programa vive mientras este contexto esté activo.
/// </summary>
public class TrayApp : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;

    public Notifier Notifier { get; }

    public TrayApp()
    {
        _menu = new ContextMenuStrip();
        _menu.Items.Add("Capturar", null, (sender, e) => Capture());

        var settingsItem = _menu.Items.Add("Configuración");
        settingsItem.Enabled = false; // Se activa en T16

#if DEBUG
        // Solo en desarrollo: sirve para comprobar R32 desde el checklist manual
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add("Provocar error (prueba)", null, (sender, e) =>
            throw new InvalidOperationException("Error de prueba provocado desde el menú"));
#endif

        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add("Salir", null, (sender, e) => Exit());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application, // Ícono provisional
            Text = "CapturaTool",
            ContextMenuStrip = _menu,
            Visible = true,
        };

        Notifier = new Notifier(_trayIcon);
    }

    private void Capture()
    {
        // Provisional: en T03 esto inicia la captura
        Notifier.Info("CapturaTool", "Capturar todavía no está disponible.");
    }

    private void Exit()
    {
        // Ocultar antes de salir para que el ícono no quede "fantasma" en la bandeja
        _trayIcon.Visible = false;
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon.Dispose();
            _menu.Dispose();
        }
        base.Dispose(disposing);
    }
}
