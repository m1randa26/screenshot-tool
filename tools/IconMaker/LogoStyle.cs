namespace IconMaker;

/// <summary>Colores del logo: el de la estable y los de la Insider (R33, R34).</summary>
public sealed record LogoStyle(Color Background, Color? Band)
{
    private static readonly Color Blue = Color.FromArgb(30, 110, 230);     // azul de la paleta del editor
    private static readonly Color Purple = Color.FromArgb(150, 60, 200);   // morado de la paleta del editor
    private static readonly Color Orange = Color.FromArgb(255, 140, 0);    // naranja de la paleta del editor

    public static readonly LogoStyle Stable = new(Blue, null);

    // Candidatos para la Insider (T22): fondo morado, o azul con una franja naranja abajo
    public static readonly LogoStyle InsiderPurple = new(Purple, null);
    public static readonly LogoStyle InsiderBand = new(Blue, Orange);

    /// <summary>El elegido por el usuario para la Insider.</summary>
    public static readonly LogoStyle Insider = InsiderPurple; // elegido por el usuario en T22
}
