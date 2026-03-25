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

        // 0: ScissorStart ("scroll")
        // 1: "scroll" (Rectangle)
        // 2: "content" (Rectangle)
        // 3: ScissorEnd ("scroll")
        
        Assert.Equal(4, commands.Length);
        Assert.Equal(RenderCommandType.ScissorStart, commands[0].CommandType);
        Assert.Equal(RenderCommandType.Rectangle, commands[1].CommandType);
        Assert.Equal(RenderCommandType.Rectangle, commands[2].CommandType);
        Assert.Equal(RenderCommandType.ScissorEnd, commands[3].CommandType);

        // "content" should be offset by scroll offset (0, -20)
        // Since "scroll" is at (0, 0), "content" is at (0, -20)
        Assert.Equal(-20, commands[2].BoundingBox.Y);
    }
}
