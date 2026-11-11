using System.Text;

namespace CapturaTool.Editor;

/// <summary>
/// El texto que se está escribiendo con la herramienta Texto (R14).
/// Edición simple: agregar letras al final y borrar la última. Una sola línea.
/// </summary>
public class TextInput
{
    private readonly StringBuilder _text = new();

    public string Text => _text.ToString();

    public bool IsEmpty => _text.Length == 0;

    /// <summary>Agrega una letra. Las teclas de control (Enter, Tab, Esc…) se ignoran.</summary>
    public void Type(char c)
    {
        if (char.IsControl(c))
        {
            return;
        }
        _text.Append(c);
    }

    /// <summary>Borra la última letra. Sin texto, no hace nada.</summary>
    public void Backspace()
    {
        if (_text.Length == 0)
        {
            return;
        }

        // Algunos símbolos (como los emojis) ocupan dos posiciones: se borran juntas
        int count = _text.Length >= 2 && char.IsLowSurrogate(_text[^1]) ? 2 : 1;
        _text.Length -= count;
    }
}
