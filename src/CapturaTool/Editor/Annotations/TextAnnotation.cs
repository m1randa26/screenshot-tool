using System.Drawing.Text;

namespace CapturaTool.Editor.Annotations;

/// <summary>
/// Texto de una sola línea, de tamaño fijo y legible en la imagen final (R14).
/// </summary>
public class TextAnnotation : Annotation
{
    /// <summary>Tamaño de la letra, en píxeles de la imagen final.</summary>
    public const float FontSize = 24f;

    public const string FontName = "Segoe UI";

    public TextAnnotation(PointF position, string text, Color color) : base(color)
    {
        Position = position;
        Text = text;
    }

    /// <summary>Esquina superior izquierda del texto, en coordenadas de la imagen.</summary>
    public PointF Position { get; }

    public string Text { get; }

    public override void Draw(Graphics g)
    {
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        using Font font = CreateFont();
        using StringFormat format = CreateFormat();
        using var brush = new SolidBrush(Color);
        g.DrawString(Text, font, brush, Position, format);
    }

    /// <summary>
    /// Ancho del texto y alto de una línea, en coordenadas de la imagen.
    /// Sirve para poner el cursor al final mientras se escribe.
    /// </summary>
    public SizeF Measure(Graphics g)
    {
        using Font font = CreateFont();
        using StringFormat format = CreateFormat();
        float width = Text.Length == 0 ? 0 : g.MeasureString(Text, font, PointF.Empty, format).Width;
        return new SizeF(width, font.GetHeight(g));
    }

    private static Font CreateFont() =>
        new(FontName, FontSize, FontStyle.Bold, GraphicsUnit.Pixel);

    /// <summary>
    /// Formato sin márgenes extra (para que el texto empiece justo donde se hizo clic)
    /// y que mide también los espacios del final (para que el cursor avance con ellos).
    /// </summary>
    private static StringFormat CreateFormat()
    {
        var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        return format;
    }
}
