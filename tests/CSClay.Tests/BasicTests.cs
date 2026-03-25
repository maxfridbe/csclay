using CSClay;
using Xunit;

namespace CSClay.Tests;

public class BasicTests
{
    [Fact]
    public void TestArenaAllocation()
    {
        var arena = new ClayArena(1024);
        int offset = arena.Allocate<int>(10);
        Assert.Equal(0, offset);
        Assert.Equal(40, arena.NextAllocation);

        var span = arena.GetSpan<int>(offset, 10);
        Assert.Equal(10, span.Length);
        
        for (int i = 0; i < 10; i++) span[i] = i;
        for (int i = 0; i < 10; i++) Assert.Equal(i, span[i]);
    }

    [Fact]
    public void TestIdHashing()
    {
        uint seed = 0;
        uint hash1 = HashUtility.HashString("test", seed);
        uint hash2 = HashUtility.HashString("test", seed);
        uint hash3 = HashUtility.HashString("other", seed);

        Assert.Equal(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
    }

    [Fact]
    public void TestDeclarativeStructure()
    {
        var arena = new ClayArena(1024 * 1024 * 4); // 4MB
        UI.Begin(arena, new Dimensions(800, 600));

        bool innerCalled = false;
        UI.Container("root", new LayoutConfig(), () => 
        {
            UI.Container("child1", new LayoutConfig());
            UI.Container("child2", new LayoutConfig(), () => 
            {
                innerCalled = true;
                UI.Text("Hello", new TextConfig());
            });
        });

        UI.End();
        Assert.True(innerCalled);
    }
}
