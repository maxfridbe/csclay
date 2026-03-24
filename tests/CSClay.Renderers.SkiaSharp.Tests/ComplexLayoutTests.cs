using Clay;
using SkiaSharp;
using CSClay.Renderers.SkiaSharp;
using Xunit;

namespace CSClay.Renderers.SkiaSharp.Tests;

public class ComplexLayoutTests
{
    private static readonly Color COLOR_BACKGROUND = new(40, 44, 52);
    private static readonly Color COLOR_HEADER = new(28, 32, 38);
    private static readonly Color COLOR_ACCENT = new(100, 150, 255);
    private static readonly Color COLOR_TEXT = new(255, 255, 255);
    private static readonly Color COLOR_TEXT_DIM = new(180, 185, 190);

    [Fact]
    public void TestWebsiteLandingPage_Desktop()
    {
        RenderLayout(1200, 800, "website_desktop.png");
    }

    [Fact]
    public void TestWebsiteLandingPage_Mobile()
    {
        RenderLayout(400, 800, "website_mobile.png");
    }

    private void RenderLayout(int width, int height, string filename)
    {
        var arena = new ClayArena(1024 * 1024 * 10);
        var context = new ClayContext(arena);
        UI.SetCurrentContext(context);

        context.TextMeasure = (text, config) => 
        {
            // Approximate text measurement
            float w = text.Length * (config.FontSize * 0.55f);
            return new Dimensions(w, config.FontSize * 1.2f);
        };

        UI.Begin(arena, new Dimensions(width, height));

        bool isMobile = width < 600;

        UI.Container("root", new LayoutConfig 
        { 
            LayoutDirection = LayoutDirection.TopToBottom,
            Sizing = new Sizing { Width = SizingAxis.Fixed(width), Height = SizingAxis.Fixed(height) }
        }, COLOR_BACKGROUND, () => 
        {
            // 1. HEADER
            UI.Container("header", new LayoutConfig 
            { 
                Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fixed(80) },
                Padding = new Padding(40, 0),
                LayoutDirection = LayoutDirection.LeftToRight,
                ChildAlignment = new ChildAlignment { X = LayoutAlignmentX.Left, Y = LayoutAlignmentY.Center },
                ChildGap = 20
            }, COLOR_HEADER, () => 
            {
                UI.Text("CLAY C#", new TextConfig { FontSize = 28, TextColor = COLOR_ACCENT });
                
                if (!isMobile)
                {
                    UI.Container("nav", new LayoutConfig 
                    { 
                        LayoutDirection = LayoutDirection.LeftToRight, 
                        ChildGap = 30,
                        Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fit() },
                        ChildAlignment = new ChildAlignment { X = LayoutAlignmentX.Right, Y = LayoutAlignmentY.Center }
                    }, () => 
                    {
                        UI.Text("Documentation", new TextConfig { FontSize = 16, TextColor = COLOR_TEXT });
                        UI.Text("Examples", new TextConfig { FontSize = 16, TextColor = COLOR_TEXT });
                        UI.Text("GitHub", new TextConfig { FontSize = 16, TextColor = COLOR_TEXT });
                    });
                }
            });

            // 2. HERO SECTION
            UI.Container("hero", new LayoutConfig 
            { 
                LayoutDirection = isMobile ? LayoutDirection.TopToBottom : LayoutDirection.LeftToRight,
                Padding = new Padding(60, 60),
                ChildGap = 40,
                Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fit() }
            }, () => 
            {
                // Left Column (Text)
                UI.Container("hero-text", new LayoutConfig 
                { 
                    LayoutDirection = LayoutDirection.TopToBottom,
                    ChildGap = 20,
                    Sizing = new Sizing { Width = isMobile ? SizingAxis.Grow() : SizingAxis.Percent(0.6f), Height = SizingAxis.Fit() }
                }, () => 
                {
                    UI.Text("High performance 2D UI layout library in pure C#.", 
                        new TextConfig { FontSize = 48, TextColor = COLOR_TEXT, WrapMode = TextWrapMode.Words });
                    
                    UI.Text("csclay provides a microsecond layout engine with zero GC allocations in the core loop. It's safe, managed, and renderer-agnostic.", 
                        new TextConfig { FontSize = 20, TextColor = COLOR_TEXT_DIM, WrapMode = TextWrapMode.Words });

                    UI.Container("cta-row", new LayoutConfig { LayoutDirection = LayoutDirection.LeftToRight, ChildGap = 16 }, () => 
                    {
                        UI.Container("btn-primary", new LayoutConfig { Padding = new Padding(24, 12) }, COLOR_ACCENT, () => 
                            UI.Text("Get Started", new TextConfig { FontSize = 18, TextColor = COLOR_BACKGROUND }));
                        
                        UI.Container("btn-secondary", new LayoutConfig { Padding = new Padding(24, 12) }, new Color(60, 64, 72), () => 
                            UI.Text("View Source", new TextConfig { FontSize = 18, TextColor = COLOR_TEXT }));
                    });
                });

                // Right Column (Placeholder Image/Graphic)
                UI.Container("hero-graphic", new LayoutConfig 
                { 
                    Sizing = new Sizing { Width = isMobile ? SizingAxis.Grow() : SizingAxis.Percent(0.4f), Height = SizingAxis.Fixed(isMobile ? 200 : 300) },
                }, new Color(50, 54, 62), () => 
                {
                    UI.Container("box", new LayoutConfig 
                    { 
                        Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() },
                        Padding = new Padding(40, 40)
                    }, () => 
                    {
                        UI.Container("inner-graphic", new LayoutConfig 
                        { 
                            Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() }
                        }, COLOR_ACCENT);
                    });
                });
            });

            // 3. FEATURES GRID
            UI.Container("features", new LayoutConfig 
            { 
                LayoutDirection = LayoutDirection.TopToBottom,
                Padding = new Padding(60, 20),
                ChildGap = 30,
                Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() }
            }, () => 
            {
                UI.Text("Features", new TextConfig { FontSize = 32, TextColor = COLOR_TEXT });

                UI.Container("grid", new LayoutConfig 
                { 
                    LayoutDirection = isMobile ? LayoutDirection.TopToBottom : LayoutDirection.LeftToRight,
                    ChildGap = 20,
                    Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fit() }
                }, () => 
                {
                    FeatureCard("Zero GC", "No heap allocations in the hot path.");
                    FeatureCard("Responsive", "Easy desktop and mobile layouts.");
                    FeatureCard("Fast", "Microsecond layout calculations.");
                });
            });
        });

        var commands = UI.End();

        // Render to image
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);
        SkiaSharpRenderer.Render(canvas, commands, context);

        var artifactsDir = Path.Combine(Directory.GetCurrentDirectory(), "TestOutput");
        if (!Directory.Exists(artifactsDir)) Directory.CreateDirectory(artifactsDir);
        var filePath = Path.Combine(artifactsDir, filename);
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }

    private void FeatureCard(string title, string desc)
    {
        UI.Container(title, new LayoutConfig 
        { 
            LayoutDirection = LayoutDirection.TopToBottom,
            Padding = new Padding(24, 24),
            ChildGap = 12,
            Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fit() }
        }, new Color(50, 54, 62), () => 
        {
            UI.Text(title, new TextConfig { FontSize = 20, TextColor = COLOR_ACCENT });
            UI.Text(desc, new TextConfig { FontSize = 16, TextColor = COLOR_TEXT_DIM, WrapMode = TextWrapMode.Words });
        });
    }
}
