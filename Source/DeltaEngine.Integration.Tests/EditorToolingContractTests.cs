using DeltaEngine.Integration;
using DeltaXAML.Contract;
using Xunit;

namespace DeltaEngine.Integration.Tests;

public sealed class EditorToolingContractTests
{
    [Fact]
    public void PhysicalKeyTextAndImeAreDistinctNeutralBoundaries()
    {
        var key = UiInputEvent.FromKey(new UiKeyEvent(
            UiKeyEventKind.Down,
            new UiPhysicalKey(65),
            new UiLogicalKey(65),
            default,
            false));
        var text = UiInputEvent.FromText(new UiTextInput("ä".AsMemory()));

        Assert.Equal(UiInputEventKind.Key, key.Kind);
        Assert.Equal(UiKeyEventKind.Down, key.Key.Kind);
        Assert.Equal((uint)65, key.Key.PhysicalKey.Value);
        Assert.Equal(UiInputEventKind.Text, text.Kind);
        Assert.Equal("ä", text.Text.Text.ToString());
        Assert.True(typeof(IEngineImeCompositionSink).GetMethod(nameof(IEngineImeCompositionSink.UpdateComposition)) is not null);
    }
}
