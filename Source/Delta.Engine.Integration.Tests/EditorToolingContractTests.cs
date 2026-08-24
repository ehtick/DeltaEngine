using Delta.Engine.Integration;
using Xunit;

namespace Delta.Engine.Integration.Tests;

public sealed class EditorToolingContractTests
{
    [Fact]
    public void PhysicalKeyTextAndImeAreDistinctNeutralBoundaries()
    {
        var key = new EngineUiInputPacket(EngineUiInputKind.KeyDown, Code: 65);
        var text = new EngineUiInputPacket(EngineUiInputKind.TextInput, Text: "ä");

        Assert.Equal(EngineUiInputKind.KeyDown, key.Kind);
        Assert.Equal(65, key.Code);
        Assert.Null(key.Text);
        Assert.Equal(EngineUiInputKind.TextInput, text.Kind);
        Assert.Equal("ä", text.Text);
        Assert.True(typeof(IEngineImeCompositionSink).GetMethod(nameof(IEngineImeCompositionSink.UpdateComposition)) is not null);
    }
}
