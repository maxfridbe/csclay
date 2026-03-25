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
        public LayoutBuilder Sizing(SizingAxis width, SizingAxis height) { Config.Sizing = new Sizing { Width = width, Height = height }; return this; }
        public LayoutBuilder Padding(ushort x, ushort y) { Config.Padding = new Padding(x, y); return this; }
        public LayoutBuilder Padding(ushort left, ushort right, ushort top, ushort bottom) { Config.Padding = new Padding(left, right, top, bottom); return this; }
        public LayoutBuilder ChildGap(ushort gap) { Config.ChildGap = gap; return this; }
        public LayoutBuilder Align(LayoutAlignmentX x, LayoutAlignmentY y) { Config.ChildAlignment = new ChildAlignment { X = x, Y = y }; return this; }
        public LayoutBuilder Direction(LayoutDirection direction) { Config.LayoutDirection = direction; return this; }
    }

    public class TextBuilder
    {
        public TextConfig Config = new();
        public TextBuilder Color(float r, float g, float b, float a = 255) { Config.TextColor = new Color(r, g, b, a); return this; }
        public TextBuilder Size(ushort fontSize) { Config.FontSize = fontSize; return this; }
        public TextBuilder Wrap(TextWrapMode mode) { Config.WrapMode = mode; return this; }
    }

    public static void Container(string id, Action<LayoutBuilder> configBuilder, Action? children = null)
    {
        var builder = new LayoutBuilder();
        configBuilder(builder);
        UI.Container(id, builder.Config, children);
    }

    public static void Container(string id, Action<LayoutBuilder> configBuilder, Color backgroundColor, Action? children = null)
    {
        var builder = new LayoutBuilder();
        configBuilder(builder);
        UI.Container(id, builder.Config, backgroundColor, children);
    }

    public static void Text(string text, Action<TextBuilder> configBuilder)
    {
        var builder = new TextBuilder();
        configBuilder(builder);
        UI.Text(text, builder.Config);
    }
}
