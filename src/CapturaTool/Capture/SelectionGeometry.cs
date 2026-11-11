namespace CapturaTool.Capture;

/// <summary>
/// Cálculos de la selección, sin ventanas, para poder probarlos automáticamente.
/// Todas las coordenadas son del monitor (0,0 = esquina superior izquierda del monitor).
/// </summary>
public static class SelectionGeometry
{
    /// <summary>Una selección más angosta o más baja que esto cuenta como clic (R8).</summary>
    public const int MinSize = 5;

    /// <summary>
    /// Rectángulo entre el punto donde empezó el arrastre y donde está el mouse,
    /// sin importar en qué dirección se arrastró.
    /// </summary>
    public static Rectangle FromPoints(Point start, Point end)
    {
        return Rectangle.FromLTRB(
            Math.Min(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Max(start.X, end.X),
            Math.Max(start.Y, end.Y));
    }

    /// <summary>
    /// Zona final que se captura: la selección recortada a los bordes del monitor,
    /// o el monitor completo si la selección es diminuta (un clic).
    /// </summary>
    public static Rectangle Resolve(Rectangle selection, Size monitorSize)
    {
        var monitor = new Rectangle(Point.Empty, monitorSize);
        Rectangle clipped = Rectangle.Intersect(selection, monitor);

        if (clipped.Width < MinSize || clipped.Height < MinSize)
        {
            return monitor;
        }
        return clipped;
    }
}
