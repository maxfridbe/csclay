using Raylib_cs;
using Clay;
using Color = Clay.Color;

namespace Clay.Demo;

class Program
{
    static void Main()
    {
        const int screenWidth = 800;
        const int screenHeight = 600;

        Raylib.InitWindow(screenWidth, screenHeight, "Clay C# - Raylib Demo");
        Raylib.SetTargetFPS(60);

        var arena = new ClayArena(1024 * 1024 * 10); // 10MB
        var context = new ClayContext(arena);
        UI.SetCurrentContext(context);

        // Text measurement callback
        context.TextMeasure = (text, config) =>
        {
            // Raylib measurement
            var size = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text.ToString(), config.FontSize, config.LetterSpacing);
            return new Clay.Dimensions(size.X, size.Y);
        };

        while (!Raylib.WindowShouldClose())
        {
            // 1. Update Interaction State
            var mousePos = Raylib.GetMousePosition();
            UI.SetPointerState(new Clay.Vector2(mousePos.X, mousePos.Y), Raylib.IsMouseButtonDown(MouseButton.Left));

            // 2. Declare UI
            UI.Begin(arena, new Clay.Dimensions(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()));
            
            UI.Container("root", new LayoutConfig 
            { 
                LayoutDirection = LayoutDirection.TopToBottom,
                ChildGap = 10,
                Padding = new Padding(20, 20),
                Sizing = new Sizing { Width = SizingAxis.Fixed(Raylib.GetScreenWidth()), Height = SizingAxis.Fixed(Raylib.GetScreenHeight()) }
            }, new Color(40, 44, 52), () => 
            {
                UI.Container("header", new LayoutConfig 
                { 
                    Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fixed(60) },
                    ChildAlignment = new ChildAlignment { X = LayoutAlignmentX.Center, Y = LayoutAlignmentY.Center }
                }, new Color(60, 64, 72), () => 
                {
                    UI.Text("CLAY C# PORT", new TextConfig { FontSize = 32, TextColor = new Color(255, 255, 255) });
                });

                UI.Container("body", new LayoutConfig 
                { 
                    LayoutDirection = LayoutDirection.LeftToRight,
                    ChildGap = 20,
                    Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() }
                }, () => 
                {
                    // Sidebar
                    UI.Container("sidebar", new LayoutConfig 
                    { 
                        Sizing = new Sizing { Width = SizingAxis.Fixed(200), Height = SizingAxis.Grow() },
                        Padding = new Padding(10, 10),
                        ChildGap = 10
                    }, new Color(50, 54, 62), () => 
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            var color = UI.Hovered() ? new Color(100, 150, 255) : new Color(70, 74, 82);
                            UI.Container($"item-{i}", new LayoutConfig 
                            { 
                                Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fixed(40) },
                                ChildAlignment = new ChildAlignment { X = LayoutAlignmentX.Center, Y = LayoutAlignmentY.Center }
                            }, color, () => 
                            {
                                UI.Text($"Menu Item {i}", new TextConfig { FontSize = 18, TextColor = new Color(255, 255, 255) });
                            });
                        }
                    });

                    // Content
                    UI.Container("content", new LayoutConfig 
                    { 
                        Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() },
                        Padding = new Padding(20, 20),
                        ChildGap = 20
                    }, new Color(30, 34, 42), () => 
                    {
                        UI.Text("Welcome to the Clay C# Port!", new TextConfig { FontSize = 24, TextColor = new Color(255, 255, 255) });
                        
                        UI.Text("This is a high-performance, zero-allocation (in the core loop) UI layout library ported from C. It supports complex Flexbox-like layouts, text wrapping, floating elements, and clipping.", 
                            new TextConfig { FontSize = 18, TextColor = new Color(200, 200, 200), WrapMode = TextWrapMode.Words });

                        UI.Container("buttons", new LayoutConfig { LayoutDirection = LayoutDirection.LeftToRight, ChildGap = 10 }, () => 
                        {
                            UI.Container("btn-1", new LayoutConfig { Padding = new Padding(15, 10) }, new Color(0, 120, 215), () => UI.Text("Button 1", new TextConfig { FontSize = 16, TextColor = new Color(255, 255, 255) }));
                            UI.Container("btn-2", new LayoutConfig { Padding = new Padding(15, 10) }, new Color(0, 120, 215), () => UI.Text("Button 2", new TextConfig { FontSize = 16, TextColor = new Color(255, 255, 255) }));
                        });
                    });
                });
            });

            var commands = UI.End();

            // 3. Render
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib_cs.Color.Black);

            foreach (var cmd in commands)
            {
                if (cmd.CommandType == RenderCommandType.Rectangle)
                {
                    var c = cmd.RenderData.Rectangle.Color;
                    Raylib.DrawRectangleRec(
                        new Raylib_cs.Rectangle(cmd.BoundingBox.X, cmd.BoundingBox.Y, cmd.BoundingBox.Width, cmd.BoundingBox.Height),
                        new Raylib_cs.Color((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A)
                    );
                }
                else if (cmd.CommandType == RenderCommandType.Text)
                {
                    var textData = cmd.RenderData.Text;
                    string fullText = context.GetString(textData.TextIndex);
                    string lineText = fullText.Substring(textData.LineStart, textData.LineLength);
                    
                    var c = textData.TextColor;
                    Raylib.DrawText(lineText, (int)cmd.BoundingBox.X, (int)cmd.BoundingBox.Y, textData.FontSize, new Raylib_cs.Color((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A));
                }
                else if (cmd.CommandType == RenderCommandType.ScissorStart)
                {
                    Raylib.BeginScissorMode((int)cmd.BoundingBox.X, (int)cmd.BoundingBox.Y, (int)cmd.BoundingBox.Width, (int)cmd.BoundingBox.Height);
                }
                else if (cmd.CommandType == RenderCommandType.ScissorEnd)
                {
                    Raylib.EndScissorMode();
                }
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
