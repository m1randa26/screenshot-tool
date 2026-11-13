using System.Drawing.Drawing2D;

namespace IconMaker;

/// <summary>
/// Dibuja el logo (diseño B: esquinas de selección y una flecha) a un tamaño dado.
/// Las medidas están pensadas en una cuadrícula de 32 × 32 y se escalan a cada tamaño.
/// Las esquinas se redondean a píxeles enteros para que se vean nítidas a 16 px.
/// </summary>
public static class LogoPainter
{
    private static readonly Color Yellow = Color.FromArgb(255, 210, 30);  // flecha

    // Medidas en la cuadrícula de 32 × 32
    private const float CornerMargin = 5.7f;     // del borde del ícono al borde de afuera de cada esquina
    private const float CornerArm = 6.4f;        // largo de cada brazo (corto, para dejar aire a la flecha a 16 px)
    private const float CornerThickness = 2.6f;  // grosor de las esquinas
    private const float BackgroundRadius = 6f;   // redondeo del fondo

    /// <summary>Dibuja el logo de la versión estable.</summary>
    public static Bitmap Paint(int size) => Paint(size, LogoStyle.Stable);

    /// <summary>Dibuja el logo con un estilo (estable o Insider, R34).</summary>
    public static Bitmap Paint(int size, LogoStyle style)
    {
        var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(Color.Transparent);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half; // así un rectángulo con números enteros cae justo en los píxeles

        float scale = size / 32f;

        DrawBackground(g, size, scale, style);
        DrawCorners(g, size, scale);
        DrawArrow(g, size, scale);

        return bitmap;
    }

    private static void DrawBackground(Graphics g, int size, float scale, LogoStyle style)
    {
        int margin = (int)Math.Floor(scale); // 0 px a 16, 1 px a 32, 8 px a 256
        float radius = BackgroundRadius * scale;
        var area = new RectangleF(margin, margin, size - 2 * margin, size - 2 * margin);

        using GraphicsPath path = RoundedRectangle(area, radius);
        using var brush = new SolidBrush(style.Background);
        g.FillPath(brush, path);

        if (style.Band is Color bandColor)
        {
            // Franja abajo, recortada con la forma redondeada del fondo; nunca menos de 2 px
            int bandHeight = Math.Max(2, (int)Math.Round(3f * scale, MidpointRounding.AwayFromZero));
            Region oldClip = g.Clip;
            g.SetClip(path);
            using var band = new SolidBrush(bandColor);
            g.FillRectangle(band, 0, size - margin - bandHeight, size, bandHeight + margin);
            g.Clip = oldClip;
            oldClip.Dispose();
        }
    }

    private static void DrawCorners(Graphics g, int size, float scale)
    {
        int margin = Snap(CornerMargin * scale);
        int arm = Snap(CornerArm * scale);
        // A 16 px, 1 px de grosor deja lugar a la flecha; desde 20 px, nunca menos de 2 px
        int thickness = size <= 16 ? 1 : Math.Max(2, Snap(CornerThickness * scale));
        int far = size - margin; // borde de afuera de las esquinas derecha e inferior

        // Cada esquina son dos barras: una horizontal y una vertical
        using var white = new SolidBrush(Color.White);
        g.SmoothingMode = SmoothingMode.None; // barras rectas: sin suavizar, píxel exacto

        // Arriba a la izquierda
        g.FillRectangle(white, margin, margin, arm, thickness);
        g.FillRectangle(white, margin, margin, thickness, arm);
        // Arriba a la derecha
        g.FillRectangle(white, far - arm, margin, arm, thickness);
        g.FillRectangle(white, far - thickness, margin, thickness, arm);
        // Abajo a la izquierda
        g.FillRectangle(white, margin, far - thickness, arm, thickness);
        g.FillRectangle(white, margin, far - arm, thickness, arm);
        // Abajo a la derecha
        g.FillRectangle(white, far - arm, far - thickness, arm, thickness);
        g.FillRectangle(white, far - thickness, far - arm, thickness, arm);

        g.SmoothingMode = SmoothingMode.AntiAlias;
    }

    private static void DrawArrow(Graphics g, int size, float scale)
    {
        // Línea en diagonal hacia arriba a la derecha, con la punta en un triángulo
        float width = Math.Max(2f, 3f * scale);
        using var pen = new Pen(Yellow, width) { StartCap = LineCap.Round, EndCap = LineCap.Flat };
        g.DrawLine(pen, 11f * scale, 21f * scale, 18.5f * scale, 13.5f * scale);

        PointF[] head =
        {
            new(14.5f * scale, 11f * scale),
            new(21f * scale, 11f * scale),
            new(21f * scale, 17.5f * scale),
        };
        using var brush = new SolidBrush(Yellow);
        g.FillPolygon(brush, head);
    }

    private static int Snap(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

    private static GraphicsPath RoundedRectangle(RectangleF area, float radius)
    {
        float diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(area.Left, area.Top, diameter, diameter, 180, 90);
        path.AddArc(area.Right - diameter, area.Top, diameter, diameter, 270, 90);
        path.AddArc(area.Right - diameter, area.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(area.Left, area.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
