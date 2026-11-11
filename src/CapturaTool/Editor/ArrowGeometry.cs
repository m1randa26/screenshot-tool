namespace CapturaTool.Editor;

/// <summary>
/// Cálculos de la flecha (R16), en coordenadas de la imagen: la punta triangular
/// y hasta dónde llega la línea.
/// </summary>
public static class ArrowGeometry
{
    /// <summary>Largo de la punta, en píxeles de la imagen final.</summary>
    public const float HeadLength = 18f;

    /// <summary>Ancho de la punta en su base, en píxeles de la imagen final.</summary>
    public const float HeadWidth = 16f;

    /// <summary>
    /// Puntos de la flecha que va de <paramref name="start"/> a <paramref name="end"/>:
    /// Tip = vértice (siempre igual a end), Left y Right = esquinas de la base del triángulo,
    /// LineEnd = hasta dónde se dibuja la línea (un poco dentro del triángulo, para que
    /// no quede una rendija entre la línea y la punta, ni asome por delante).
    /// </summary>
    public static (PointF Tip, PointF Left, PointF Right, PointF LineEnd) Calculate(PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float length = MathF.Sqrt(dx * dx + dy * dy);

        if (length == 0)
        {
            return (end, end, end, end); // sin dirección: todo en un punto
        }

        // Dirección de la flecha, con largo 1
        float ux = dx / length;
        float uy = dy / length;

        // En una flecha corta, la punta se achica para no ser más larga que la flecha
        float headLength = Math.Min(HeadLength, length);
        float halfWidth = HeadWidth / 2 * (headLength / HeadLength);

        // Centro de la base del triángulo: retroceder desde la punta
        var baseCenter = new PointF(end.X - ux * headLength, end.Y - uy * headLength);

        // Perpendicular a la dirección, para abrir las dos esquinas de la base
        float px = -uy;
        float py = ux;
        var left = new PointF(baseCenter.X + px * halfWidth, baseCenter.Y + py * halfWidth);
        var right = new PointF(baseCenter.X - px * halfWidth, baseCenter.Y - py * halfWidth);

        // La línea entra un tercio en el triángulo (ahí el triángulo ya es más ancho que la línea)
        var lineEnd = new PointF(baseCenter.X + ux * headLength / 3, baseCenter.Y + uy * headLength / 3);

        return (end, left, right, lineEnd);
    }
}
