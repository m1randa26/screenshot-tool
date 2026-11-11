using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CapturaTool.Editor.Annotations;

/// <summary>
/// Resaltador de trazo libre, semitransparente (R17).
///
/// Para que el tono sea uniforme (sin manchas más oscuras donde el trazo pasa dos veces),
/// el trazo se dibuja primero OPACO en una capa aparte y luego esa capa se pega con
/// transparencia de una sola vez.
/// </summary>
public class HighlighterAnnotation : Annotation
{
    /// <summary>Grosor del trazo, en píxeles de la imagen final: cubre una línea de texto.</summary>
    public const float Width = 20f;

    /// <summary>Opacidad del trazo: 0 = invisible, 1 = tapa todo.</summary>
    public const float Opacity = 0.45f;

    private readonly PointF[] _points;

    public HighlighterAnnotation(IEnumerable<PointF> points, Color color) : base(color)
    {
        // Copia propia: la anotación no cambia aunque cambie la lista original (R20)
        _points = points.ToArray();
        Points = Array.AsReadOnly(_points);
    }

    /// <summary>Puntos del trazo en orden, en coordenadas de la imagen. Solo lectura.</summary>
    public ReadOnlyCollection<PointF> Points { get; }

    public override void Draw(Graphics g)
    {
        if (_points.Length == 0)
        {
            return;
        }

        // La capa se arma en los píxeles del destino (la pantalla, o la imagen final a 1:1),
        // no en los de la imagen: con una captura grande mostrada reducida, la capa es
        // varias veces más pequeña y dibujar no se traba.
        // Para eso se aplican a los puntos la misma conversión que ya trae g (ViewTransform).
        var target = (PointF[])_points.Clone();
        float scale;
        using (Matrix transform = g.Transform)
        {
            transform.TransformPoints(target);
            scale = transform.Elements[0]; // zoom: la conversión usa la misma escala en X y en Y
        }
        float width = Width * scale;

        // Zona que ocupa el trazo (con margen por el grosor), en píxeles enteros del destino
        RectangleF bounds = ShapeGeometry.BoundsOf(target);
        float margin = width / 2 + 2;
        Rectangle area = Rectangle.FromLTRB(
            (int)Math.Floor(bounds.Left - margin),
            (int)Math.Floor(bounds.Top - margin),
            (int)Math.Ceiling(bounds.Right + margin),
            (int)Math.Ceiling(bounds.Bottom + margin));

        // 1. Trazo opaco en una capa transparente del tamaño de esa zona
        using var layer = new Bitmap(area.Width, area.Height);
        using (Graphics layerGraphics = Graphics.FromImage(layer))
        {
            layerGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            layerGraphics.TranslateTransform(-area.X, -area.Y);
            using var pen = new Pen(Color.FromArgb(255, Color), width)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round,
            };

            if (target.Length == 1)
            {
                using var brush = new SolidBrush(pen.Color);
                layerGraphics.FillEllipse(brush, target[0].X - width / 2, target[0].Y - width / 2, width, width);
            }
            else
            {
                layerGraphics.DrawLines(pen, target);
            }
        }

        // 2. La capa se pega 1:1 (ya está en píxeles del destino), con la misma transparencia en todos sus puntos
        var colorMatrix = new ColorMatrix { Matrix33 = Opacity };
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(colorMatrix);

        GraphicsState state = g.Save(); // se restaura la conversión de quien llama al terminar
        g.ResetTransform();
        g.PixelOffsetMode = PixelOffsetMode.Half; // copia exacta, píxel por píxel
        g.DrawImage(layer, area, 0, 0, area.Width, area.Height, GraphicsUnit.Pixel, attributes);
        g.Restore(state);
    }
}
