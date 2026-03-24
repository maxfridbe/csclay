using System;
using System.Collections.Generic;

namespace Clay;

public delegate Dimensions TextMeasureDelegate(ReadOnlySpan<char> text, TextConfig config);

public class ClayContext
{
    private readonly ClayArena _arena;
    private uint _currentParentId;
    private Dimensions _layoutDimensions;
    
    public TextMeasureDelegate? TextMeasure { get; set; }

    // Managed string storage (resets every frame)
    private readonly List<string> _strings = new();

    // Memory offsets
    private int _layoutElementsOffset;
    private int _layoutElementsCapacity;
    private int _layoutElementsCount;
    
    private int _layoutElementChildrenOffset;
    private int _layoutElementChildrenCapacity;
    private int _layoutElementChildrenCount;

    private int _openLayoutElementStackOffset;
    private int _openLayoutElementStackCapacity;
    private int _openLayoutElementStackCount;

    private int _layoutElementTreeRootsOffset;
    private int _layoutElementTreeRootsCapacity;
    private int _layoutElementTreeRootsCount;

    private int _bfsBufferOffset;
    private int _bfsBufferCapacity;

    private int _renderCommandsOffset;
    private int _renderCommandsCapacity;
    private int _renderCommandsCount;

    private int _dfsBufferOffset;
    private int _dfsBufferCapacity;

    private int _treeNodeVisitedOffset;
    private int _treeNodeVisitedCapacity;

    private int _textElementDataOffset;
    private int _textElementDataCapacity;
    private int _textElementDataCount;

    private int _wrappedTextLinesOffset;
    private int _wrappedTextLinesCapacity;
    private int _wrappedTextLinesCount;

    private int _floatingConfigsOffset;
    private int _floatingConfigsCapacity;
    private int _floatingConfigsCount;

    private int _clipConfigsOffset;
    private int _clipConfigsCapacity;
    private int _clipConfigsCount;

    private int _rectangleConfigsOffset;
    private int _rectangleConfigsCapacity;
    private int _rectangleConfigsCount;

    private int _borderConfigsOffset;
    private int _borderConfigsCapacity;
    private int _borderConfigsCount;

    private int _imageConfigsOffset;
    private int _imageConfigsCapacity;
    private int _imageConfigsCount;

    private int _customConfigsOffset;
    private int _customConfigsCapacity;
    private int _customConfigsCount;

    private readonly Dictionary<uint, int> _idToElementIndex = new();

    // Persistent state across frames
    private readonly Dictionary<uint, LayoutElementHashMapItem> _persistentElementState = new();
    
    private Vector2 _pointerPosition;
    private bool _isPointerDown;
    private readonly List<uint> _pointerOverIds = new();

    public ClayContext(ClayArena arena)
    {
        _arena = arena;
    }

    public void SetPointerState(Vector2 position, bool isPointerDown)
    {
        _pointerPosition = position;
        _isPointerDown = isPointerDown;
        
        _pointerOverIds.Clear();
        // Check which elements the pointer is over based on PREVIOUS frame's bounding boxes
        foreach (var item in _persistentElementState.Values)
        {
            if (IsPointInside(position, item.BoundingBox))
            {
                _pointerOverIds.Add(item.Id);
            }
        }
    }

    public bool IsHovered(uint id)
    {
        return _pointerOverIds.Contains(id);
    }

    internal uint GetCurrentElementId() => _currentParentId;

    private static bool IsPointInside(Vector2 point, BoundingBox rect)
    {
        return point.X >= rect.X && point.X <= rect.X + rect.Width &&
               point.Y >= rect.Y && point.Y <= rect.Y + rect.Height;
    }

    public void BeginLayout(Dimensions windowSize, int maxElements = 8192, int maxChildren = 16384)
    {
        _arena.Reset();
        _strings.Clear();
        _idToElementIndex.Clear();
        _currentParentId = 0;
        _layoutDimensions = windowSize;

        _layoutElementsCapacity = maxElements;
        _layoutElementsOffset = _arena.Allocate<LayoutElement>(maxElements);
        _layoutElementsCount = 0;

        _layoutElementChildrenCapacity = maxChildren;
        _layoutElementChildrenOffset = _arena.Allocate<int>(maxChildren);
        _layoutElementChildrenCount = 0;

        _openLayoutElementStackCapacity = 256;
        _openLayoutElementStackOffset = _arena.Allocate<int>(256);
        _openLayoutElementStackCount = 0;

        _layoutElementTreeRootsCapacity = 1024;
        _layoutElementTreeRootsOffset = _arena.Allocate<LayoutElementTreeRoot>(1024);
        _layoutElementTreeRootsCount = 0;

        _bfsBufferCapacity = maxElements;
        _bfsBufferOffset = _arena.Allocate<int>(maxElements);

        _renderCommandsCapacity = maxElements;
        _renderCommandsOffset = _arena.Allocate<RenderCommand>(maxElements);
        _renderCommandsCount = 0;

        _dfsBufferCapacity = 1024;
        _dfsBufferOffset = _arena.Allocate<LayoutElementTreeNode>(1024);

        _treeNodeVisitedCapacity = maxElements;
        _treeNodeVisitedOffset = _arena.Allocate<bool>(maxElements);

        _textElementDataCapacity = maxElements;
        _textElementDataOffset = _arena.Allocate<TextElementData>(maxElements);
        _textElementDataCount = 0;
_wrappedTextLinesCapacity = maxElements * 4;
_wrappedTextLinesOffset = _arena.Allocate<WrappedTextLine>(maxElements * 4);
_wrappedTextLinesCount = 0;

_floatingConfigsCapacity = 1024;
_floatingConfigsOffset = _arena.Allocate<FloatingConfig>(1024);
_floatingConfigsCount = 0;

_clipConfigsCapacity = 1024;
_clipConfigsOffset = _arena.Allocate<ClipConfig>(1024);
_clipConfigsCount = 0;

_rectangleConfigsCapacity = 1024;
_rectangleConfigsOffset = _arena.Allocate<RectangleConfig>(1024);
_rectangleConfigsCount = 0;

_borderConfigsCapacity = 1024;
_borderConfigsOffset = _arena.Allocate<BorderConfig>(1024);
_borderConfigsCount = 0;

_imageConfigsCapacity = 1024;
_imageConfigsOffset = _arena.Allocate<ImageConfig>(1024);
_imageConfigsCount = 0;

_customConfigsCapacity = 1024;
_customConfigsOffset = _arena.Allocate<CustomConfig>(1024);
_customConfigsCount = 0;

OpenElement("Clay__RootContainer", ElementType.None);

ConfigureOpenElement(new LayoutConfig 
{ 
    Sizing = new Sizing 
    { 
        Width = SizingAxis.Fixed(windowSize.Width), 
        Height = SizingAxis.Fixed(windowSize.Height) 
    } 
});
ConfigureRectangleElement(new RectangleConfig { Color = new Color(0, 0, 0, 0) });

var roots = GetLayoutElementTreeRoots();        roots[_layoutElementTreeRootsCount++] = new LayoutElementTreeRoot { LayoutElementIndex = 0 };
    }

    private Span<LayoutElement> GetLayoutElements() => _arena.GetSpan<LayoutElement>(_layoutElementsOffset, _layoutElementsCapacity);
    private Span<int> GetLayoutElementChildren() => _arena.GetSpan<int>(_layoutElementChildrenOffset, _layoutElementChildrenCapacity);
    private Span<int> GetOpenLayoutElementStack() => _arena.GetSpan<int>(_openLayoutElementStackOffset, _openLayoutElementStackCapacity);
    private Span<LayoutElementTreeRoot> GetLayoutElementTreeRoots() => _arena.GetSpan<LayoutElementTreeRoot>(_layoutElementTreeRootsOffset, _layoutElementTreeRootsCapacity);
    private Span<int> GetBfsBuffer() => _arena.GetSpan<int>(_bfsBufferOffset, _bfsBufferCapacity);
    private Span<RenderCommand> GetRenderCommands() => _arena.GetSpan<RenderCommand>(_renderCommandsOffset, _renderCommandsCapacity);
    private Span<LayoutElementTreeNode> GetDfsBuffer() => _arena.GetSpan<LayoutElementTreeNode>(_dfsBufferOffset, _dfsBufferCapacity);
    private Span<bool> GetTreeNodeVisited() => _arena.GetSpan<bool>(_treeNodeVisitedOffset, _treeNodeVisitedCapacity);
    private Span<TextElementData> GetTextElementData() => _arena.GetSpan<TextElementData>(_textElementDataOffset, _textElementDataCapacity);
    private Span<WrappedTextLine> GetWrappedTextLines() => _arena.GetSpan<WrappedTextLine>(_wrappedTextLinesOffset, _wrappedTextLinesCapacity);
    private Span<FloatingConfig> GetFloatingConfigs() => _arena.GetSpan<FloatingConfig>(_floatingConfigsOffset, _floatingConfigsCapacity);
    private Span<ClipConfig> GetClipConfigs() => _arena.GetSpan<ClipConfig>(_clipConfigsOffset, _clipConfigsCapacity);
    private Span<RectangleConfig> GetRectangleConfigs() => _arena.GetSpan<RectangleConfig>(_rectangleConfigsOffset, _rectangleConfigsCapacity);
    private Span<BorderConfig> GetBorderConfigs() => _arena.GetSpan<BorderConfig>(_borderConfigsOffset, _borderConfigsCapacity);
    private Span<ImageConfig> GetImageConfigs() => _arena.GetSpan<ImageConfig>(_imageConfigsOffset, _imageConfigsCapacity);
    private Span<CustomConfig> GetCustomConfigs() => _arena.GetSpan<CustomConfig>(_customConfigsOffset, _customConfigsCapacity);

    internal int OpenElement(string id, ElementType type)
    {
        uint oldId = _currentParentId;
        _currentParentId = HashUtility.HashId(id, oldId);

        int index = _layoutElementsCount++;
        var elements = GetLayoutElements();
        
        _idToElementIndex[_currentParentId] = index;

        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int parentIndex = stack[_openLayoutElementStackCount - 1];
            var children = GetLayoutElementChildren();
            children[_layoutElementChildrenCount++] = index;
            ref var parent = ref elements[parentIndex];
            parent.ChildrenCount++;
        }

        elements[index] = new LayoutElement 
        { 
            Id = _currentParentId,
            ElementType = type,
            ChildrenStartIndex = _layoutElementChildrenCount,
            ChildrenCount = 0,
            ConfigIndex = -1,
            RectangleConfigIndex = -1,
            BorderConfigIndex = -1,
            ImageConfigIndex = -1
        };

        var openStack = GetOpenLayoutElementStack();
        openStack[_openLayoutElementStackCount++] = index;

        return index;
    }

    internal void CloseElement()
    {
        _openLayoutElementStackCount--;
    }

    internal void ConfigureOpenElement(LayoutConfig config)
    {
        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int index = stack[_openLayoutElementStackCount - 1];
            var elements = GetLayoutElements();
            ref var element = ref elements[index];
            element.Config = config;
            if (config.Sizing.Width.Type == SizingType.Fixed) element.Dimensions.Width = config.Sizing.Width.Size.MinMax.Min;
            if (config.Sizing.Height.Type == SizingType.Fixed) element.Dimensions.Height = config.Sizing.Height.Size.MinMax.Min;
        }
    }

    internal void ConfigureFloatingElement(FloatingConfig config)
    {
        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int index = stack[_openLayoutElementStackCount - 1];
            var elements = GetLayoutElements();
            ref var element = ref elements[index];
            
            var floatingConfigs = GetFloatingConfigs();
            int configIdx = _floatingConfigsCount++;
            floatingConfigs[configIdx] = config;
            element.ConfigIndex = configIdx;
            element.ElementType = ElementType.Floating;

            // Floating elements are added as new roots
            var roots = GetLayoutElementTreeRoots();
            uint actualParentId = config.AttachTo switch
            {
                FloatingAttachToElement.Parent => _openLayoutElementStackCount > 1 ? elements[stack[_openLayoutElementStackCount - 2]].Id : 0,
                FloatingAttachToElement.ElementWithId => config.ParentId,
                FloatingAttachToElement.Root => HashUtility.HashId("Clay__RootContainer", 0),
                _ => 0
            };

            roots[_layoutElementTreeRootsCount++] = new LayoutElementTreeRoot
            {
                LayoutElementIndex = index,
                ParentId = actualParentId,
                ZIndex = config.ZIndex
            };
        }
    }

    internal void ConfigureClipElement(ClipConfig config)
    {
        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int index = stack[_openLayoutElementStackCount - 1];
            var elements = GetLayoutElements();
            ref var element = ref elements[index];
            
            var clipConfigs = GetClipConfigs();
            int configIdx = _clipConfigsCount++;
            clipConfigs[configIdx] = config;
            element.ConfigIndex = configIdx;
            element.ElementType = ElementType.Scissor;
        }
    }

    internal void ConfigureRectangleElement(RectangleConfig config)
    {
        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int index = stack[_openLayoutElementStackCount - 1];
            var elements = GetLayoutElements();
            ref var element = ref elements[index];
            
            var configs = GetRectangleConfigs();
            int configIdx = _rectangleConfigsCount++;
            configs[configIdx] = config;
            element.RectangleConfigIndex = configIdx;
        }
    }

    internal void ConfigureBorderElement(BorderConfig config)
    {
        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int index = stack[_openLayoutElementStackCount - 1];
            var elements = GetLayoutElements();
            ref var element = ref elements[index];
            
            var configs = GetBorderConfigs();
            int configIdx = _borderConfigsCount++;
            configs[configIdx] = config;
            element.BorderConfigIndex = configIdx;
        }
    }

    internal void ConfigureCustomElement(CustomConfig config)
    {
        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int index = stack[_openLayoutElementStackCount - 1];
            var elements = GetLayoutElements();
            ref var element = ref elements[index];
            
            var configs = GetCustomConfigs();
            int configIdx = _customConfigsCount++;
            configs[configIdx] = config;
            element.ConfigIndex = configIdx;
            element.ElementType = ElementType.Custom;
        }
    }

    internal void ConfigureImageElement(ImageConfig config)
    {
        if (_openLayoutElementStackCount > 0)
        {
            var stack = GetOpenLayoutElementStack();
            int index = stack[_openLayoutElementStackCount - 1];
            var elements = GetLayoutElements();
            ref var element = ref elements[index];
            
            var configs = GetImageConfigs();
            int configIdx = _imageConfigsCount++;
            configs[configIdx] = config;
            element.ImageConfigIndex = configIdx;
            element.ElementType = ElementType.Image;
            
            // Images also set preferred dimensions
            element.PreferredDimensions = config.Dimensions;
            element.Dimensions = config.Dimensions;
        }
    }

    internal void AddTextElement(string text, TextConfig config)
    {
        int textIdx = _strings.Count;
        _strings.Add(text);

        int elementIndex = OpenElement(text, ElementType.Text);
        
        var textElements = GetTextElementData();
        int textIndex = _textElementDataCount++;
        textElements[textIndex] = new TextElementData
        {
            TextIndex = textIdx,
            Config = config,
            ElementIndex = elementIndex,
            WrappedLinesStartIndex = _wrappedTextLinesCount,
            WrappedLinesCount = 0
        };

        var elements = GetLayoutElements();
        ref var element = ref elements[elementIndex];
        element.ConfigIndex = textIndex;

        if (TextMeasure != null)
        {
            element.PreferredDimensions = TextMeasure(text, config);
            element.Dimensions = element.PreferredDimensions;
        }

        CloseElement();
    }

    public Span<RenderCommand> EndLayout()
    {
        if (_openLayoutElementStackCount > 0) CloseElement();
        CalculateFinalLayout();
        return GetRenderCommands().Slice(0, _renderCommandsCount);
    }

    public string GetString(int index) => _strings[index];
    public WrappedTextLine GetWrappedTextLine(int index) => GetWrappedTextLines()[index];

    private void CalculateFinalLayout()
    {
        // 1. Initial X sizing (Fixed, Percent)
        // 2. Bottom-up FIT sizing (Width)
        UpdateFitSizingBottomUp();
        // 3. Flex distribution X (Grow)
        SizeContainersAlongAxis(true); 

        // 4. Wrap text based on established Width
        WrapText();

        // 5. Bottom-up FIT sizing (Height)
        UpdateFitSizingBottomUp();
        // 6. Flex distribution Y (Grow)
        SizeContainersAlongAxis(false);

        // Sort tree roots by z-index (simple bubble sort as in original Clay)
        var roots = GetLayoutElementTreeRoots();
        int sortMax = _layoutElementTreeRootsCount - 1;
        while (sortMax > 0)
        {
            for (int i = 0; i < sortMax; i++)
            {
                if (roots[i + 1].ZIndex < roots[i].ZIndex)
                {
                    var temp = roots[i];
                    roots[i] = roots[i + 1];
                    roots[i + 1] = temp;
                }
            }
            sortMax--;
        }

        CalculatePositions();
    }

    private void SizeContainersAlongAxis(bool xAxis)
    {
        var roots = GetLayoutElementTreeRoots();
        var elements = GetLayoutElements();
        var allChildren = GetLayoutElementChildren();
        var bfsBuffer = GetBfsBuffer();
        var resizableContainerBuffer = GetOpenLayoutElementStack();

        for (int i = 0; i < _layoutElementTreeRootsCount; i++)
        {
            var root = roots[i];
            int bfsStart = 0;
            int bfsCount = 0;
            bfsBuffer[bfsCount++] = root.LayoutElementIndex;

            while (bfsStart < bfsCount)
            {
                int parentIndex = bfsBuffer[bfsStart++];
                ref LayoutElement parent = ref elements[parentIndex];
                
                int resizableCount = 0;
                float parentSize = xAxis ? parent.Dimensions.Width : parent.Dimensions.Height;
                float parentPadding = xAxis ? (float)(parent.Config.Padding.Left + parent.Config.Padding.Right) : (float)(parent.Config.Padding.Top + parent.Config.Padding.Bottom);
                float innerContentSize = 0;
                float totalPaddingAndChildGaps = parentPadding;
                bool sizingAlongAxis = (xAxis && parent.Config.LayoutDirection == LayoutDirection.LeftToRight) || (!xAxis && parent.Config.LayoutDirection == LayoutDirection.TopToBottom);
                float parentChildGap = parent.Config.ChildGap;

                for (int childOffset = 0; childOffset < parent.ChildrenCount; childOffset++)
                {
                    int childElementIndex = allChildren[parent.ChildrenStartIndex + childOffset];
                    ref LayoutElement childElement = ref elements[childElementIndex];
                    
                    // Skip floating elements in normal flow
                    if (childElement.ElementType == ElementType.Floating) continue;

                    SizingAxis childSizing = xAxis ? childElement.Config.Sizing.Width : childElement.Config.Sizing.Height;
                    float childSize = xAxis ? childElement.Dimensions.Width : childElement.Dimensions.Height;

                    if (childElement.ElementType != ElementType.Text && childElement.ChildrenCount > 0)
                    {
                        bfsBuffer[bfsCount++] = childElementIndex;
                    }

                    if (childSizing.Type != SizingType.Percent && childSizing.Type != SizingType.Fixed)
                    {
                        resizableContainerBuffer[resizableCount++] = childElementIndex;
                    }

                    if (sizingAlongAxis)
                    {
                        innerContentSize += (childSizing.Type == SizingType.Percent ? 0 : childSize);
                        if (childOffset > 0)
                        {
                            innerContentSize += parentChildGap;
                            totalPaddingAndChildGaps += parentChildGap;
                        }
                    }
                    else
                    {
                        innerContentSize = Math.Max(childSize, innerContentSize);
                    }
                }

                for (int childOffset = 0; childOffset < parent.ChildrenCount; childOffset++)
                {
                    int childElementIndex = allChildren[parent.ChildrenStartIndex + childOffset];
                    ref LayoutElement childElement = ref elements[childElementIndex];
                    
                    // Skip floating elements in normal flow
                    if (childElement.ElementType == ElementType.Floating) continue;

                    SizingAxis childSizing = xAxis ? childElement.Config.Sizing.Width : childElement.Config.Sizing.Height;
                    
                    if (childSizing.Type == SizingType.Percent)
                    {
                        float size = (parentSize - totalPaddingAndChildGaps) * childSizing.Size.Percent;
                        if (xAxis) childElement.Dimensions.Width = size; else childElement.Dimensions.Height = size;
                        if (sizingAlongAxis) innerContentSize += size;
                    }
                }

                if (sizingAlongAxis)
                {
                    float sizeToDistribute = parentSize - parentPadding - innerContentSize;
                    if (sizeToDistribute > 0)
                    {
                        int growCount = 0;
                        for (int j = 0; j < resizableCount; j++)
                        {
                            int childIdx = resizableContainerBuffer[j];
                            if ((xAxis ? elements[childIdx].Config.Sizing.Width.Type : elements[childIdx].Config.Sizing.Height.Type) == SizingType.Grow)
                                growCount++;
                        }

                        if (growCount > 0)
                        {
                            float growSize = sizeToDistribute / growCount;
                            for (int j = 0; j < resizableCount; j++)
                            {
                                int childIdx = resizableContainerBuffer[j];
                                if ((xAxis ? elements[childIdx].Config.Sizing.Width.Type : elements[childIdx].Config.Sizing.Height.Type) == SizingType.Grow)
                                {
                                    if (xAxis) elements[childIdx].Dimensions.Width += growSize; else elements[childIdx].Dimensions.Height += growSize;
                                }
                            }
                        }
                    }
                    else if (sizeToDistribute < 0 && resizableCount > 0)
                    {
                        // Compression
                        for (int j = 0; j < resizableCount; j++)
                        {
                            int childIdx = resizableContainerBuffer[j];
                            ref var child = ref elements[childIdx];
                            float previousSize = xAxis ? child.Dimensions.Width : child.Dimensions.Height;
                            float minSize = xAxis ? child.MinDimensions.Width : child.MinDimensions.Height;
                            
                            float newSize = Math.Max(minSize, previousSize + (sizeToDistribute / (resizableCount - j)));
                            if (xAxis) child.Dimensions.Width = newSize; else child.Dimensions.Height = newSize;
                            sizeToDistribute -= (newSize - previousSize);
                        }
                    }
                }
                else
                {
                    // For non-layout axis
                    for (int j = 0; j < resizableCount; j++)
                    {
                        int childIdx = resizableContainerBuffer[j];
                        ref var child = ref elements[childIdx];
                        float minSize = xAxis ? child.MinDimensions.Width : child.MinDimensions.Height;
                        float maxSize = parentSize - parentPadding;

                        // If it's Grow, it takes full size. If it's Fit, it stays at preferred but capped by parent.
                        if ((xAxis ? child.Config.Sizing.Width.Type : child.Config.Sizing.Height.Type) == SizingType.Grow)
                        {
                            if (xAxis) child.Dimensions.Width = Math.Max(minSize, maxSize); else child.Dimensions.Height = Math.Max(minSize, maxSize);
                        }
                        else
                        {
                            if (xAxis) child.Dimensions.Width = Math.Min(child.Dimensions.Width, maxSize); else child.Dimensions.Height = Math.Min(child.Dimensions.Height, maxSize);
                        }
                    }
                }
            }
        }
    }

    private void WrapText()
    {
        var textElements = GetTextElementData();
        var elements = GetLayoutElements();
        var wrappedLines = GetWrappedTextLines();

        if (TextMeasure == null) return;

        for (int i = 0; i < _textElementDataCount; i++)
        {
            ref var textData = ref textElements[i];
            ref var element = ref elements[textData.ElementIndex];
            string text = _strings[textData.TextIndex];
            
            if (textData.Config.WrapMode == TextWrapMode.None)
            {
                wrappedLines[_wrappedTextLinesCount++] = new WrappedTextLine
                {
                    Dimensions = element.Dimensions,
                    StartOffset = 0,
                    Length = text.Length
                };
                textData.WrappedLinesCount = 1;
                continue;
            }

            // Word wrapping
            float maxWidth = element.Dimensions.Width;
            int start = 0;
            int lineCount = 0;
            textData.WrappedLinesStartIndex = _wrappedTextLinesCount;

            ReadOnlySpan<char> textSpan = text.AsSpan();
            float currentLineHeight = 0;

            while (start < textSpan.Length)
            {
                int end = start;
                int lastSpace = -1;
                float currentWidth = 0;

                while (end < textSpan.Length)
                {
                    char c = textSpan[end];
                    if (c == '\n')
                    {
                        end++;
                        break;
                    }

                    // Simple measurement: measure up to current char
                    // In a real port we'd measure word by word
                    float nextWidth = TextMeasure(textSpan.Slice(start, end - start + 1), textData.Config).Width;
                    
                    if (nextWidth > maxWidth && end > start)
                    {
                        if (lastSpace != -1)
                        {
                            end = lastSpace + 1;
                        }
                        break;
                    }

                    if (c == ' ') lastSpace = end;
                    currentWidth = nextWidth;
                    end++;
                }

                if (end > start)
                {
                    var lineDimensions = TextMeasure(textSpan.Slice(start, end - start), textData.Config);
                    wrappedLines[_wrappedTextLinesCount++] = new WrappedTextLine
                    {
                        Dimensions = lineDimensions,
                        StartOffset = start,
                        Length = end - start
                    };
                    currentLineHeight += lineDimensions.Height;
                    lineCount++;
                }
                start = end;
            }

            textData.WrappedLinesCount = lineCount;
            element.Dimensions.Height = currentLineHeight;
        }
    }

    private void UpdateFitSizingBottomUp()
    {
        var elements = GetLayoutElements();
        var allChildren = GetLayoutElementChildren();
        for (int i = _layoutElementsCount - 1; i >= 0; i--)
        {
            ref var element = ref elements[i];
            if (element.ChildrenCount == 0 && element.ElementType != ElementType.Text) continue;

            if (element.ElementType == ElementType.Text)
            {
                // Text elements already have their dimensions set by measurement/wrapping
                continue;
            }

            float contentWidth = (float)(element.Config.Padding.Left + element.Config.Padding.Right);
            float contentHeight = (float)(element.Config.Padding.Top + element.Config.Padding.Bottom);

            if (element.Config.LayoutDirection == LayoutDirection.LeftToRight)
            {
                float maxChildHeight = 0;
                for (int j = 0; j < element.ChildrenCount; j++)
                {
                    int childIdx = allChildren[element.ChildrenStartIndex + j];
                    ref var child = ref elements[childIdx];
                    if (child.ElementType == ElementType.Floating) continue;

                    contentWidth += child.Dimensions.Width;
                    maxChildHeight = Math.Max(maxChildHeight, child.Dimensions.Height);
                }
                contentWidth += Math.Max(0, element.ChildrenCount - 1) * element.Config.ChildGap;
                contentHeight += maxChildHeight;
            }
            else // TopToBottom
            {
                float maxChildWidth = 0;
                for (int j = 0; j < element.ChildrenCount; j++)
                {
                    int childIdx = allChildren[element.ChildrenStartIndex + j];
                    ref var child = ref elements[childIdx];
                    if (child.ElementType == ElementType.Floating) continue;

                    contentHeight += child.Dimensions.Height;
                    maxChildWidth = Math.Max(maxChildWidth, child.Dimensions.Width);
                }
                contentHeight += Math.Max(0, element.ChildrenCount - 1) * element.Config.ChildGap;
                contentWidth += maxChildWidth;
            }

            if (element.Config.Sizing.Width.Type == SizingType.Fit)
            {
                element.Dimensions.Width = contentWidth;
            }
            if (element.Config.Sizing.Height.Type == SizingType.Fit)
            {
                element.Dimensions.Height = contentHeight;
            }
        }
    }

    private void CalculatePositions()
    {
        var roots = GetLayoutElementTreeRoots();
        var elements = GetLayoutElements();
        var allChildren = GetLayoutElementChildren();
        var dfsBuffer = GetDfsBuffer();
        var treeNodeVisited = GetTreeNodeVisited();
        var floatingConfigs = GetFloatingConfigs();
        _renderCommandsCount = 0;
        var renderCommands = GetRenderCommands();

        for (int i = 0; i < _layoutElementTreeRootsCount; i++)
        {
            var root = roots[i];
            ref LayoutElement rootElement = ref elements[root.LayoutElementIndex];
            
            Vector2 rootPosition = new Vector2(0, 0);

            // If it's a floating root, calculate its position based on parent
            if (rootElement.ElementType != ElementType.Text && rootElement.ConfigIndex >= 0 && root.ParentId != 0)
            {
                if (_idToElementIndex.TryGetValue(root.ParentId, out int parentIdx))
                {
                    ref var parentElement = ref elements[parentIdx];
                    ref var floatingConfig = ref floatingConfigs[rootElement.ConfigIndex];
                    
                    BoundingBox parentBB = parentElement.BoundingBox;
                    Vector2 targetPos = new Vector2(parentBB.X, parentBB.Y);

                    // Parent attach point
                    switch (floatingConfig.AttachPoints.Parent)
                    {
                        case FloatingAttachPoint.LeftCenter:
                        case FloatingAttachPoint.CenterCenter:
                        case FloatingAttachPoint.RightCenter: targetPos.Y += parentBB.Height / 2; break;
                        case FloatingAttachPoint.LeftBottom:
                        case FloatingAttachPoint.CenterBottom:
                        case FloatingAttachPoint.RightBottom: targetPos.Y += parentBB.Height; break;
                    }
                    switch (floatingConfig.AttachPoints.Parent)
                    {
                        case FloatingAttachPoint.CenterTop:
                        case FloatingAttachPoint.CenterCenter:
                        case FloatingAttachPoint.CenterBottom: targetPos.X += parentBB.Width / 2; break;
                        case FloatingAttachPoint.RightTop:
                        case FloatingAttachPoint.RightCenter:
                        case FloatingAttachPoint.RightBottom: targetPos.X += parentBB.Width; break;
                    }

                    // Element attach point (offset)
                    switch (floatingConfig.AttachPoints.Element)
                    {
                        case FloatingAttachPoint.LeftCenter:
                        case FloatingAttachPoint.CenterCenter:
                        case FloatingAttachPoint.RightCenter: targetPos.Y -= rootElement.Dimensions.Height / 2; break;
                        case FloatingAttachPoint.LeftBottom:
                        case FloatingAttachPoint.CenterBottom:
                        case FloatingAttachPoint.RightBottom: targetPos.Y -= rootElement.Dimensions.Height; break;
                    }
                    switch (floatingConfig.AttachPoints.Element)
                    {
                        case FloatingAttachPoint.CenterTop:
                        case FloatingAttachPoint.CenterCenter:
                        case FloatingAttachPoint.CenterBottom: targetPos.X -= rootElement.Dimensions.Width / 2; break;
                        case FloatingAttachPoint.RightTop:
                        case FloatingAttachPoint.RightCenter:
                        case FloatingAttachPoint.RightBottom: targetPos.X -= rootElement.Dimensions.Width; break;
                    }

                    rootPosition = targetPos + floatingConfig.Offset;
                }
            }

            int dfsCount = 0;
            dfsBuffer[dfsCount] = new LayoutElementTreeNode 
            { 
                LayoutElementIndex = root.LayoutElementIndex,
                Position = rootPosition,
                NextChildOffset = new Vector2(rootElement.Config.Padding.Left, rootElement.Config.Padding.Top)
            };
            treeNodeVisited[dfsCount] = false;
            dfsCount++;

            while (dfsCount > 0)
            {
                int currentNodeIdx = dfsCount - 1;
                ref LayoutElementTreeNode currentNode = ref dfsBuffer[currentNodeIdx];
                ref LayoutElement currentElement = ref elements[currentNode.LayoutElementIndex];
                
                if (!treeNodeVisited[currentNodeIdx])
                {
                    treeNodeVisited[currentNodeIdx] = true;
                    currentElement.BoundingBox = new BoundingBox(currentNode.Position.X, currentNode.Position.Y, currentElement.Dimensions.Width, currentElement.Dimensions.Height);
                    
                    // Update persistent state for NEXT frame
                    _persistentElementState[currentElement.Id] = new LayoutElementHashMapItem
                    {
                        Id = currentElement.Id,
                        BoundingBox = currentElement.BoundingBox
                    };

                    Vector2 scrollOffset = new Vector2(0, 0);
                    if (currentElement.ElementType == ElementType.Scissor && currentElement.ConfigIndex >= 0)
                    {
                        ref var clipConfig = ref GetClipConfigs()[currentElement.ConfigIndex];
                        scrollOffset = clipConfig.ChildOffset;

                        if (_renderCommandsCount < _renderCommandsCapacity)
                        {
                            renderCommands[_renderCommandsCount++] = new RenderCommand
                            {
                                BoundingBox = currentElement.BoundingBox,
                                CommandType = RenderCommandType.ScissorStart,
                                Id = currentElement.Id,
                                ZIndex = root.ZIndex
                            };
                        }
                    }

                    if (currentElement.ElementType == ElementType.Text)
                    {
                        // Emit commands for each wrapped line
                        if (currentElement.ConfigIndex >= 0)
                        {
                            ref var textData = ref GetTextElementData()[currentElement.ConfigIndex];
                            var lines = GetWrappedTextLines();
                            float lineYOffset = 0;

                            for (int j = 0; j < textData.WrappedLinesCount; j++)
                            {
                                if (_renderCommandsCount >= _renderCommandsCapacity) break;

                                var line = lines[textData.WrappedLinesStartIndex + j];
                                var lineBB = new BoundingBox(currentElement.BoundingBox.X, currentElement.BoundingBox.Y + lineYOffset, line.Dimensions.Width, line.Dimensions.Height);
                                
                                renderCommands[_renderCommandsCount++] = new RenderCommand
                                {
                                    BoundingBox = lineBB,
                                    CommandType = RenderCommandType.Text,
                                    RenderData = new RenderData { Text = new TextRenderData 
                                    { 
                                        TextIndex = textData.TextIndex, 
                                        TextColor = textData.Config.TextColor, 
                                        FontSize = textData.Config.FontSize,
                                        LineStart = line.StartOffset,
                                        LineLength = line.Length
                                    } },
                                    Id = currentElement.Id,
                                    ZIndex = root.ZIndex
                                };
                                lineYOffset += line.Dimensions.Height;
                            }
                        }
                    }
                    else
                    {
                        if (currentElement.RectangleConfigIndex >= 0)
                        {
                            ref var rectConfig = ref GetRectangleConfigs()[currentElement.RectangleConfigIndex];
                            if (_renderCommandsCount < _renderCommandsCapacity)
                            {
                                renderCommands[_renderCommandsCount++] = new RenderCommand
                                {
                                    BoundingBox = currentElement.BoundingBox,
                                    CommandType = RenderCommandType.Rectangle,
                                    RenderData = new RenderData { Rectangle = new RectangleRenderData { Color = rectConfig.Color, CornerRadius = rectConfig.CornerRadius } },
                                    Id = currentElement.Id,
                                    ZIndex = root.ZIndex
                                };
                            }
                        }

                        if (currentElement.BorderConfigIndex >= 0)
                        {
                            ref var borderConfig = ref GetBorderConfigs()[currentElement.BorderConfigIndex];
                            if (_renderCommandsCount < _renderCommandsCapacity)
                            {
                                renderCommands[_renderCommandsCount++] = new RenderCommand
                                {
                                    BoundingBox = currentElement.BoundingBox,
                                    CommandType = RenderCommandType.ScissorStart, // Placeholder for border logic
                                    Id = currentElement.Id,
                                    ZIndex = root.ZIndex
                                };
                            }
                        }

                        if (currentElement.ImageConfigIndex >= 0)
                        {
                            ref var imageConfig = ref GetImageConfigs()[currentElement.ImageConfigIndex];
                            if (_renderCommandsCount < _renderCommandsCapacity)
                            {
                                renderCommands[_renderCommandsCount++] = new RenderCommand
                                {
                                    BoundingBox = currentElement.BoundingBox,
                                    CommandType = RenderCommandType.Image,
                                    Id = currentElement.Id,
                                    ZIndex = root.ZIndex
                                };
                            }
                        }

                        if (currentElement.ElementType == ElementType.Custom && currentElement.ConfigIndex >= 0)
                        {
                            if (_renderCommandsCount < _renderCommandsCapacity)
                            {
                                renderCommands[_renderCommandsCount++] = new RenderCommand
                                {
                                    BoundingBox = currentElement.BoundingBox,
                                    CommandType = RenderCommandType.Custom,
                                    Id = currentElement.Id,
                                    ZIndex = root.ZIndex
                                };
                            }
                        }
                    }

                    if (currentElement.ElementType != ElementType.Text)
                    {
                        int childrenCount = currentElement.ChildrenCount;
                        
                        int validChildrenCount = 0;
                        float totalContentWidth = 0;
                        float totalContentHeight = 0;
                        float maxChildWidth = 0;
                        float maxChildHeight = 0;

                        for (int j = 0; j < childrenCount; j++)
                        {
                            int childIdx = allChildren[currentElement.ChildrenStartIndex + j];
                            ref LayoutElement child = ref elements[childIdx];
                            if (child.ElementType != ElementType.Floating)
                            {
                                validChildrenCount++;
                                if (currentElement.Config.LayoutDirection == LayoutDirection.LeftToRight)
                                {
                                    totalContentWidth += child.Dimensions.Width;
                                    maxChildHeight = Math.Max(maxChildHeight, child.Dimensions.Height);
                                }
                                else
                                {
                                    totalContentHeight += child.Dimensions.Height;
                                    maxChildWidth = Math.Max(maxChildWidth, child.Dimensions.Width);
                                }
                            }
                        }

                        if (validChildrenCount > 0)
                        {
                            if (currentElement.Config.LayoutDirection == LayoutDirection.LeftToRight)
                            {
                                totalContentWidth += (validChildrenCount - 1) * currentElement.Config.ChildGap;
                                totalContentHeight = maxChildHeight;
                            }
                            else
                            {
                                totalContentHeight += (validChildrenCount - 1) * currentElement.Config.ChildGap;
                                totalContentWidth = maxChildWidth;
                            }
                        }

                        // Calculate starting offset based on alignment along the layout axis
                        Vector2 alignmentOffset = new Vector2(0, 0);
                        float availableWidth = currentElement.Dimensions.Width - (currentElement.Config.Padding.Left + currentElement.Config.Padding.Right);
                        float availableHeight = currentElement.Dimensions.Height - (currentElement.Config.Padding.Top + currentElement.Config.Padding.Bottom);

                        if (currentElement.Config.LayoutDirection == LayoutDirection.LeftToRight)
                        {
                            switch (currentElement.Config.ChildAlignment.X)
                            {
                                case LayoutAlignmentX.Center: alignmentOffset.X = (availableWidth - totalContentWidth) / 2; break;
                                case LayoutAlignmentX.Right: alignmentOffset.X = availableWidth - totalContentWidth; break;
                            }
                        }
                        else
                        {
                            switch (currentElement.Config.ChildAlignment.Y)
                            {
                                case LayoutAlignmentY.Center: alignmentOffset.Y = (availableHeight - totalContentHeight) / 2; break;
                                case LayoutAlignmentY.Bottom: alignmentOffset.Y = availableHeight - totalContentHeight; break;
                            }
                        }

                        currentNode.NextChildOffset += alignmentOffset;

                        int stackStartIndex = dfsCount;
                        dfsCount += validChildrenCount;

                        int addedCount = 0;
                        for (int j = 0; j < childrenCount; j++)
                        {
                            int childIdx = allChildren[currentElement.ChildrenStartIndex + j];
                            ref LayoutElement child = ref elements[childIdx];
                            
                            if (child.ElementType == ElementType.Floating) continue;

                            // Calculate individual alignment for the non-layout axis
                            Vector2 individualOffset = new Vector2(0, 0);
                            if (currentElement.Config.LayoutDirection == LayoutDirection.LeftToRight)
                            {
                                switch (currentElement.Config.ChildAlignment.Y)
                                {
                                    case LayoutAlignmentY.Center: individualOffset.Y = (availableHeight - child.Dimensions.Height) / 2; break;
                                    case LayoutAlignmentY.Bottom: individualOffset.Y = availableHeight - child.Dimensions.Height; break;
                                }
                            }
                            else
                            {
                                switch (currentElement.Config.ChildAlignment.X)
                                {
                                    case LayoutAlignmentX.Center: individualOffset.X = (availableWidth - child.Dimensions.Width) / 2; break;
                                    case LayoutAlignmentX.Right: individualOffset.X = availableWidth - child.Dimensions.Width; break;
                                }
                            }

                            Vector2 childPos = currentNode.Position + currentNode.NextChildOffset + scrollOffset + individualOffset;
                            
                            int newNodeStackIdx = stackStartIndex + (validChildrenCount - 1 - addedCount);
                            dfsBuffer[newNodeStackIdx] = new LayoutElementTreeNode
                            {
                                LayoutElementIndex = childIdx,
                                Position = childPos,
                                NextChildOffset = new Vector2(child.Config.Padding.Left, child.Config.Padding.Top)
                            };
                            treeNodeVisited[newNodeStackIdx] = false;
                            addedCount++;

                            if (currentElement.Config.LayoutDirection == LayoutDirection.LeftToRight)
                            {
                                currentNode.NextChildOffset.X += child.Dimensions.Width + currentElement.Config.ChildGap;
                            }
                            else
                            {
                                currentNode.NextChildOffset.Y += child.Dimensions.Height + currentElement.Config.ChildGap;
                            }
                        }
                    }
                }
                else
                {
                    if (currentElement.ElementType == ElementType.Scissor)
                    {
                        if (_renderCommandsCount < _renderCommandsCapacity)
                        {
                            renderCommands[_renderCommandsCount++] = new RenderCommand
                            {
                                Id = currentElement.Id,
                                CommandType = RenderCommandType.ScissorEnd,
                                ZIndex = root.ZIndex
                            };
                        }
                    }
                    dfsCount--;
                }
            }
        }
    }
}
