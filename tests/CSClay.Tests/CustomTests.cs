using CSClay;
using Xunit;

namespace CSClay.Tests;

public class CustomTests
{
    [Fact]
    public void TestCustomElement()
    {
        var arena = new ClayArena(1024 * 1024 * 4);
        UI.Begin(arena, new Dimensions(800, 600));

        UI.Custom("my-widget", new CustomConfig { CustomDataId = 123 });

        var commands = UI.End();

        // 0: "my-widget" (Custom)
        Assert.Equal(1, commands.Length);
        Assert.Equal(RenderCommandType.Custom, commands[0].CommandType);
    }
}
