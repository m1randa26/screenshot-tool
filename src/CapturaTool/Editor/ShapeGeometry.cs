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
}
