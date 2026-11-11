using System.Drawing.Drawing2D;

namespace CapturaTool.Editor.Annotations;

/// <summary>
/// Flecha del punto inicial a la punta, con grosor fijo (R16).
/// La punta queda donde se soltó el mouse.
/// </summary>
public class ArrowAnnotation : Annotation
{
    public ArrowAnnotation(PointF start, PointF end, Color color) : base(color)
    {
        Start = start;
        End = end;
    }

    /// <summary>Donde empezó el arrastre, en coordenadas de la imagen.</summary>
    public PointF Start { get; }

    /// <summary>La punta: donde se soltó el mouse, en coordenadas de la imagen.</summary>
    public PointF End { get; }

    public override void Draw(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var arrow = ArrowGeometry.Calculate(Start, End);

        using var pen = new Pen(Color, StrokeWidth);
        g.DrawLine(pen, Start, arrow.LineEnd);

        using var brush = new SolidBrush(Color);
        g.FillPolygon(brush, new[] { arrow.Tip, arrow.Left, arrow.Right });
    }
}
