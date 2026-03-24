using System;
using System.Runtime.InteropServices;

namespace Clay;

public struct Color
{
    public float R, G, B, A;

    public Color(float r, float g, float b, float a = 255.0f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}

public struct Dimensions
{
    public float Width, Height;

    public Dimensions(float width, float height)
    {
        Width = width;
        Height = height;
    }
}

public struct Vector2
{
    public float X, Y;

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
}

public struct BoundingBox
{
    public float X, Y, Width, Height;

    public BoundingBox(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

public enum LayoutDirection : byte
{
    LeftToRight,
    TopToBottom
}

public enum LayoutAlignmentX : byte
{
    Left,
    Center,
    Right
}

public enum LayoutAlignmentY : byte
{
    Top,
    Center,
    Bottom
}

public struct ChildAlignment
{
    public LayoutAlignmentX X;
    public LayoutAlignmentY Y;
}

public enum SizingType : byte
{
    Fit,
    Grow,
    Percent,
    Fixed
}

public struct SizingMinMax
{
    public float Min;
    public float Max;
}

[StructLayout(LayoutKind.Explicit)]
public struct SizingValue
{
    [FieldOffset(0)] public SizingMinMax MinMax;
    [FieldOffset(0)] public float Percent;
}

public struct SizingAxis
{
    public SizingValue Size;
    public SizingType Type;

    public static SizingAxis Fit(float min = 0, float max = float.MaxValue) => new SizingAxis { Type = SizingType.Fit, Size = new SizingValue { MinMax = new SizingMinMax { Min = min, Max = max } } };
    public static SizingAxis Grow(float min = 0, float max = float.MaxValue) => new SizingAxis { Type = SizingType.Grow, Size = new SizingValue { MinMax = new SizingMinMax { Min = min, Max = max } } };
    public static SizingAxis Fixed(float value) => new SizingAxis { Type = SizingType.Fixed, Size = new SizingValue { MinMax = new SizingMinMax { Min = value, Max = value } } };
    public static SizingAxis Percent(float percent) => new SizingAxis { Type = SizingType.Percent, Size = new SizingValue { Percent = percent } };
}

public struct Sizing
{
    public SizingAxis Width;
    public SizingAxis Height;
}

public struct Padding
{
    public ushort Left, Right, Top, Bottom;

    public Padding(ushort x, ushort y)
    {
        Left = Right = x;
        Top = Bottom = y;
    }

    public Padding(ushort left, ushort right, ushort top, ushort bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }
}

public struct LayoutElement
{
    public uint Id;
    public Dimensions Dimensions; // Actual calculated dimensions
    public Dimensions MinDimensions;
    public Dimensions PreferredDimensions;
    public BoundingBox BoundingBox;
    public LayoutConfig Config;
    public int ChildrenStartIndex;
    public int ChildrenCount;
    public ElementType ElementType;
    public int ConfigIndex; // Base/Floating config index
    public int RectangleConfigIndex;
    public int BorderConfigIndex;
    public int ImageConfigIndex;
}

public struct LayoutElementTreeRoot
{
    public int LayoutElementIndex;
    public uint ParentId;
    public int ClipElementId;
    public int ZIndex;
}

public enum RenderCommandType : byte
{
    None,
    Rectangle,
    Text,
    Image,
    ScissorStart,
    ScissorEnd,
    Custom
}

public struct RectangleRenderData
{
    public Color Color;
    public CornerRadius CornerRadius;
}

public struct TextRenderData
{
    public Color TextColor;
    public ushort FontId;
    public ushort FontSize;
    public int TextIndex;
    public int LineStart;
    public int LineLength;
}

[StructLayout(LayoutKind.Explicit)]
public struct RenderData
{
    [FieldOffset(0)] public RectangleRenderData Rectangle;
    [FieldOffset(0)] public TextRenderData Text;
}

public struct RenderCommand
{
    public BoundingBox BoundingBox;
    public RenderData RenderData;
    public uint Id;
    public int ZIndex;
    public RenderCommandType CommandType;
}

public struct LayoutElementTreeNode
{
    public int LayoutElementIndex;
    public Vector2 Position;
    public Vector2 NextChildOffset;
}

public struct WrappedTextLine
{
    public Dimensions Dimensions;
    public int StartOffset;
    public int Length;
}

public struct LayoutElementHashMapItem
{
    public uint Id;
    public BoundingBox BoundingBox;
}

public struct TextElementData
{
    public int TextIndex; // Index into a managed string list
    public TextConfig Config;
    public int ElementIndex;
    public int WrappedLinesStartIndex;
    public int WrappedLinesCount;
}
