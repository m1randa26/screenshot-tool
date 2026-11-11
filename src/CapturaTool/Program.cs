namespace CapturaTool;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Aplica la configuración del .csproj (entre otras cosas, el modo DPI PerMonitorV2)
        ApplicationConfiguration.Initialize();

        // Ventana provisional para comprobar que la app abre y cierra; en T02 se reemplaza por la bandeja
        using var window = new Form { Text = "CapturaTool" };
        Application.Run(window);
    }
}
