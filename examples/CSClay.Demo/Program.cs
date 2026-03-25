using Raylib_cs;
using CSClay;
using CSClay.Fluent;
using static CSClay.Fluent.Clay;
using Color = CSClay.Color;

namespace CSClay.Demo;

class Program
{
    static void Main()
    {
        const int screenWidth = 800;
        const int screenHeight = 600;

        Raylib.InitWindow(screenWidth, screenHeight, "Clay C# - Fluent Raylib Demo");
        Raylib.SetTargetFPS(60);

        var arena = new ClayArena(1024 * 1024 * 10); // 10MB
        var context = new ClayContext(arena);
        UI.SetCurrentContext(context);

        // Text measurement callback
        context.TextMeasure = (text, config) =>
        {
            // Raylib measurement
            var size = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text.ToString(), config.FontSize, config.LetterSpacing);
            return new CSClay.Dimensions(size.X, size.Y);
        };

        while (!Raylib.WindowShouldClose())
        {
            // 1. Update Interaction State
            var mousePos = Raylib.GetMousePosition();
            UI.SetPointerState(new CSClay.Vector2(mousePos.X, mousePos.Y), Raylib.IsMouseButtonDown(MouseButton.Left));

            // 2. Declare UI
            UI.Begin(arena, new CSClay.Dimensions(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()));
            
            Container("root", c => c
                .Direction(LayoutDirection.TopToBottom)
                .ChildGap(10)
                .Padding(20, 20)
                .Sizing(Fixed(Raylib.GetScreenWidth()), Fixed(Raylib.GetScreenHeight()))
            , new Color(40, 44, 52), () => 
            {
                Container("header", c => c
                    .Sizing(Grow(), Fixed(60))
                    .Align(LayoutAlignmentX.Center, LayoutAlignmentY.Center)
                , new Color(60, 64, 72), () => 
                {
                    Text("CLAY C# PORT", t => t.Size(32).Color(255, 255, 255));
                });

                Container("body", c => c
                    .Direction(LayoutDirection.LeftToRight)
                    .ChildGap(20)
                    .Sizing(Grow(), Grow())
                , () => 
                {
                    // Sidebar
                    Container("sidebar", c => c
                        .Sizing(Fixed(200), Grow())
                        .Padding(10, 10)
                        .ChildGap(10)
                    , new Color(50, 54, 62), () => 
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            var color = UI.Hovered() ? new Color(100, 150, 255) : new Color(70, 74, 82);
                            Container($"item-{i}", c => c
                                .Sizing(Grow(), Fixed(40))
                                .Align(LayoutAlignmentX.Center, LayoutAlignmentY.Center)
                            , color, () => 
                            {
                                Text($"Menu Item {i}", t => t.Size(18).Color(255, 255, 255));
                            });
                        }
                    });

                    // Content
                    Container("content", c => c
                        .Sizing(Grow(), Grow())
                        .Padding(20, 20)
                        .ChildGap(20)
                    , new Color(30, 34, 42), () => 
                    {
                        Text("Welcome to the Clay C# Port!", t => t.Size(24).Color(255, 255, 255));
                        
                        Text("This is a high-performance, zero-allocation (in the core loop) UI layout library ported from C. It supports complex Flexbox-like layouts, text wrapping, floating elements, and clipping.", 
                            t => t.Size(18).Color(200, 200, 200).Wrap(TextWrapMode.Words));

                        Container("buttons", c => c.Direction(LayoutDirection.LeftToRight).ChildGap(10), () => 
                        {
                            Container("btn-1", c => c.Padding(15, 10), new Color(0, 120, 215), () => 
                                Text("Button 1", t => t.Size(16).Color(255, 255, 255)));
                            Container("btn-2", c => c.Padding(15, 10), new Color(0, 120, 215), () => 
                                Text("Button 2", t => t.Size(16).Color(255, 255, 255)));
                        });
                    });
                });
            });

            var commands = UI.End();

            // 3. Render
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib_cs.Color.Black);

            CSClay.Renderers.Raylib.RaylibRenderer.Render(commands, context);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
