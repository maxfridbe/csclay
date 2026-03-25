using Raylib_cs;
using CSClay;
using CSClay.Fluent;
using static CSClay.Fluent.Clay;
using Color = CSClay.Color;

namespace CSClay.Demo;

class Program
{
    static bool showModal = false;
    static float globalScale = 1.0f;
    static float scrollY = 0f;

    static void Main()
    {
        const int screenWidth = 800;
        const int screenHeight = 600;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(screenWidth, screenHeight, "CSClay - Fluent Raylib Demo");
        Raylib.SetTargetFPS(60);

        var arena = new ClayArena(1024 * 1024 * 10); // 10MB
        var context = new ClayContext(arena);
        UI.SetCurrentContext(context);

        // Load Nerd Font with icons
        string fontPath = "assets/fonts/JetBrainsMonoNerdFont-Regular.ttf";
        
        // Define codepoint ranges for Nerd Font (Basic Latin + Private Use Area for icons)
        int[] codepoints = new int[512];
        for (int i = 0; i < 256; i++) codepoints[i] = i; // Basic Latin
        // Add specific icons used in the demo:
        // 󰀘 (U+F0018), 󰉋 (U+F024B), 󰒓 (U+F0493), 󰅖 (U+F0156), 󰄬 (U+F012C), 󰄱 (U+F0131), 󰋚 (U+F02DA)
        codepoints[256] = 0xF0018; 
        codepoints[257] = 0xF024B;
        codepoints[258] = 0xF0493;
        codepoints[259] = 0xF0156;
        codepoints[260] = 0xF012C;
        codepoints[261] = 0xF0131;
        codepoints[262] = 0xF02DA;

        Font nerdFont;
        // Raylib-cs LoadFontEx managed overload
        nerdFont = Raylib.LoadFontEx(fontPath, 64, codepoints, 263);
        Raylib.SetTextureFilter(nerdFont.Texture, TextureFilter.Bilinear);

        // Text measurement callback using the Nerd Font
        context.TextMeasure = (text, config) =>
        {
            var size = Raylib.MeasureTextEx(nerdFont, text.ToString(), config.FontSize, config.LetterSpacing);
            return new CSClay.Dimensions(size.X, size.Y);
        };

        RunMainLoop(arena, context, nerdFont);

        Raylib.UnloadFont(nerdFont);
        Raylib.CloseWindow();
    }

    static void RunMainLoop(ClayArena arena, ClayContext context, Font nerdFont)
    {
        while (!Raylib.WindowShouldClose())
        {
            // 0. Handle Scale Input
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
            {
                if (Raylib.IsKeyPressed(KeyboardKey.Equal)) globalScale += 0.1f;
                if (Raylib.IsKeyPressed(KeyboardKey.Minus)) globalScale = Math.Max(0.5f, globalScale - 0.1f);
            }

            // 1. Update Interaction State
            var mousePos = Raylib.GetMousePosition();
            UI.SetPointerState(new CSClay.Vector2(mousePos.X, mousePos.Y), Raylib.IsMouseButtonDown(MouseButton.Left));

            // 2. Declare UI
            UI.Begin(arena, new CSClay.Dimensions(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()));
            
            Container("root", c => c
                .Direction(LayoutDirection.TopToBottom)
                .ChildGap((ushort)(10 * globalScale))
                .Padding((ushort)(20 * globalScale), (ushort)(20 * globalScale))
                .Sizing(Fixed(Raylib.GetScreenWidth()), Fixed(Raylib.GetScreenHeight()))
                .Color(40, 44, 52)
            , () => 
            {
                Container("header", c => c
                    .Sizing(Grow(), Fixed(60 * globalScale))
                    .Align(LayoutAlignmentX.Center, LayoutAlignmentY.Center)
                    .Color(60, 64, 72)
                , () => 
                {
                    Text("󰀘 CSCLAY PORT", t => t.Size((ushort)(32 * globalScale)).Color(255, 255, 255));
                });

                Container("body", c => c
                    .Direction(LayoutDirection.LeftToRight)
                    .ChildGap((ushort)(20 * globalScale))
                    .Sizing(Grow(), Grow())
                , () => 
                {
                    // Sidebar
                    Container("sidebar", c => c
                        .Direction(LayoutDirection.TopToBottom)
                        .Sizing(Fixed(200 * globalScale), Grow())
                        .Padding((ushort)(10 * globalScale), (ushort)(10 * globalScale))
                        .ChildGap((ushort)(10 * globalScale))
                        .Color(50, 54, 62)
                    , () => 
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            string itemId = $"sidebar-item-{i}";
                            var color = UI.IsHovered(itemId) ? new Color(100, 150, 255) : new Color(70, 74, 82);
                            Container(itemId, c => c
                                .Sizing(Grow(), Fixed(40 * globalScale))
                                .Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center)
                                .Padding((ushort)(10 * globalScale), 0)
                                .Color(color)
                            , () => 
                            {
                                Text($"󰉋 Menu Item {i}", t => t.Size((ushort)(18 * globalScale)).Color(255, 255, 255));
                            });
                        }
                    });

                    // Content
                    Container("content", c => c
                        .Direction(LayoutDirection.TopToBottom)
                        .Sizing(Grow(), Grow())
                        .Padding((ushort)(20 * globalScale), (ushort)(20 * globalScale))
                        .ChildGap((ushort)(20 * globalScale))
                        .Color(30, 34, 42)
                    , () => 
                    {
                        Text("Welcome to the Clay C# Port!", t => t.Size((ushort)(24 * globalScale)).Color(255, 255, 255));
                        
                        Text("This is a high-performance, zero-allocation UI layout library ported from C. Use Ctrl + and Ctrl - to scale the UI!", 
                            t => t.Size((ushort)(18 * globalScale)).Color(200, 200, 200).Wrap(TextWrapMode.Words));

                        // Modal Trigger Button
                        string modalBtnId = "modal-trigger";
                        var btnColor = UI.IsHovered(modalBtnId) ? new Color(80, 180, 100) : new Color(40, 140, 60);
                        Container(modalBtnId, c => c
                            .Padding((ushort)(20 * globalScale), (ushort)(10 * globalScale))
                            .Color(btnColor)
                        , () => 
                        {
                            Text("󰒓 Open Settings", t => t.Size((ushort)(18 * globalScale)).Color(255, 255, 255));
                            
                            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && UI.Hovered())
                            {
                                showModal = !showModal;
                            }

                            if (showModal)
                            {
                                // Render a popup menu attached to the button
                                Container("modal", 
                                    c => c
                                        .Sizing(Fixed(300 * globalScale), Fit())
                                        .Direction(LayoutDirection.TopToBottom)
                                        .Color(45, 49, 57, 240) // Slightly transparent
                                        .Floating(f => f.Attach(FloatingAttachPoint.LeftTop, FloatingAttachPoint.LeftBottom, FloatingAttachToElement.Parent).ZIndex(1000).Offset(0, 5)),
                                    () => 
                                    {
                                        Container("modal-header", c => c
                                            .Sizing(Grow(), Fixed(50 * globalScale))
                                            .Padding((ushort)(15 * globalScale), 0)
                                            .Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center)
                                            .Color(70, 74, 82)
                                        , () => 
                                        {
                                            Text("󰒓 Settings Modal", t => t.Size((ushort)(20 * globalScale)).Color(255, 255, 255));
                                            
                                            // Close button
                                            Container("close-btn", c => c.Align(LayoutAlignmentX.Right, LayoutAlignmentY.Center).Sizing(Grow(), Fit()), () => {
                                                Text("󰅖 ", t => t.Size((ushort)(24 * globalScale)).Color(255, 100, 100));
                                                if (Raylib.IsMouseButtonPressed(MouseButton.Left) && UI.Hovered()) showModal = false;
                                            });
                                        });

                                        Container("modal-body", c => c.Direction(LayoutDirection.TopToBottom).Padding((ushort)(20 * globalScale), (ushort)(20 * globalScale)).ChildGap((ushort)(15 * globalScale)), () => 
                                        {
                                            Text("󰄬 Setting 1: Enabled", t => t.Size((ushort)(18 * globalScale)).Color(200, 200, 200));
                                            Text("󰄬 Setting 2: Performance Mode", t => t.Size((ushort)(18 * globalScale)).Color(200, 200, 200));
                                            Text("󰄱 Setting 3: Beta Features", t => t.Size((ushort)(18 * globalScale)).Color(150, 150, 150));
                                        });
                                    }
                                );
                            }
                        });

                        // Scrollable pane
                        Container("scroll-pane", 
                            c => c
                                .Sizing(Grow(), Grow())
                                .Direction(LayoutDirection.TopToBottom)
                                .ChildGap((ushort)(10 * globalScale))
                                .Color(25, 29, 36)
                                .Scroll(cl => cl.Vertical().Offset(0, scrollY)),
                            () => 
                        {
                            if (UI.Hovered())
                            {
                                float wheel = Raylib.GetMouseWheelMove();
                                if (wheel != 0)
                                {
                                    scrollY += wheel * 30.0f; // Scroll speed
                                    if (scrollY > 0) scrollY = 0; // Prevent scrolling past top
                                    // A real implementation would also clamp the bottom based on content height
                                }
                            }

                            for (int i = 1; i <= 100; i++)
                            {
                                string scrollItemId = $"scroll-item-{i}";
                                var itemColor = UI.IsHovered(scrollItemId) ? new Color(70, 100, 150) : new Color(45, 49, 57);
                                
                                Container(scrollItemId, c => c
                                    .Sizing(Grow(), Fixed(50 * globalScale))
                                    .Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center)
                                    .Padding((ushort)(15 * globalScale), 0)
                                    .Color(itemColor)
                                , () => 
                                {
                                    Text($"󰋚 Scrollable Item {i}", t => t.Size((ushort)(18 * globalScale)).Color(220, 220, 220));
                                });
                            }
                        });
                    });
                });
            });

            var commands = UI.End();

            // 3. Render
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib_cs.Color.Black);

            CSClay.Renderers.Raylib.RaylibRenderer.Render(commands, context, nerdFont);

            Raylib.EndDrawing();
        }
    }
}
