using System.Drawing.Drawing2D;

namespace CapturaTool.Editor.Annotations;

/// <summary>Rectángulo: solo contorno, sin relleno, grosor fijo (R15).</summary>
public class RectangleAnnotation : Annotation
{
    public RectangleAnnotation(RectangleF bounds, Color color) : base(color)
    {
        Bounds = bounds;
    }

    /// <summary>Posición y tamaño, en coordenadas de la imagen.</summary>
    public RectangleF Bounds { get; }

    public override void Draw(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color, StrokeWidth);
        g.DrawRectangle(pen, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
    }
}
