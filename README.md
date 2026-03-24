# csclay (Clay C# Port)

[![Build Status](https://github.com/maxfridbe/csclay/actions/workflows/dotnet.yml/badge.svg)](https://github.com/maxfridbe/csclay/actions)
[![NuGet Version](https://img.shields.io/nuget/v/csclay.svg)](https://www.nuget.org/packages/csclay)

**csclay** is a high-performance, single-file (conceptually) 2D UI layout library for C#, ported from the original [Clay](https://github.com/nicbarker/clay) C library. It provides microsecond layout performance and a declarative, flexbox-like model with **zero garbage collection allocations** in the core layout loop.

---

## 🚀 Key Features

- **Microsecond Performance:** High-speed layout engine suitable for real-time applications and games.
- **Zero GC Allocations:** Core layout loop uses a managed arena (`byte[]`) and `Span<T>` to avoid heap allocations.
- **Pure C#:** No `unsafe` blocks, no raw pointers, and no unmanaged dependencies.
- **Declarative Syntax:** React-like nested syntax using C# Action delegates (Lambdas).
- **Flex-box Model:** Supports complex responsive layouts, including `Grow`, `Fixed`, `Percent`, and `Fit` sizing rules.
- **Advanced Features:** Word wrapping text, scrolling containers with clipping, floating elements (tooltips/modals), and Z-index sorting.
- **Renderer Agnostic:** Outputs a flat, sorted array of render commands (Rectangle, Text, Image, Scissor) ready for any engine (Raylib-cs, MonoGame, Unity, SkiaSharp, etc.).

---

## 🧠 Philosophy

Like the original Clay, **csclay** treats UI layout as a pure calculation:
1. **Input:** A hierarchy of elements and their constraints (LayoutConfig).
2. **Process:** A multi-pass calculation (sizing along axes, text wrapping, and positioning).
3. **Output:** A list of simple render commands.

It doesn't handle windowing, input events, or GPU rendering—it simply tells your renderer exactly where everything should go.

---

## 🛠 Quick Start

### 1. Initialization
Initialize the `ClayArena` and `ClayContext` with your screen dimensions.

```csharp
using Clay;

// Pre-allocate 4MB for the layout arena
var arena = new ClayArena(1024 * 1024 * 4);
var context = new ClayContext(arena);
UI.SetCurrentContext(context);

// Provide a text measurement callback
context.TextMeasure = (ReadOnlySpan<char> text, TextConfig config) => {
    // Return dimensions based on your font/renderer
    return new Dimensions(text.Length * 10, 20); 
};
```

### 2. Declare Your Layout
Use the declarative `UI` API in your update/render loop.

```csharp
UI.Begin(arena, new Dimensions(800, 600));

UI.Container("root", new LayoutConfig { 
    LayoutDirection = LayoutDirection.LeftToRight,
    Padding = new Padding(20, 20),
    ChildGap = 10,
    Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() }
}, new Color(40, 44, 52), () => 
{
    // Sidebar
    UI.Container("sidebar", new LayoutConfig { 
        Sizing = new Sizing { Width = SizingAxis.Fixed(200), Height = SizingAxis.Grow() }
    }, new Color(60, 64, 72), () => 
    {
        UI.Text("Sidebar Item", new TextConfig { FontSize = 18, TextColor = new Color(255, 255, 255) });
    });

    // Main Content
    UI.Container("content", new LayoutConfig { 
        Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() }
    }, () => 
    {
        UI.Text("Welcome to csclay!", new TextConfig { FontSize = 24, TextColor = new Color(255, 255, 255) });
    });
});

Span<RenderCommand> commands = UI.End();
```

### 3. Render
Iterate through the generated commands and call your drawing API.

```csharp
foreach (var cmd in commands)
{
    switch (cmd.CommandType)
    {
        case RenderCommandType.Rectangle:
            DrawRect(cmd.BoundingBox, cmd.RenderData.Rectangle.Color);
            break;
        case RenderCommandType.Text:
            DrawText(cmd.BoundingBox, cmd.RenderData.Text);
            break;
        case RenderCommandType.ScissorStart:
            BeginScissor(cmd.BoundingBox);
            break;
        case RenderCommandType.ScissorEnd:
            EndScissor();
            break;
    }
}
```

---

## 📂 Project Structure

- **`src/Clay/`**: The core library implementation.
- **`examples/Clay.Demo/`**: A visual demonstration using [Raylib-cs](https://github.com/ChrisDill/Raylib-cs).
- **`tests/Clay.Tests/`**: xUnit tests covering sizing, wrapping, and interaction.

---

## 🤝 Contributing

This is a port of the original C library by [Nic Barker](https://github.com/nicbarker). 

We welcome contributions! If you find a bug, want to improve performance, or add examples for other C# renderers, please feel free to open an issue or submit a pull request.

## 📄 License

Original Clay library is licensed under zlib License. This port preserves that spirit.
