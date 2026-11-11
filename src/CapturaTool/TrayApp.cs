using CapturaTool.Capture;
using CapturaTool.Editor;
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

    // Selección en curso: null cuando no hay ninguna abierta
    private List<MonitorCapture>? _captures;
    private List<SelectionOverlay>? _overlays;

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
        if (_overlays != null)
        {
            return; // ya hay una selección abierta
        }

        // Primero congelar todos los monitores, después mostrar las ventanas (R6)
        _captures = ScreenCapture.CaptureAllMonitors();
        _overlays = new List<SelectionOverlay>();

        foreach (MonitorCapture capture in _captures)
        {
            var overlay = new SelectionOverlay(capture);
            overlay.SelectionCompleted += (sender, area) => OnSelectionCompleted(capture, area);
            overlay.Cancelled += (sender, e) => EndSelection();
            _overlays.Add(overlay);
            overlay.Show();
        }

        // El foco va al monitor donde está el mouse, para que Esc funcione de inmediato
        SelectionOverlay underMouse =
            _overlays.FirstOrDefault(o => o.Bounds.Contains(Cursor.Position)) ?? _overlays[0];
        underMouse.Activate();
    }

    private void OnSelectionCompleted(MonitorCapture capture, Rectangle area)
    {
        // Copiar la zona antes de liberar las capturas
        Bitmap image = capture.Crop(area);
        EndSelection();

        // Provisional: en T05 se abre el editor
        var preview = new PreviewForm(image);
        preview.Show();
        preview.Activate();
    }

    /// <summary>Cierra todos los overlays y libera las capturas.</summary>
    private void EndSelection()
    {
        if (_overlays == null || _captures == null)
        {
            return; // ya se cerró (cerrar un overlay vuelve a llamar aquí)
        }

        List<SelectionOverlay> overlays = _overlays;
        List<MonitorCapture> captures = _captures;
        _overlays = null;
        _captures = null;

        foreach (SelectionOverlay overlay in overlays)
        {
            overlay.Close(); // Close también libera la ventana
        }
        foreach (MonitorCapture capture in captures)
        {
            capture.Dispose();
        }
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
            EndSelection();
            _hotkey.Dispose();
            _trayIcon.Dispose();
            _menu.Dispose();
        }
        base.Dispose(disposing);
    }
}
