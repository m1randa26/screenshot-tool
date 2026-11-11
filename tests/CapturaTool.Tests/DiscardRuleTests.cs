using CapturaTool.Editor;

namespace CapturaTool.Tests;

public class DiscardRuleTests
{
    [Theory]
    // anotaciones, texto sin confirmar, ya se copió → ¿preguntar?
    [InlineData(false, false, false, false)] // captura sin tocar: cierra sin preguntar
    [InlineData(true, false, false, true)]   // con anotaciones: pregunta
    [InlineData(false, true, false, true)]   // solo un texto a medio escribir: pregunta
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, false)]   // ya se copió: no se pierde nada, no pregunta
    [InlineData(false, true, true, false)]
    [InlineData(false, false, true, false)]
    public void MustAsk_FollowsTheRule(bool hasAnnotations, bool hasUnconfirmedText, bool outputDone, bool expected)
    {
        Assert.Equal(expected, DiscardRule.MustAsk(hasAnnotations, hasUnconfirmedText, outputDone));
    }
}
