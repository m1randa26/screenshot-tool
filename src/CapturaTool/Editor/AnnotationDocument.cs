using CapturaTool.Editor.Annotations;

namespace CapturaTool.Editor;

/// <summary>
/// El modelo del editor: la captura original, las anotaciones y lo que se puede rehacer (R18–R20).
/// No conoce la interfaz. Es dueño de la imagen base: la libera en Dispose.
/// </summary>
public sealed class AnnotationDocument : IDisposable
{
    private readonly List<Annotation> _annotations = new();
    private readonly Stack<Annotation> _redoStack = new();

    public AnnotationDocument(Bitmap baseImage)
    {
        BaseImage = baseImage;
    }

    /// <summary>La captura original. Nunca se modifica.</summary>
    public Bitmap BaseImage { get; }

    /// <summary>Anotaciones en el orden en que se crearon.</summary>
    public IReadOnlyList<Annotation> Annotations => _annotations;

    public bool CanUndo => _annotations.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>Agrega una anotación. Lo que se había deshecho ya no se puede rehacer (R19).</summary>
    public void Add(Annotation annotation)
    {
        _annotations.Add(annotation);
        _redoStack.Clear();
    }

    /// <summary>Deshace la última anotación (R18). Devuelve false si no había nada que deshacer.</summary>
    public bool Undo()
    {
        if (!CanUndo)
        {
            return false;
        }

        Annotation last = _annotations[^1];
        _annotations.RemoveAt(_annotations.Count - 1);
        _redoStack.Push(last);
        return true;
    }

    /// <summary>Rehace la última anotación deshecha (R19). Devuelve false si no había nada que rehacer.</summary>
    public bool Redo()
    {
        if (!CanRedo)
        {
            return false;
        }

        _annotations.Add(_redoStack.Pop());
        return true;
    }

    public void Dispose()
    {
        BaseImage.Dispose();
    }
}
