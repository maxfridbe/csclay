using Clay;
using Xunit;

namespace Clay.Tests;

public class ImageTests
{
    [Fact]
    public void TestImageSizing()
    {
        var arena = new ClayArena(1024 * 1024 * 4);
        UI.Begin(arena, new Dimensions(800, 600));

        UI.Container("root", new LayoutConfig 
        { 
            LayoutDirection = LayoutDirection.TopToBottom,
            Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fit() }
        }, new Color(0,0,0), () => 
        {
            UI.Image("logo", new ImageConfig { Dimensions = new Dimensions(100, 50) });
        });

        var commands = UI.End();

        // 0: "root"
        // 1: "logo" (Image)
        Assert.Equal(2, commands.Length);
        Assert.Equal(RenderCommandType.Image, commands[1].CommandType);
        Assert.Equal(100, commands[1].BoundingBox.Width);
        Assert.Equal(50, commands[1].BoundingBox.Height);

        // Root container height should be 50 (Fit)
        Assert.Equal(50, commands[0].BoundingBox.Height);
    }
}
