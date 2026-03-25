using Clay;
using Xunit;

namespace Clay.Tests;

public class TextTests
{
    [Fact]
    public void TestTextSizing()
    {
        var arena = new ClayArena(1024 * 1024 * 4); // 4MB
        UI.Begin(arena, new Dimensions(800, 600));

        
        // Mock text measurement: 10 pixels per character
        var context = UI.GetCurrentContext();
        context.TextMeasure = (text, config) => new Dimensions(text.Length * 10, 20);

        UI.Container("root", new LayoutConfig 
        { 
            LayoutDirection = LayoutDirection.TopToBottom,
            Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fit() }
        }, new Color(100, 100, 100), () => 
        {
            UI.Text("Hello World", new TextConfig());
        });

        var commands = UI.End();

        // 0: "root" (Rectangle)
        // 1: "Hello World" (Text)
        Assert.Equal(2, commands.Length);
        Assert.Equal(RenderCommandType.Rectangle, commands[0].CommandType);
        Assert.Equal(RenderCommandType.Text, commands[1].CommandType);

        Assert.Equal(110, commands[1].BoundingBox.Width);
        Assert.Equal(20, commands[1].BoundingBox.Height);
    }

    [Fact]
    public void TestTextWrapping()
    {
        var arena = new ClayArena(1024 * 1024 * 4);
        UI.Begin(arena, new Dimensions(800, 600));
        
        var context = UI.GetCurrentContext();
        // 10 pixels per char. "Hello World" is 11 chars = 110px.
        context.TextMeasure = (text, config) => new Dimensions(text.Length * 10, 20);

        UI.Container("root", new LayoutConfig 
        { 
            Sizing = new Sizing { Width = SizingAxis.Fixed(60), Height = SizingAxis.Fit() }
        }, new Color(255, 255, 255), () => 
        {
            UI.Text("Hello World", new TextConfig { WrapMode = TextWrapMode.Words });
        });

        var commands = UI.End();

        // 0: "root" (Rectangle)
        // 1: "Hello " (Line 1)
        // 2: "World" (Line 2)
        
        Assert.Equal(3, commands.Length);
        Assert.Equal(40, commands[0].BoundingBox.Height); // "root" grew to 40
        Assert.Equal(20, commands[1].BoundingBox.Height); // Line 1
        Assert.Equal(20, commands[2].BoundingBox.Height); // Line 2
    }
}
