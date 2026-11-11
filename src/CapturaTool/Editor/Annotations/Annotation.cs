namespace CapturaTool.Editor.Annotations;

/// <summary>
/// Base de todas las anotaciones (rectángulo, círculo, flecha, resaltador, texto).
/// Una vez creada no se puede modificar (R20): sus datos solo se leen.
/// Las coordenadas de cada anotación son de la imagen, nunca de la pantalla.
/// </summary>
public abstract class Annotation
{
    protected Annotation(Color color)
    {
        Color = color;
    }

    public Color Color { get; }

    /// <summary>
    /// Dibuja la anotación en coordenadas de la imagen.
    /// Si se dibuja en pantalla, quien llama prepara la conversión con ViewTransform.
    /// </summary>
    public abstract void Draw(Graphics g);
}
