using Clay;
using Xunit;

namespace Clay.Tests;

public class FloatingTests
{
    [Fact]
    public void TestFloatingPosition()
    {
        var arena = new ClayArena(1024 * 1024 * 4);
        UI.Begin(arena, new Dimensions(800, 600));

        UI.Container("root", new LayoutConfig 
        { 
            Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fixed(600) },
            Padding = new Padding(10, 10)
        }, new Color(100, 100, 100), () => 
        {
            UI.Container("parent", new LayoutConfig 
            { 
                Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(100) }
            }, new Color(150, 150, 150), () => 
            {
                UI.FloatingContainer("tooltip", 
                    new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(50), Height = SizingAxis.Fixed(20) } },
                    new FloatingConfig 
                    { 
                        AttachTo = FloatingAttachToElement.Parent,
                        AttachPoints = new FloatingAttachPoints 
                        { 
                            Parent = FloatingAttachPoint.RightCenter, 
                            Element = FloatingAttachPoint.LeftCenter 
                        },
                        Offset = new Vector2(5, 0)
                    }, new Color(200, 200, 200));
            });
        });

        var commands = UI.End();

        // 0: RootContainer
        // 1: "root"
        // 2: "parent"
        // 3: "tooltip" (floating)
        Assert.Equal(4, commands.Length);

        // "parent" should be at (10, 10) - because of root padding
        Assert.Equal(10, commands[2].BoundingBox.X);
        Assert.Equal(10, commands[2].BoundingBox.Y);

        // "tooltip" attaches its LeftCenter to parent's RightCenter
        // parent RightCenter = (10 + 100, 10 + 50) = (110, 60)
        // tooltip LeftCenter = (BB.X, BB.Y + 10)
        // target BB.X = 110 + 5 (offset) = 115
        // target BB.Y = 60 - 10 = 50
        
        Assert.Equal(115, commands[3].BoundingBox.X);
        Assert.Equal(50, commands[3].BoundingBox.Y);
    }

    [Fact]
    public void TestZIndexSorting()
    {
        var arena = new ClayArena(1024 * 1024 * 4);
        UI.Begin(arena, new Dimensions(800, 600));

        UI.FloatingContainer("low", new LayoutConfig(), new FloatingConfig { ZIndex = 10 }, new Color(100, 100, 100));
        UI.FloatingContainer("high", new LayoutConfig(), new FloatingConfig { ZIndex = 100 }, new Color(255, 255, 255));
        UI.FloatingContainer("medium", new LayoutConfig(), new FloatingConfig { ZIndex = 50 }, new Color(150, 150, 150));

        var commands = UI.End();

        // 0: RootContainer (Z=0)
        // 1: "low" (Z=10)
        // 2: "medium" (Z=50)
        // 3: "high" (Z=100)
        
        Assert.Equal(4, commands.Length);
        Assert.Equal(0, commands[0].ZIndex);
        Assert.Equal(10, commands[1].ZIndex);
        Assert.Equal(50, commands[2].ZIndex);
        Assert.Equal(100, commands[3].ZIndex);
    }

    private string GetIdString(uint id, ClayArena arena)
    {
        // This is tricky because we don't store the original string in the arena by default for elements
        // But in our tests we can check if it matches the expected hash
        if (id == HashUtility.HashId("low", HashUtility.HashId("Clay__RootContainer", 0))) return "low";
        if (id == HashUtility.HashId("medium", HashUtility.HashId("Clay__RootContainer", 0))) return "medium";
        if (id == HashUtility.HashId("high", HashUtility.HashId("Clay__RootContainer", 0))) return "high";
        return "unknown";
    }
}
