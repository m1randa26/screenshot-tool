namespace CapturaTool.Platform;

/// <summary>
/// Atrapa los errores inesperados, los escribe en el log y avisa con una notificación,
/// para que el programa nunca muera en silencio (R32).
/// </summary>
public class ErrorHandler
{
    private readonly Logger _logger;

    public ErrorHandler(Logger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Se asigna cuando ya existe el ícono de la bandeja. Antes de eso, los errores solo van al log.
    /// </summary>
    public Notifier? Notifier { get; set; }

    /// <summary>
    /// Debe llamarse al arrancar, antes de crear cualquier ventana o control.
    /// </summary>
    public void Install()
    {
        // Errores en la interfaz: se atrapan y el programa sigue funcionando
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (sender, e) =>
            Report("Error inesperado en la interfaz", e.Exception);

        // Errores en tareas (Task) que nadie revisó: se marcan como atendidos y el programa sigue
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Report("Error inesperado en una tarea en segundo plano", e.Exception);
            e.SetObserved();
        };

        // Errores en otros hilos: .NET cierra el programa siempre; aquí solo queda registrar y avisar
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            Report("Error fatal: el programa se va a cerrar", e.ExceptionObject as Exception);
    }

    public void Report(string message, Exception? exception)
    {
        _logger.Write(message, exception);

        try
        {
            Notifier?.Error($"{AppIdentity.Current.DisplayName}: ocurrió un error", "El detalle quedó guardado en el log.");
        }
        catch
        {
            // Si ni siquiera se puede avisar, el error ya quedó en el log
        }
    }
}
