using Raylib_cs;
using CSClay;
using CSClay.Fluent;
using static CSClay.Fluent.Clay;
using Color = CSClay.Color;

namespace CSClay.Demo;

class Program
{
    class Theme
    {
        public Color Background;
        public Color HeaderBg;
        public Color TextPrimary;
        public Color TextSecondary;
        public Color SidebarBg;
        public Color SidebarItemHover;
        public Color SidebarItemIdle;
        public Color ContentBg;
        public Color ButtonHover;
        public Color ButtonIdle;
        public Color ModalBg;
        public Color ModalHeaderBg;
        public Color ScrollPaneBg;
        public Color ScrollItemHover;
        public Color ScrollItemIdle;
        public Raylib_cs.Color RaylibBackground;
    }

    static readonly Theme DarkTheme = new Theme
    {
        Background = new Color(40, 44, 52),
        HeaderBg = new Color(60, 64, 72),
        TextPrimary = new Color(255, 255, 255),
        TextSecondary = new Color(200, 200, 200),
        SidebarBg = new Color(50, 54, 62),
        SidebarItemHover = new Color(100, 150, 255),
        SidebarItemIdle = new Color(70, 74, 82),
        ContentBg = new Color(30, 34, 42),
        ButtonHover = new Color(80, 180, 100),
        ButtonIdle = new Color(40, 140, 60),
        ModalBg = new Color(45, 49, 57, 240),
        ModalHeaderBg = new Color(70, 74, 82),
        ScrollPaneBg = new Color(25, 29, 36),
        ScrollItemHover = new Color(70, 100, 150),
        ScrollItemIdle = new Color(45, 49, 57),
        RaylibBackground = Raylib_cs.Color.Black
    };

    static readonly Theme LightTheme = new Theme
    {
        Background = new Color(240, 240, 245),
        HeaderBg = new Color(220, 220, 230),
        TextPrimary = new Color(20, 20, 20),
        TextSecondary = new Color(80, 80, 80),
        SidebarBg = new Color(230, 230, 240),
        SidebarItemHover = new Color(150, 180, 255),
        SidebarItemIdle = new Color(210, 210, 220),
        ContentBg = new Color(250, 250, 255),
        ButtonHover = new Color(100, 200, 120),
        ButtonIdle = new Color(60, 160, 80),
        ModalBg = new Color(235, 235, 245, 240),
        ModalHeaderBg = new Color(210, 210, 220),
        ScrollPaneBg = new Color(255, 255, 255),
        ScrollItemHover = new Color(200, 220, 255),
        ScrollItemIdle = new Color(240, 240, 250),
        RaylibBackground = Raylib_cs.Color.RayWhite
    };

    static bool showModal = false;
    static float globalScale = 1.0f;
    static float scrollY = 0f;
    static bool isDarkTheme = true;
    static bool isSpinning = true;
    static float rotationAngle = 0f;
    static bool isCachedMode = false;
    static bool isDirty = true;

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
        RenderTexture2D cacheTexture = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        while (!Raylib.WindowShouldClose())
        {
            bool inputChanged = false;

            if (Raylib.IsWindowResized())
            {
                Raylib.UnloadRenderTexture(cacheTexture);
                cacheTexture = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                inputChanged = true;
            }

            var mouseDelta = Raylib.GetMouseDelta();
            if (mouseDelta.X != 0 || mouseDelta.Y != 0) inputChanged = true;
            if (Raylib.GetMouseWheelMove() != 0) inputChanged = true;
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) || Raylib.IsMouseButtonReleased(MouseButton.Left)) inputChanged = true;

            // 0. Handle Input (Scale and Theme)
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
            {
                if (Raylib.IsKeyPressed(KeyboardKey.Equal)) { globalScale += 0.1f; inputChanged = true; }
                if (Raylib.IsKeyPressed(KeyboardKey.Minus)) { globalScale = Math.Max(0.5f, globalScale - 0.1f); inputChanged = true; }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.T)) { isDarkTheme = !isDarkTheme; inputChanged = true; }
            if (Raylib.IsKeyPressed(KeyboardKey.S)) isSpinning = !isSpinning;
            if (Raylib.IsKeyPressed(KeyboardKey.C)) { isCachedMode = !isCachedMode; inputChanged = true; }

            if (isSpinning)
            {
                rotationAngle += 1.0f;
                if (rotationAngle >= 360f) rotationAngle -= 360f;
            }

            if (inputChanged || !isCachedMode) isDirty = true;

            Theme theme = isDarkTheme ? DarkTheme : LightTheme;

            // Handle scrolling
            var mousePos = Raylib.GetMousePosition();
            UI.SetPointerState(new CSClay.Vector2(mousePos.X, mousePos.Y), Raylib.IsMouseButtonDown(MouseButton.Left));

            // 2. Declare UI
            void DeclareUI()
            {
            UI.Begin(arena, new CSClay.Dimensions(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()));
            
                Container("root", c => c
                    .Direction(LayoutDirection.TopToBottom)
                    .ChildGap((ushort)(10 * globalScale))
                    .Padding((ushort)(20 * globalScale), (ushort)(20 * globalScale))
                    .Sizing(Fixed(Raylib.GetScreenWidth()), Fixed(Raylib.GetScreenHeight()))
                    .Color(theme.Background)
                , () => 
                {
                    Container("header", c => c
                        .Sizing(Grow(), Fixed(60 * globalScale))
                        .Align(LayoutAlignmentX.Center, LayoutAlignmentY.Center)
                        .Color(theme.HeaderBg)
                    , () => 
                    {
                        Text("󰀘 CSCLAY PORT", t => t.Size((ushort)(32 * globalScale)).Color(theme.TextPrimary));
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
                            .Color(theme.SidebarBg)
                        , () => 
                        {
                            for (int i = 1; i <= 5; i++)
                            {
                                string itemId = $"sidebar-item-{i}";
                                var color = UI.IsHovered(itemId) ? theme.SidebarItemHover : theme.SidebarItemIdle;
                                Container(itemId, c => c
                                    .Sizing(Grow(), Fixed(40 * globalScale))
                                    .Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center)
                                    .Padding((ushort)(10 * globalScale), 0)
                                    .Color(color)
                                    .CornerRadius(8 * globalScale)
                                , () => 
                                {
                                    Text($"󰉋 Menu Item {i}", t => t.Size((ushort)(18 * globalScale)).Color(theme.TextPrimary));
                                });
                            }
                        });

                        // Content
                        Container("content", c => c
                            .Direction(LayoutDirection.TopToBottom)
                            .Sizing(Grow(), Grow())
                            .Padding((ushort)(20 * globalScale), (ushort)(20 * globalScale))
                            .ChildGap((ushort)(20 * globalScale))
                            .Color(theme.ContentBg)
                        , () => 
                        {
                            Container("header-row", c => c.Sizing(Grow(), Fit()).Direction(LayoutDirection.LeftToRight).ChildGap(20), () => 
                            {
                                Text("Welcome to the Clay C# Port!", t => t.Size((ushort)(24 * globalScale)).Color(theme.TextPrimary));
                            
                                // Custom Element for Spinning Cube
                                Custom("spinning-cube", c => c
                                    .Sizing(Fixed(100 * globalScale), Fixed(100 * globalScale))
                                , 1); // 1 = Cube Custom ID
                            });
                        
                            Text("This is a high-performance, zero-allocation UI layout library ported from C. Use Ctrl + and Ctrl - to scale the UI!", 
                                t => t.Size((ushort)(18 * globalScale)).Color(theme.TextSecondary).Wrap(TextWrapMode.Words));

                            // Modal Trigger Button
                            string modalBtnId = "modal-trigger";
                            var btnColor = UI.IsHovered(modalBtnId) ? theme.ButtonHover : theme.ButtonIdle;
                            Container(modalBtnId, c => c
                                .Padding((ushort)(20 * globalScale), (ushort)(10 * globalScale))
                                .Color(btnColor)
                                .CornerRadius(8 * globalScale)
                            , () => 
                            {
                                Text("󰒓 Open Settings", t => t.Size((ushort)(18 * globalScale)).Color(theme.TextPrimary));
                            
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
                                            .Color(theme.ModalBg)
                                            .Floating(f => f.Attach(FloatingAttachPoint.LeftTop, FloatingAttachPoint.LeftBottom, FloatingAttachToElement.Parent).ZIndex(1000).Offset(0, 5)),
                                        () => 
                                        {
                                            Container("modal-header", c => c
                                                .Sizing(Grow(), Fixed(50 * globalScale))
                                                .Padding((ushort)(15 * globalScale), 0)
                                                .Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center)
                                                .Color(theme.ModalHeaderBg)
                                            , () => 
                                            {
                                                Text("󰒓 Settings Modal", t => t.Size((ushort)(20 * globalScale)).Color(theme.TextPrimary));
                                            
                                                // Close button
                                                Container("close-btn", c => c.Align(LayoutAlignmentX.Right, LayoutAlignmentY.Center).Sizing(Grow(), Fit()), () => {
                                                    Text("󰅖 ", t => t.Size((ushort)(24 * globalScale)).Color(new Color(255, 100, 100)));
                                                    if (Raylib.IsMouseButtonPressed(MouseButton.Left) && UI.Hovered()) showModal = false;
                                                });
                                            });

                                            Container("modal-body", c => c.Direction(LayoutDirection.TopToBottom).Padding((ushort)(20 * globalScale), (ushort)(20 * globalScale)).ChildGap((ushort)(15 * globalScale)), () => 
                                            {
                                                Text("󰄬 Setting 1: Enabled", t => t.Size((ushort)(18 * globalScale)).Color(theme.TextSecondary));
                                                Text("󰄬 Setting 2: Performance Mode", t => t.Size((ushort)(18 * globalScale)).Color(theme.TextSecondary));
                                                Text("󰄱 Setting 3: Beta Features", t => t.Size((ushort)(18 * globalScale)).Color(theme.TextSecondary));
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
                                    .Color(theme.ScrollPaneBg)
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
                                    var itemColor = UI.IsHovered(scrollItemId) ? theme.ScrollItemHover : theme.ScrollItemIdle;
                                
                                    Container(scrollItemId, c => c
                                        .Sizing(Grow(), Fixed(50 * globalScale))
                                        .Align(LayoutAlignmentX.Left, LayoutAlignmentY.Center)
                                        .Padding((ushort)(15 * globalScale), 0)
                                        .Color(itemColor)
                                        .CornerRadius(8 * globalScale)
                                    , () => 
                                    {
                                        Text($"󰋚 Scrollable Item {i}", t => t.Size((ushort)(18 * globalScale)).Color(theme.TextPrimary));
                                    });
                                }
                            });
                        });
                    });
                });
            }

            DeclareUI();
            var commands = UI.End();

            if (Raylib.IsKeyPressed(KeyboardKey.P))
            {
                var info = new SkiaSharp.SKImageInfo(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                using var surface = SkiaSharp.SKSurface.Create(info);
                var canvas = surface.Canvas;
                canvas.Clear(new SkiaSharp.SKColor((byte)theme.Background.R, (byte)theme.Background.G, (byte)theme.Background.B, (byte)theme.Background.A));
                
                using var typeface = SkiaSharp.SKTypeface.FromFile("assets/fonts/JetBrainsMonoNerdFont-Regular.ttf");
                using var skPaint = new SkiaSharp.SKPaint { IsAntialias = true };
                using var skFont = new SkiaSharp.SKFont(typeface);

                // Swap TextMeasure to SkiaSharp
                var oldMeasure = context.TextMeasure;
                context.TextMeasure = (text, config) =>
                {
                    skFont.Size = config.FontSize;
                    var width = skFont.MeasureText(text.ToString());
                    return new CSClay.Dimensions(width, skFont.Metrics.Descent - skFont.Metrics.Ascent);
                };

                // Recalculate layout with Skia metrics
                DeclareUI();
                var skiaCommands = UI.End();

                // Render Skia
                CSClay.Renderers.SkiaSharp.SkiaSharpRenderer.Render(canvas, skiaCommands, context, typeface);
                
                using var img = surface.Snapshot();
                using var data = img.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                using var stream = System.IO.File.OpenWrite("screenshot.png");
                data.SaveTo(stream);
                System.Console.WriteLine("Saved screenshot.png to disk using SkiaSharp metrics.");

                // Restore Raylib TextMeasure
                context.TextMeasure = oldMeasure;
            }

                        // 3. Render
            if (isDirty)
            {
                Raylib.BeginTextureMode(cacheTexture);
                Raylib.ClearBackground(theme.RaylibBackground);

                CSClay.Renderers.Raylib.RaylibRenderer.Render(commands, context, nerdFont, (cmd, rect) => 
                {
                    if (cmd.RenderData.Custom.CustomDataId == 1) // Our Cube ID
                    {
                        // Draw background for cube in cache
                        Raylib.BeginScissorMode((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                        Raylib.DrawRectangleRec(rect, new Raylib_cs.Color(50, 50, 50, 255));
                        Raylib.EndScissorMode();
                    }
                });

                Raylib.EndTextureMode();
                isDirty = false;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(theme.RaylibBackground);

            // Draw the cached UI
            Raylib.DrawTextureRec(cacheTexture.Texture, new Rectangle(0, 0, cacheTexture.Texture.Width, -cacheTexture.Texture.Height), new System.Numerics.Vector2(0, 0), Raylib_cs.Color.White);

            // Draw dynamic elements on top
            foreach (var cmd in commands)
            {
                if (cmd.CommandType == RenderCommandType.Custom && cmd.RenderData.Custom.CustomDataId == 1)
                {
                    var rect = new Rectangle(cmd.BoundingBox.X, cmd.BoundingBox.Y, cmd.BoundingBox.Width, cmd.BoundingBox.Height);
                    DrawSpinningCube(rect, rotationAngle);
                }
            }

            Raylib.DrawText($"Mode: {(isCachedMode ? "Cached (Press 'C')" : "Continuous (Press 'C')")}", 10, 10, 20, isCachedMode ? Raylib_cs.Color.Green : Raylib_cs.Color.Red);

            Raylib.EndDrawing();
        }
        
        Raylib.UnloadRenderTexture(cacheTexture);
    }

    static void DrawSpinningCube(Rectangle rect, float rotationAngle)
    {
        Raylib.BeginScissorMode((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                    
        float cx = rect.X + rect.Width / 2;
        float cy = rect.Y + rect.Height / 2;
        float size = Math.Min(rect.Width, rect.Height) * 0.4f;
        
        float rad = rotationAngle * (float)Math.PI / 180f;
        float cos = (float)Math.Cos(rad);
        float sin = (float)Math.Sin(rad);

        System.Numerics.Vector3[] vertices = new System.Numerics.Vector3[]
        {
            new (-1, -1, -1), new (1, -1, -1), new (1, 1, -1), new (-1, 1, -1),
            new (-1, -1, 1), new (1, -1, 1), new (1, 1, 1), new (-1, 1, 1)
        };

        System.Numerics.Vector2[] proj = new System.Numerics.Vector2[8];
        for (int j = 0; j < 8; j++)
        {
            float x = vertices[j].X;
            float y = vertices[j].Y;
            float z = vertices[j].Z;

            float x1 = x * cos - z * sin;
            float z1 = z * cos + x * sin;
            
            float y2 = y * cos - z1 * sin;
            float z2 = z1 * cos + y * sin;

            proj[j] = new System.Numerics.Vector2(cx + x1 * size, cy + y2 * size);
        }

        int[,] edges = new int[,] {
            {0,1}, {1,2}, {2,3}, {3,0},
            {4,5}, {5,6}, {6,7}, {7,4},
            {0,4}, {1,5}, {2,6}, {3,7}
        };

        for (int j = 0; j < 12; j++)
        {
            Raylib.DrawLineEx(proj[edges[j,0]], proj[edges[j,1]], 2f, Raylib_cs.Color.Lime);
        }

        Raylib.EndScissorMode();
    }
}
