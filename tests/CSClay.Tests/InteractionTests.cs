using CSClay;
using Xunit;

namespace CSClay.Tests;

public class InteractionTests
{
    [Fact]
    public void TestHoverState()
    {
        var arena = new ClayArena(1024 * 1024 * 4);
        
        // FRAME 1: Establish bounding boxes
        UI.Begin(arena, new Dimensions(800, 600));
        UI.Container("btn", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(50) } });
        UI.End();

        // FRAME 2: Test hover
        UI.SetPointerState(new Vector2(50, 25), false); // Inside "btn"
        UI.Begin(arena, new Dimensions(800, 600));
        
        bool isHovered = false;
        UI.Container("btn", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(50) } }, () => 
        {
            isHovered = UI.Hovered();
        });
        
        UI.End();

        Assert.True(isHovered);

        // Test outside
        UI.SetPointerState(new Vector2(200, 200), false); // Outside
        UI.Begin(arena, new Dimensions(800, 600));
        
        UI.Container("btn", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(50) } }, () => 
        {
            isHovered = UI.Hovered();
        });
        
        UI.End();

        Assert.False(isHovered);
    }
}
