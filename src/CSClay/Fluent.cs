using System;

namespace CSClay.Fluent;

public static class Clay
{
    // LayoutConfig Extensions
    public static LayoutConfig Sizing(this LayoutConfig config, SizingAxis width, SizingAxis height)
    {
        config.Sizing = new Sizing { Width = width, Height = height };
        return config;
    }

    public static LayoutConfig Padding(this LayoutConfig config, ushort x, ushort y)
    {
        config.Padding = new Padding(x, y);
        return config;
    }

    public static LayoutConfig Padding(this LayoutConfig config, ushort left, ushort right, ushort top, ushort bottom)
    {
        config.Padding = new Padding(left, right, top, bottom);
        return config;
    }

    public static LayoutConfig ChildGap(this LayoutConfig config, ushort gap)
    {
        config.ChildGap = gap;
        return config;
    }

    public static LayoutConfig Align(this LayoutConfig config, LayoutAlignmentX x, LayoutAlignmentY y)
    {
        config.ChildAlignment = new ChildAlignment { X = x, Y = y };
        return config;
    }

    public static LayoutConfig Direction(this LayoutConfig config, LayoutDirection direction)
    {
        config.LayoutDirection = direction;
        return config;
    }

    // Sizing Builders (to avoid SizingAxis.Grow() etc)
    public static SizingAxis Grow(float min = 0, float max = float.MaxValue) => SizingAxis.Grow(min, max);
    public static SizingAxis Fit(float min = 0, float max = float.MaxValue) => SizingAxis.Fit(min, max);
    public static SizingAxis Fixed(float value) => SizingAxis.Fixed(value);
    public static SizingAxis Percent(float percent) => SizingAxis.Percent(percent);

    // TextConfig Extensions
    public static TextConfig Color(this TextConfig config, float r, float g, float b, float a = 255)
    {
        config.TextColor = new Color(r, g, b, a);
        return config;
    }

    public static TextConfig Size(this TextConfig config, ushort fontSize)
    {
        config.FontSize = fontSize;
        return config;
    }

    public static TextConfig Wrap(this TextConfig config, TextWrapMode mode)
    {
        config.WrapMode = mode;
        return config;
    }

    // UI Fluent Wrappers
    public class LayoutBuilder
    {
        public LayoutConfig Config = new();
        public Color? BackgroundColor { get; private set; }
        public CornerRadius CornerRadiusData { get; private set; }
        public FloatingConfig? FloatingConfig { get; private set; }
        public ClipConfig? ClipConfig { get; private set; }

        public LayoutBuilder Sizing(SizingAxis width, SizingAxis height) { Config.Sizing = new Sizing { Width = width, Height = height }; return this; }
        public LayoutBuilder Padding(ushort x, ushort y) { Config.Padding = new Padding(x, y); return this; }
        public LayoutBuilder Padding(ushort left, ushort right, ushort top, ushort bottom) { Config.Padding = new Padding(left, right, top, bottom); return this; }
        public LayoutBuilder ChildGap(ushort gap) { Config.ChildGap = gap; return this; }
        public LayoutBuilder Align(LayoutAlignmentX x, LayoutAlignmentY y) { Config.ChildAlignment = new ChildAlignment { X = x, Y = y }; return this; }
        public LayoutBuilder Direction(LayoutDirection direction) { Config.LayoutDirection = direction; return this; }
        public LayoutBuilder Color(float r, float g, float b, float a = 255) { BackgroundColor = new CSClay.Color(r, g, b, a); return this; }
        public LayoutBuilder Color(CSClay.Color color) { BackgroundColor = color; return this; }
        
        // CSS-like border-radius syntax
        public LayoutBuilder CornerRadius(float radius) 
        { 
            CornerRadiusData = new CSClay.CornerRadius { TopLeft = radius, TopRight = radius, BottomRight = radius, BottomLeft = radius }; 
            return this; 
        }
        
        public LayoutBuilder CornerRadius(float topLeftAndBottomRight, float topRightAndBottomLeft) 
        { 
            CornerRadiusData = new CSClay.CornerRadius { TopLeft = topLeftAndBottomRight, TopRight = topRightAndBottomLeft, BottomRight = topLeftAndBottomRight, BottomLeft = topRightAndBottomLeft }; 
            return this; 
        }
        
        public LayoutBuilder CornerRadius(float topLeft, float topRightAndBottomLeft, float bottomRight) 
        { 
            CornerRadiusData = new CSClay.CornerRadius { TopLeft = topLeft, TopRight = topRightAndBottomLeft, BottomRight = bottomRight, BottomLeft = topRightAndBottomLeft }; 
            return this; 
        }

        public LayoutBuilder CornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft) 
        { 
            CornerRadiusData = new CSClay.CornerRadius { TopLeft = topLeft, TopRight = topRight, BottomRight = bottomRight, BottomLeft = bottomLeft }; 
            return this; 
        }
        
        public LayoutBuilder Floating(Action<FloatingBuilder> floatingBuilder)
        {
            var f = new FloatingBuilder();
            floatingBuilder(f);
            FloatingConfig = f.Config;
            return this;
        }

        public LayoutBuilder Scroll(Action<ClipBuilder> clipBuilder)
        {
            var c = new ClipBuilder();
            clipBuilder(c);
            ClipConfig = c.Config;
            return this;
        }
    }

    public class TextBuilder
    {
        public TextConfig Config = new();
        public TextBuilder Color(float r, float g, float b, float a = 255) { Config.TextColor = new CSClay.Color(r, g, b, a); return this; }
        public TextBuilder Color(CSClay.Color color) { Config.TextColor = color; return this; }
        public TextBuilder Size(ushort fontSize) { Config.FontSize = fontSize; return this; }
        public TextBuilder Wrap(TextWrapMode mode) { Config.WrapMode = mode; return this; }
    }

    public class FloatingBuilder
    {
        public FloatingConfig Config = new();
        public FloatingBuilder Offset(float x, float y) { Config.Offset = new Vector2(x, y); return this; }
        public FloatingBuilder ZIndex(int z) { Config.ZIndex = z; return this; }
        public FloatingBuilder Attach(FloatingAttachPoint element, FloatingAttachPoint parent, FloatingAttachToElement to = FloatingAttachToElement.Parent) 
        { 
            Config.AttachPoints = new FloatingAttachPoints { Element = element, Parent = parent }; 
            Config.AttachTo = to;
            return this; 
        }
    }

    public class ClipBuilder
    {
        public ClipConfig Config = new();
        public ClipBuilder Horizontal(bool clip = true) { Config.Horizontal = clip; return this; }
        public ClipBuilder Vertical(bool clip = true) { Config.Vertical = clip; return this; }
        public ClipBuilder Offset(float x, float y) { Config.ChildOffset = new Vector2(x, y); return this; }
    }

    public static void Container(string id, Action<LayoutBuilder> configBuilder, Action? children = null)
    {
        var builder = new LayoutBuilder();
        configBuilder(builder);

        var context = UI.GetCurrentContext();
        context.OpenElement(id, ElementType.None);
        context.ConfigureOpenElement(builder.Config);
        
        if (builder.BackgroundColor.HasValue || builder.CornerRadiusData.TopLeft > 0 || builder.CornerRadiusData.TopRight > 0 || builder.CornerRadiusData.BottomLeft > 0 || builder.CornerRadiusData.BottomRight > 0)
        {
            context.ConfigureRectangleElement(new RectangleConfig { Color = builder.BackgroundColor ?? new Color(0,0,0,0), CornerRadius = builder.CornerRadiusData });
        }
        
        if (builder.FloatingConfig.HasValue)
        {
            context.ConfigureFloatingElement(builder.FloatingConfig.Value);
        }
        
        if (builder.ClipConfig.HasValue)
        {
            context.ConfigureClipElement(builder.ClipConfig.Value);
        }

        children?.Invoke();

        context.CloseElement();
    }

    public static void Text(string text, Action<TextBuilder> configBuilder)
    {
        var builder = new TextBuilder();
        configBuilder(builder);
        UI.Text(text, builder.Config);
    }

    public static void Custom(string id, Action<LayoutBuilder> configBuilder, uint customDataId = 0)
    {
        var builder = new LayoutBuilder();
        configBuilder(builder);

        var context = UI.GetCurrentContext();
        context.OpenElement(id, ElementType.Custom);
        context.ConfigureOpenElement(builder.Config);
        context.ConfigureCustomElement(new CustomConfig { CustomDataId = customDataId });

        if (builder.BackgroundColor.HasValue || builder.CornerRadiusData.TopLeft > 0 || builder.CornerRadiusData.TopRight > 0 || builder.CornerRadiusData.BottomLeft > 0 || builder.CornerRadiusData.BottomRight > 0)
        {
            context.ConfigureRectangleElement(new RectangleConfig { Color = builder.BackgroundColor ?? new Color(0,0,0,0), CornerRadius = builder.CornerRadiusData });
        }

        context.CloseElement();
    }
}
