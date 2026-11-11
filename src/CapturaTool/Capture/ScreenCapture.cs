namespace CapturaTool.Capture;

/// <summary>
/// Toma una imagen de cada monitor en el instante del atajo ("congelar", R6).
/// Como el programa es PerMonitorV2, las medidas llegan en píxeles físicos
/// y la imagen sale en resolución nativa (NF4).
/// </summary>
public static class ScreenCapture
{
    /// <summary>
    /// Quien recibe la lista es dueño de las capturas y debe liberarlas.
    /// </summary>
    public static List<MonitorCapture> CaptureAllMonitors()
    {
        var captures = new List<MonitorCapture>();
        try
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                Rectangle bounds = screen.Bounds;
                var image = new Bitmap(bounds.Width, bounds.Height);
                using (var g = Graphics.FromImage(image))
                {
                    g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                }
                captures.Add(new MonitorCapture(bounds, image));
            }
        }
        catch
        {
            // Si falla a la mitad, liberar lo que ya se capturó antes de propagar el error
            foreach (MonitorCapture capture in captures)
            {
                capture.Dispose();
            }
            throw;
        }
        return captures;
    }
}
