using System.Diagnostics;
using CapturaTool.Capture;
using CapturaTool.Editor;
using CapturaTool.Output;
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
    private readonly ClipboardWriter _clipboard;

    // Una ventana de selección por monitor, creada una vez y reutilizada
    private readonly List<SelectionOverlay> _overlays = new();

    // Capturas de la selección en curso: null cuando no hay ninguna abierta
    private List<MonitorCapture>? _captures;

    // Editor abierto: null cuando no hay ninguno (R10)
    private EditorForm? _editor;

    public Notifier Notifier { get; }

    public TrayApp(Logger logger)
    {
        _logger = logger;
        _clipboard = new ClipboardWriter(logger);

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

        // Crear las ventanas de selección ahora, para que la primera captura sea rápida (NF1)
        EnsureOverlays(Screen.AllScreens.Select(s => s.Bounds).ToList());
    }

    private void Capture()
    {
        if (_editor != null)
        {
            // R10: con un editor abierto no se captura de nuevo; el editor pasa al frente
            _editor.ShowAgain();
            return;
        }

        if (_captures != null)
        {
            return; // ya hay una selección abierta
        }

        // NF1: tiempo desde el atajo hasta que la selección se ve
        var timer = Stopwatch.StartNew();

        // Primero congelar todos los monitores, después mostrar las ventanas (R6)
        _captures = ScreenCapture.CaptureAllMonitors();

        // Normalmente las ventanas ya existen; solo se recrean si cambiaron los monitores
        EnsureOverlays(_captures.Select(c => c.Bounds).ToList());

        for (int i = 0; i < _overlays.Count; i++)
        {
            _overlays[i].Begin(_captures[i]);
        }

        // El foco va al monitor donde está el mouse, para que Esc funcione de inmediato
        SelectionOverlay underMouse =
            _overlays.FirstOrDefault(o => o.MonitorBounds.Contains(Cursor.Position)) ?? _overlays[0];
        underMouse.Activate();

        // Dibujar ya (lo haría igual un instante después) para medir cuándo se ve de verdad
        foreach (SelectionOverlay overlay in _overlays)
        {
            overlay.Update();
        }
        _logger.Write($"Medición: selección visible en {timer.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Deja lista una ventana de selección por monitor. Crear ventanas es lento (NF1),
    /// así que se crean una vez y se reutilizan; solo se recrean si cambian los monitores.
    /// </summary>
    private void EnsureOverlays(List<Rectangle> monitors)
    {
        bool same = _overlays.Count == monitors.Count
            && _overlays.Select(o => o.MonitorBounds).SequenceEqual(monitors);
        if (same)
        {
            return;
        }

        foreach (SelectionOverlay old in _overlays)
        {
            old.Dispose();
        }
        _overlays.Clear();

        foreach (Rectangle monitor in monitors)
        {
            var overlay = new SelectionOverlay(monitor);
            overlay.SelectionCompleted += (sender, area) => OnSelectionCompleted(overlay, area);
            overlay.Cancelled += (sender, e) => EndSelection();
            _ = overlay.Handle; // crea la ventana de Windows ya (oculta), para no pagarlo al capturar
            _overlays.Add(overlay);
        }
    }

    private void OnSelectionCompleted(SelectionOverlay overlay, Rectangle area)
    {
        if (_captures == null)
        {
            return;
        }

        // NF2: tiempo desde que se suelta el mouse hasta que el editor se ve
        var timer = Stopwatch.StartNew();

        // Copiar la zona antes de liberar las capturas
        MonitorCapture capture = _captures[_overlays.IndexOf(overlay)];
        Bitmap image = capture.Crop(area);
        EndSelection();

        var editor = new EditorForm(image, _clipboard, Notifier); // el editor pasa a ser dueño de la imagen
        _editor = editor;
        editor.FormClosed += (sender, e) => _editor = null;
        editor.Show();
        editor.Activate();

        editor.RenderNow();
        _logger.Write($"Medición: editor visible en {timer.ElapsedMilliseconds} ms");
    }

    /// <summary>Oculta los overlays (quedan listos para la próxima vez) y libera las capturas.</summary>
    private void EndSelection()
    {
        if (_captures == null)
        {
            return; // ya se terminó
        }

        List<MonitorCapture> captures = _captures;
        _captures = null;

        foreach (SelectionOverlay overlay in _overlays)
        {
            overlay.End();
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
            foreach (SelectionOverlay overlay in _overlays)
            {
                overlay.Dispose();
            }
            _hotkey.Dispose();
            _trayIcon.Dispose();
            _menu.Dispose();
        }
        base.Dispose(disposing);
    }
}
