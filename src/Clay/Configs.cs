using System;

namespace Clay;

public struct LayoutConfig
{
    public Sizing Sizing;
    public Padding Padding;
    public ushort ChildGap;
    public ChildAlignment ChildAlignment;
    public LayoutDirection LayoutDirection;
}

public struct CornerRadius
{
    public float TopLeft, TopRight, BottomLeft, BottomRight;

    public CornerRadius(float radius)
    {
        TopLeft = TopRight = BottomLeft = BottomRight = radius;
    }
}

public struct BorderConfig
{
    public ushort Left, Right, Top, Bottom, BetweenChildren;
    public Color Color;
}

public struct RectangleConfig
{
    public Color Color;
    public CornerRadius CornerRadius;
}

public struct TextAlignment
{
    public LayoutAlignmentX X;
    public LayoutAlignmentY Y;
}

public enum TextWrapMode : byte
{
    Words,
    Characters,
    None
}

public struct TextConfig
{
    public Color TextColor;
    public ushort FontId;
    public ushort FontSize;
    public ushort LetterSpacing;
    public ushort LineHeight;
    public TextAlignment TextAlignment;
    public TextWrapMode WrapMode;
}

public struct ImageConfig
{
    public Dimensions Dimensions;
    public uint ImageDataId; // In a real port, this might be an object or pointer
    public Color BackgroundColor;
    public CornerRadius CornerRadius;
}

public struct CustomConfig
{
    public uint CustomDataId;
}

public enum FloatingAttachPoint : byte
{
    LeftTop,
    LeftCenter,
    LeftBottom,
    CenterTop,
    CenterCenter,
    CenterBottom,
    RightTop,
    RightCenter,
    RightBottom
}

public struct FloatingAttachPoints
{
    public FloatingAttachPoint Element;
    public FloatingAttachPoint Parent;
}

public enum FloatingAttachToElement : byte
{
    Parent,
    ElementWithId,
    Root
}

public struct FloatingConfig
{
    public Vector2 Offset;
    public Dimensions Expand;
    public int ZIndex;
    public uint ParentId;
    public FloatingAttachPoints AttachPoints;
    public FloatingAttachToElement AttachTo;
}

public struct ClipConfig
{
    public Vector2 ChildOffset;
    public bool Horizontal;
    public bool Vertical;
}

public enum ElementType : byte
{
    None,
    Rectangle,
    Text,
    Image,
    Scissor,
    Custom,
    Border,
    Floating
}
