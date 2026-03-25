using Raylib_cs;
using CSClay;
using CSClay.Fluent;
using static CSClay.Fluent.Clay;
using Color = CSClay.Color;

namespace CSClay.Demo;

class Program
{
    static bool showModal = false;

    static void Main()
    {
        const int screenWidth = 800;
        const int screenHeight = 600;

        Raylib.InitWindow(screenWidth, screenHeight, "Clay C# - Fluent Raylib Demo");
        Raylib.SetTargetFPS(60);

        // Load Nerd Font
        string fontPath = "assets/fonts/JetBrainsMonoNerdFont-Regular.ttf";
        // We load a decent size and include enough characters for icons
        Font nerdFont = Raylib.LoadFontEx(fontPath, 32, null, 0);
        Raylib.SetTextureFilter(nerdFont.Texture, TextureFilter.Bilinear);

        var arena = new ClayArena(1024 * 1024 * 10); // 10MB
        var context = new ClayContext(arena);
        UI.SetCurrentContext(context);

        // Text measurement callback using the Nerd Font
        context.TextMeasure = (text, config) =>
        {
            var size = Raylib.MeasureTextEx(nerdFont, text.ToString(), config.FontSize, config.LetterSpacing);
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
                    Text("󰀘 CSCLAY PORT", t => t.Size(32).Color(255, 255, 255));
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
                                .Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center)
                                .Padding(10, 0)
                            , color, () => 
                            {
                                Text($"󰉋 Menu Item {i}", t => t.Size(18).Color(255, 255, 255));
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
                        
                        Text("This is a high-performance, zero-allocation UI layout library ported from C. Click the button below to see a floating element!", 
                            t => t.Size(18).Color(200, 200, 200).Wrap(TextWrapMode.Words));

                        // Modal Trigger Button
                        var btnColor = UI.Hovered() ? new Color(80, 180, 100) : new Color(40, 140, 60);
                        Container("modal-trigger", c => c.Padding(20, 10), btnColor, () => 
                        {
                            Text("󰒓 Open Settings", t => t.Size(18).Color(255, 255, 255));
                            
                            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && UI.Hovered())
                            {
                                showModal = !showModal;
                            }
                        });

                        if (showModal)
                        {
                            // Render a simple centered floating element
                            FloatingContainer("modal", 
                                l => l.Sizing(Fixed(400), Fixed(300)),
                                f => f.Attach(FloatingAttachPoint.CenterCenter, FloatingAttachPoint.CenterCenter, FloatingAttachToElement.Root).ZIndex(1000),
                                new Color(45, 49, 57, 240), // Slightly transparent
                                () => 
                                {
                                    Container("modal-header", c => c.Sizing(Grow(), Fixed(50)).Padding(15, 0).Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center), new Color(70, 74, 82), () => 
                                    {
                                        Text("󰒓 Settings Modal", t => t.Size(20).Color(255, 255, 255));
                                        
                                        // Close button
                                        Container("close-btn", c => c.Align(LayoutAlignmentX.Right, LayoutAlignmentY.Center).Sizing(Grow(), Fit()), () => {
                                            Text("󰅖 ", t => t.Size(24).Color(255, 100, 100));
                                            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && UI.Hovered()) showModal = false;
                                        });
                                    });

                                    Container("modal-body", c => c.Padding(20, 20).ChildGap(15), () => 
                                    {
                                        Text("󰄬 Setting 1: Enabled", t => t.Size(18).Color(200, 200, 200));
                                        Text("󰄬 Setting 2: Performance Mode", t => t.Size(18).Color(200, 200, 200));
                                        Text("󰄱 Setting 3: Beta Features", t => t.Size(18).Color(150, 150, 150));
                                    });
                                }
                            );
                        }
                    });
                });
            });

            var commands = UI.End();

            // 3. Render
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib_cs.Color.Black);

            // We need to pass the font to our renderer or handle it globally.
            // Since our current RaylibRenderer is simple, let's update it to support custom fonts or just use our nerd font here.
            // For now, I will modify the Render loop to use the loaded font.
            RenderWithNerdFont(commands, context, nerdFont);

            Raylib.EndDrawing();
        }

        Raylib.UnloadFont(nerdFont);
        Raylib.CloseWindow();
    }

    // Temporary helper to render with the nerd font until RaylibRenderer is font-aware
    static void RenderWithNerdFont(Span<RenderCommand> commands, ClayContext context, Font font)
    {
        foreach (var cmd in commands)
        {
            var rect = new Rectangle(cmd.BoundingBox.X, cmd.BoundingBox.Y, cmd.BoundingBox.Width, cmd.BoundingBox.Height);

            switch (cmd.CommandType)
            {
                case RenderCommandType.Rectangle:
                    var c = cmd.RenderData.Rectangle.Color;
                    Raylib.DrawRectangleRec(rect, new Raylib_cs.Color((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A));
                    break;
                case RenderCommandType.Text:
                    var textData = cmd.RenderData.Text;
                    string fullText = context.GetString(textData.TextIndex);
                    string lineText = fullText.Substring(textData.LineStart, textData.LineLength);
                    var tc = textData.TextColor;
                    Raylib.DrawTextEx(font, lineText, new System.Numerics.Vector2(rect.X, rect.Y), textData.FontSize, 0, new Raylib_cs.Color((byte)tc.R, (byte)tc.G, (byte)tc.B, (byte)tc.A));
                    break;
                case RenderCommandType.ScissorStart:
                    Raylib.BeginScissorMode((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                    break;
                case RenderCommandType.ScissorEnd:
                    Raylib.EndScissorMode();
                    break;
            }
        }
    }
}
