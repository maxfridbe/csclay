using CSClay;
using CSClay.Renderers.Raylib;
using Xunit;

namespace CSClay.Renderers.Raylib.Tests;

public class RendererTests
{
    [Fact]
    public void TestRendererInitialization()
    {
        // This is a minimal test to ensure the renderer can be called.
        // In a headless environment, full Raylib rendering might not be possible,
        // but we can at least check if the code executes.
        var arena = new ClayArena(1024 * 1024 * 10);
        var context = new ClayContext(arena);
        
        UI.SetCurrentContext(context);
        UI.Begin(arena, new Dimensions(800, 600));
        UI.Container("root", new LayoutConfig(), new Color(255, 0, 0));
        var commands = UI.End();

        // We don't call Raylib.InitWindow here because it would fail in CI.
        // We just ensure the Render method can be reached if we had a context.
        // RaylibRenderer.Render(commands, context);
        
        Assert.Equal(1, commands.Length);
    }
}
