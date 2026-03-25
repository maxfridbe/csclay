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
            // 0. Handle Input (Scale and Theme)
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
            {
                if (Raylib.IsKeyPressed(KeyboardKey.Equal)) globalScale += 0.1f;
                if (Raylib.IsKeyPressed(KeyboardKey.Minus)) globalScale = Math.Max(0.5f, globalScale - 0.1f);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.T)) isDarkTheme = !isDarkTheme;
            if (Raylib.IsKeyPressed(KeyboardKey.S)) isSpinning = !isSpinning;

            if (isSpinning)
            {
                rotationAngle += 1.0f;
                if (rotationAngle >= 360f) rotationAngle -= 360f;
            }

            Theme theme = isDarkTheme ? DarkTheme : LightTheme;

            // Handle scrolling
            var mousePos = Raylib.GetMousePosition();
            UI.SetPointerState(new CSClay.Vector2(mousePos.X, mousePos.Y), Raylib.IsMouseButtonDown(MouseButton.Left));

            // 2. Declare UI
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
                                , () => 
                                {
                                    Text($"󰋚 Scrollable Item {i}", t => t.Size((ushort)(18 * globalScale)).Color(theme.TextPrimary));
                                });
                            }
                        });
                    });
                });
            });

            var commands = UI.End();

            // 3. Render
            Raylib.BeginDrawing();
            Raylib.ClearBackground(theme.RaylibBackground);

            CSClay.Renderers.Raylib.RaylibRenderer.Render(commands, context, nerdFont, (cmd, rect) => 
            {
                if (cmd.RenderData.Custom.CustomDataId == 1) // Our Cube ID
                {
                    // Draw a 3D cube inside the 2D rectangle using Raylib's 3D mode
                    Camera3D camera = new Camera3D();
                    camera.Position = new System.Numerics.Vector3(0.0f, 10.0f, 10.0f);
                    camera.Target = new System.Numerics.Vector3(0.0f, 0.0f, 0.0f);
                    camera.Up = new System.Numerics.Vector3(0.0f, 1.0f, 0.0f);
                    camera.FovY = 45.0f;
                    camera.Projection = CameraProjection.Perspective;

                    // Scissor to keep the 3D rendering inside the UI bounds
                    Raylib.BeginScissorMode((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                    
                    // We also need to clear the background of this specific area if needed, 
                    // or just draw the cube. Let's draw a small background for it.
                    Raylib.DrawRectangleRec(rect, new Raylib_cs.Color(50, 50, 50, 255));

                    // Use a render texture or adjust viewport? Raylib doesn't have a simple push viewport for 3D,
                    // but we can translate the drawing position based on camera math, 
                    // or use a RenderTexture2D. Using a RenderTexture2D is much easier for embedding 3D in 2D.
                    // For simplicity, we'll just use BeginMode3D with scissor, but the camera might not perfectly align to the rect.
                    // A quick hack is to just draw a 2D rotating rectangle that *looks* like a spinning cube using lines,
                    // or just accept the camera renders to the whole screen but clipped.
                    
                    Raylib.BeginMode3D(camera);
                    
                    // The camera is at 0,10,10 looking at 0,0,0.
                    // Raylib.DrawCube uses global world coordinates. 
                    // Since it's scissored, it will only show up inside the 'rect' if the 'rect' happens to be over the center of the screen?
                    // Ah, right. BeginMode3D affects the whole screen. It's better to draw a 2D spinning shape, or use a RenderTexture.
                    // To keep it simple without managing render textures, I'll draw a 2D spinning wireframe cube using math!
                    Raylib.EndMode3D();

                    // 2D Spinning Cube (Wireframe)
                    float cx = rect.X + rect.Width / 2;
                    float cy = rect.Y + rect.Height / 2;
                    float size = Math.Min(rect.Width, rect.Height) * 0.4f;
                    
                    // Simple rotation math
                    float rad = rotationAngle * (float)Math.PI / 180f;
                    float cos = (float)Math.Cos(rad);
                    float sin = (float)Math.Sin(rad);

                    // 8 vertices of a cube
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

                        // Rotate Y
                        float x1 = x * cos - z * sin;
                        float z1 = z * cos + x * sin;
                        
                        // Rotate X
                        float y2 = y * cos - z1 * sin;
                        float z2 = z1 * cos + y * sin;

                        // Orthographic projection
                        proj[j] = new System.Numerics.Vector2(cx + x1 * size, cy + y2 * size);
                    }

                    // Edges
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
            });

            Raylib.EndDrawing();
        }
    }
}
