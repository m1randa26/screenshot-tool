namespace CapturaTool.Output;

/// <summary>
/// Repite una acción hasta que funcione o se acaben los intentos (R22).
/// </summary>
public static class Retry
{
    /// <param name="attempt">Un intento; devuelve true si funcionó.</param>
    /// <param name="maxAttempts">Cuántas veces intentar como máximo.</param>
    /// <param name="wait">Espera entre un intento y el siguiente (no se llama después del último).</param>
    /// <param name="attemptsUsed">Cuántos intentos se hicieron.</param>
    /// <returns>true si algún intento funcionó.</returns>
    public static bool Run(Func<bool> attempt, int maxAttempts, Action wait, out int attemptsUsed)
    {
        for (int i = 1; i <= maxAttempts; i++)
        {
            attemptsUsed = i;
            if (attempt())
            {
                return true;
            }
            if (i < maxAttempts)
            {
                wait();
            }
        }

        attemptsUsed = maxAttempts;
        return false;
    }
}
