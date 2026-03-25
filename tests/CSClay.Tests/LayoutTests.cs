using CSClay;
using Xunit;

namespace CSClay.Tests;

public class LayoutTests
{
    [Fact]
    public void TestSimpleRowLayout()
    {
        var arena = new ClayArena(1024 * 1024 * 4); // 4MB
        UI.Begin(arena, new Dimensions(800, 600));

        UI.Container("root", new LayoutConfig
        {
            LayoutDirection = LayoutDirection.LeftToRight,
            Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fixed(600) }
        }, new Color(100, 100, 100), () =>
        {
            UI.Container("child1", new LayoutConfig
            {
                Sizing = new Sizing { Width = SizingAxis.Fixed(200), Height = SizingAxis.Grow() }
            }, new Color(200, 200, 200));
            UI.Container("child2", new LayoutConfig
            {
                Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() }
            }, new Color(255, 255, 255));
        });
        var commands = UI.End();

        // 0: "root"
        // 1: "child1"
        // 2: "child2"
        Assert.Equal(3, commands.Length);

        // Child 1 should be at (0, 0) with width 200
        Assert.Equal(0, commands[1].BoundingBox.X);
        Assert.Equal(200, commands[1].BoundingBox.Width);

        // Child 2 should be at (200, 0) and grow to fill the rest (800 - 200 = 600)
        Assert.Equal(200, commands[2].BoundingBox.X);
        Assert.Equal(600, commands[2].BoundingBox.Width);
    }
}
