using CapturaTool.Platform;

namespace CapturaTool.Tests;

public class SingleInstanceTests
{
    // Nombre único para no chocar con el programa real ni con otras pruebas
    private readonly string _name = "Local\\CapturaToolTests_" + Guid.NewGuid();

    [Fact]
    public void FirstInstance_IsFirst()
    {
        using var first = new SingleInstance(_name);

        Assert.True(first.IsFirst);
    }

    [Fact]
    public void SecondInstance_IsNotFirst()
    {
        using var first = new SingleInstance(_name);
        using var second = new SingleInstance(_name);

        Assert.False(second.IsFirst);
    }

    [Fact]
    public void AfterFirstIsDisposed_NewInstanceIsFirst()
    {
        var first = new SingleInstance(_name);
        first.Dispose();

        using var again = new SingleInstance(_name);

        Assert.True(again.IsFirst);
    }
}
