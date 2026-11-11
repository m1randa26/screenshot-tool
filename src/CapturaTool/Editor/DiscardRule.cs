namespace CapturaTool.Editor;

/// <summary>
/// Decide si hay que preguntar antes de cerrar el editor (R27, flujo F3).
/// </summary>
public static class DiscardRule
{
    /// <param name="hasAnnotations">El documento tiene anotaciones visibles.</param>
    /// <param name="hasUnconfirmedText">Se está escribiendo un texto que no está vacío.</param>
    /// <param name="outputDone">La imagen ya se copió (o guardó): no se pierde nada al cerrar.</param>
    /// <returns>true si hay que preguntar "¿Descartar la captura?".</returns>
    public static bool MustAsk(bool hasAnnotations, bool hasUnconfirmedText, bool outputDone)
    {
        if (outputDone)
        {
            return false;
        }
        return hasAnnotations || hasUnconfirmedText;
    }
}
