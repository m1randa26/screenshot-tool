namespace CapturaTool.Editor;

/// <summary>
/// Cálculos de las figuras que se dibujan arrastrando, en coordenadas de la imagen.
/// </summary>
public static class ShapeGeometry
{
    /// <summary>
    /// Una figura más pequeña que esto en ancho Y en alto no se crea
    /// (fue un clic sin arrastrar). Píxeles de imagen.
    /// </summary>
    public const float MinSize = 3f;

    /// <summary>Rectángulo entre dos puntos, sin importar en qué dirección se arrastró.</summary>
    public static RectangleF FromPoints(PointF start, PointF end)
    {
        return RectangleF.FromLTRB(
            Math.Min(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Max(start.X, end.X),
            Math.Max(start.Y, end.Y));
    }

    /// <summary>
    /// true si la figura es diminuta en las dos direcciones.
    /// Una figura delgada pero larga sí cuenta.
    /// </summary>
    public static bool IsTooSmall(RectangleF shape)
    {
        return shape.Width < MinSize && shape.Height < MinSize;
    }

    /// <summary>Rectángulo más pequeño que contiene todos los puntos (por ejemplo, un trazo libre).</summary>
    public static RectangleF BoundsOf(IReadOnlyList<PointF> points)
    {
        if (points.Count == 0)
        {
            return RectangleF.Empty;
        }

        float left = points.Min(p => p.X);
        float top = points.Min(p => p.Y);
        float right = points.Max(p => p.X);
        float bottom = points.Max(p => p.Y);
        return RectangleF.FromLTRB(left, top, right, bottom);
    }
}
