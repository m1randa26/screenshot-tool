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
    private readonly GlobalHotkey _hotkey;
    private readonly Logger _logger;

    public Notifier Notifier { get; }

    public TrayApp(Logger logger)
    {
        _logger = logger;

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

        // El atajo y el menú Capturar hacen lo mismo (R4)
        _hotkey = new GlobalHotkey();
        _hotkey.Pressed += (sender, e) => Capture();
        if (!_hotkey.Register(KeyModifiers.Control | KeyModifiers.Alt, Keys.S))
        {
            // R5: avisar y dejar el menú Capturar como alternativa
            _logger.Write("No se pudo registrar el atajo Ctrl + Alt + S (otro programa lo usa)");
            Notifier.Error(
                "CapturaTool: atajo no disponible",
                "No se pudo activar Ctrl + Alt + S porque otro programa lo usa. Puedes capturar desde el menú del ícono.");
        }
    }

    private void Capture()
    {
        // Provisional: en T04 esto inicia la captura de verdad
        Notifier.Info("CapturaTool", "Captura iniciada (prueba).");
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
            _hotkey.Dispose();
            _trayIcon.Dispose();
            _menu.Dispose();
        }
        base.Dispose(disposing);
    }
}
