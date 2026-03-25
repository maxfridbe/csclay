using System;
using System.Threading;

namespace CSClay;

public static class UI
{
    private static readonly ThreadLocal<ClayContext> _currentContext = new();

    public static void SetCurrentContext(ClayContext context)
    {
        _currentContext.Value = context;
    }

    public static ClayContext GetCurrentContext() => Context;

    private static ClayContext Context => _currentContext.Value ?? throw new InvalidOperationException("No Clay context set for this thread.");

    public static void Begin(ClayArena arena, Dimensions windowSize)
    {
        if (_currentContext.Value == null)
        {
            _currentContext.Value = new ClayContext(arena);
        }
        _currentContext.Value.BeginLayout(windowSize);
    }

    public static void SetPointerState(Vector2 position, bool isPointerDown)
    {
        Context.SetPointerState(position, isPointerDown);
    }

    public static bool Hovered()
    {
        return Context.IsHovered(Context.GetCurrentElementId());
    }

    public static bool IsHovered(string id)
    {
        return Context.IsHovered(GetElementId(id));
    }

    public static uint GetElementId(string id)
    {
        return HashUtility.HashId(id, Context.GetCurrentElementId());
    }

    public static Span<RenderCommand> End()
    {
        return Context.EndLayout();
    }

    public static void Container(string id, LayoutConfig config, Action? children = null)
    {
        Context.OpenElement(id, ElementType.None);
        Context.ConfigureOpenElement(config);
        
        children?.Invoke();
        
        Context.CloseElement();
    }

    public static void FloatingContainer(string id, LayoutConfig layoutConfig, FloatingConfig floatingConfig, Action? children = null)
    {
        Context.OpenElement(id, ElementType.None);
        Context.ConfigureOpenElement(layoutConfig);
        Context.ConfigureFloatingElement(floatingConfig);

        children?.Invoke();

        Context.CloseElement();
    }

    public static void FloatingContainer(string id, LayoutConfig layoutConfig, FloatingConfig floatingConfig, Color backgroundColor, Action? children = null)
    {
        Context.OpenElement(id, ElementType.None);
        Context.ConfigureOpenElement(layoutConfig);
        Context.ConfigureFloatingElement(floatingConfig);
        Context.ConfigureRectangleElement(new RectangleConfig { Color = backgroundColor });

        children?.Invoke();

        Context.CloseElement();
    }

    public static void Container(string id, LayoutConfig layoutConfig, Color backgroundColor, Action? children = null)
    {
        Context.OpenElement(id, ElementType.None);
        Context.ConfigureOpenElement(layoutConfig);
        Context.ConfigureRectangleElement(new RectangleConfig { Color = backgroundColor });

        children?.Invoke();

        Context.CloseElement();
    }

    public static void ScrollContainer(string id, LayoutConfig layoutConfig, ClipConfig clipConfig, Action? children = null)
    {
        Context.OpenElement(id, ElementType.None);
        Context.ConfigureOpenElement(layoutConfig);
        Context.ConfigureClipElement(clipConfig);

        children?.Invoke();

        Context.CloseElement();
    }

    public static void ScrollContainer(string id, LayoutConfig layoutConfig, ClipConfig clipConfig, Color backgroundColor, Action? children = null)
    {
        Context.OpenElement(id, ElementType.None);
        Context.ConfigureOpenElement(layoutConfig);
        Context.ConfigureClipElement(clipConfig);
        Context.ConfigureRectangleElement(new RectangleConfig { Color = backgroundColor });

        children?.Invoke();

        Context.CloseElement();
    }

    public static void Text(string text, TextConfig config)
    {
        Context.AddTextElement(text, config);
    }

    public static void Image(string id, ImageConfig config)
    {
        Context.OpenElement(id, ElementType.Image);
        Context.ConfigureImageElement(config);
        Context.CloseElement();
    }

    public static void Image(string id, LayoutConfig layoutConfig, ImageConfig imageConfig)
    {
        Context.OpenElement(id, ElementType.Image);
        Context.ConfigureOpenElement(layoutConfig);
        Context.ConfigureImageElement(imageConfig);
        Context.CloseElement();
    }

    public static void Custom(string id, CustomConfig config)
    {
        Context.OpenElement(id, ElementType.Custom);
        Context.ConfigureCustomElement(config);
        Context.CloseElement();
    }

    public static void Custom(string id, LayoutConfig layoutConfig, CustomConfig customConfig)
    {
        Context.OpenElement(id, ElementType.Custom);
        Context.ConfigureOpenElement(layoutConfig);
        Context.ConfigureCustomElement(customConfig);
        Context.CloseElement();
    }
}
