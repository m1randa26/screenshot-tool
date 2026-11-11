namespace CapturaTool.Platform;

/// <summary>
/// Impide abrir el programa dos veces (R3) usando un Mutex con nombre de Windows.
/// Debe liberarse desde el mismo hilo que lo creó.
/// </summary>
public sealed class SingleInstance : IDisposable
{
    private readonly Mutex _mutex;

    /// <summary>true si esta es la primera copia del programa en ejecución.</summary>
    public bool IsFirst { get; }

    public SingleInstance(string name)
    {
        _mutex = new Mutex(true, name, out bool createdNew);
        IsFirst = createdNew;
    }

    public void Dispose()
    {
        if (IsFirst)
        {
            _mutex.ReleaseMutex();
        }
        _mutex.Dispose();
    }
}
