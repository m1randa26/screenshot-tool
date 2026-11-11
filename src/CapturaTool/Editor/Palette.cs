namespace CapturaTool.Editor;

/// <summary>
/// Colores disponibles para las anotaciones (R13). El primero es el inicial.
/// </summary>
public static class Palette
{
    public static readonly (string Name, Color Color)[] Colors =
    {
        ("Rojo", Color.FromArgb(230, 30, 30)),
        ("Naranja", Color.FromArgb(255, 140, 0)),
        ("Amarillo", Color.FromArgb(255, 215, 0)),
        ("Verde", Color.FromArgb(30, 170, 60)),
        ("Azul", Color.FromArgb(30, 110, 230)),
        ("Morado", Color.FromArgb(150, 60, 200)),
        ("Negro", Color.Black),
        ("Blanco", Color.White),
    };

    /// <summary>Color con el que abre el editor: rojo.</summary>
    public static Color Default => Colors[0].Color;
}
