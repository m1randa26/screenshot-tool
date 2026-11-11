using System.Drawing.Drawing2D;

namespace CapturaTool.Editor.Annotations;

/// <summary>Círculo u óvalo: solo contorno, sin relleno, grosor fijo (R15).</summary>
public class EllipseAnnotation : Annotation
{
    public EllipseAnnotation(RectangleF bounds, Color color) : base(color)
    {
        Bounds = bounds;
    }

    /// <summary>Rectángulo que contiene al óvalo, en coordenadas de la imagen.</summary>
    public RectangleF Bounds { get; }

    public override void Draw(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color, StrokeWidth);
        g.DrawEllipse(pen, Bounds);
    }
}
