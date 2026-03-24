using Clay;
using Xunit;

namespace Clay.Tests;

public class ScissorTests
{
    [Fact]
    public void TestScissorAndScroll()
    {
        var arena = new ClayArena(1024 * 1024 * 4);
        UI.Begin(arena, new Dimensions(800, 600));

        UI.ScrollContainer("scroll", 
            new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(100) } },
            new ClipConfig { ChildOffset = new Vector2(0, -20) }, // Scrolled down 20px
            new Color(100, 100, 100),
            () => 
            {
                UI.Container("content", new LayoutConfig 
                { 
                    Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(200) }
                }, new Color(200, 200, 200));
            });

        var commands = UI.End();

        // 0: RootContainer
        // 1: ScissorStart ("scroll")
        // 2: "scroll" (Rectangle)
        // 3: "content" (Rectangle)
        // 4: ScissorEnd ("scroll")
        
        Assert.Equal(5, commands.Length);
        Assert.Equal(RenderCommandType.ScissorStart, commands[1].CommandType);
        Assert.Equal(RenderCommandType.Rectangle, commands[2].CommandType);
        Assert.Equal(RenderCommandType.Rectangle, commands[3].CommandType);
        Assert.Equal(RenderCommandType.ScissorEnd, commands[4].CommandType);

        // "content" should be offset by scroll offset (0, -20)
        // Since "scroll" is at (0, 0), "content" is at (0, -20)
        Assert.Equal(-20, commands[3].BoundingBox.Y);
    }
}
