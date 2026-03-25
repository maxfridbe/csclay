using CSClay;
using CSClay.Fluent;
using Xunit;
using static CSClay.Fluent.Clay;

namespace CSClay.Fluent.Tests;

public class FluentTests
{
    [Fact]
    public void TestFluentContainer()
    {
        var arena = new ClayArena(1024 * 1024 * 10);
        var context = new ClayContext(arena);
        UI.SetCurrentContext(context);
        context.TextMeasure = (text, config) => new Dimensions(text.Length * 10, 20);

        UI.Begin(arena, new Dimensions(800, 600));

        Clay.Container("root", c => c
            .Sizing(Fixed(100), Grow())
            .Padding(20, 10)
            .Direction(LayoutDirection.LeftToRight)
            .Align(LayoutAlignmentX.Center, LayoutAlignmentY.Center)
            .Color(255, 0, 0)
        , () => {
            Clay.Text("Hello", t => t.Size(20).Color(255, 255, 255));
        });

        var commands = UI.End();

        Assert.Equal(2, commands.Length);
        
        var root = commands[0];
        Assert.Equal(RenderCommandType.Rectangle, root.CommandType);
        Assert.Equal(255, root.RenderData.Rectangle.Color.R);

        var text = commands[1];
        Assert.Equal(RenderCommandType.Text, text.CommandType);
        Assert.Equal(50, text.BoundingBox.Width);
        Assert.Equal(20, text.RenderData.Text.FontSize);
    }
}
