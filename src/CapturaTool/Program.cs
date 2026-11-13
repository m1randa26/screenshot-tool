using CapturaTool.Platform;

namespace CapturaTool;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Si el programa ya está abierto, esta copia termina sin hacer nada (R3)
        using var singleInstance = new SingleInstance(AppIdentity.Current.MutexName);
        if (!singleInstance.IsFirst)
        {
            return;
        }

        // Aplica la configuración del .csproj (entre otras cosas, el modo DPI PerMonitorV2)
        ApplicationConfiguration.Initialize();

        var logger = new Logger(Logger.DefaultFolder);
        var errorHandler = new ErrorHandler(logger);
        errorHandler.Install();

        try
        {
            using var trayApp = new TrayApp(logger);
            errorHandler.Notifier = trayApp.Notifier;

            logger.Write("Programa iniciado");
            Application.Run(trayApp);
            logger.Write("Programa cerrado");
        }
        catch (Exception ex)
        {
            // Error al arrancar: todavía no hay ícono en la bandeja, así que se avisa con un mensaje
            logger.Write("Error al arrancar", ex);
            MessageBox.Show(
                AppIdentity.Current.DisplayName + " no pudo iniciar. El detalle quedó guardado en:\n" + logger.CurrentFilePath,
                AppIdentity.Current.DisplayName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
