using SkiaSharp;
using CSClay;

namespace CSClay.Renderers.SkiaSharp;

public class SkiaSharpRenderer
{
    public static void Render(SKCanvas canvas, Span<RenderCommand> commands, ClayContext context)
    {
        using var paint = new SKPaint();
        using var font = new SKFont(SKTypeface.Default, 12);
        paint.IsAntialias = true;

        foreach (var cmd in commands)
        {
            var rect = new SKRect(
                cmd.BoundingBox.X, 
                cmd.BoundingBox.Y, 
                cmd.BoundingBox.X + cmd.BoundingBox.Width, 
                cmd.BoundingBox.Y + cmd.BoundingBox.Height
            );

            switch (cmd.CommandType)
            {
                case RenderCommandType.Rectangle:
                    var clayColor = cmd.RenderData.Rectangle.Color;
                    paint.Color = new SKColor((byte)clayColor.R, (byte)clayColor.G, (byte)clayColor.B, (byte)clayColor.A);
                    paint.Style = SKPaintStyle.Fill;
                    
                    var radius = cmd.RenderData.Rectangle.CornerRadius;
                    if (radius.TopLeft > 0 || radius.TopRight > 0 || radius.BottomLeft > 0 || radius.BottomRight > 0)
                    {
                        var roundRect = new SKRoundRect(rect);
                        roundRect.SetRectRadii(rect, new[] { 
                            new SKPoint(radius.TopLeft, radius.TopLeft),
                            new SKPoint(radius.TopRight, radius.TopRight),
                            new SKPoint(radius.BottomRight, radius.BottomRight),
                            new SKPoint(radius.BottomLeft, radius.BottomLeft) 
                        });
                        canvas.DrawRoundRect(roundRect, paint);
                    }
                    else
                    {
                        canvas.DrawRect(rect, paint);
                    }
                    break;

                case RenderCommandType.Border:
                    var border = cmd.RenderData.Border.Config;
                    var borderColor = border.Color;
                    paint.Color = new SKColor((byte)borderColor.R, (byte)borderColor.G, (byte)borderColor.B, (byte)borderColor.A);
                    paint.Style = SKPaintStyle.Fill; // Borders are drawn as filled rects for thickness

                    if (border.Top > 0)
                        canvas.DrawRect(SKRect.Create(rect.Left, rect.Top, rect.Width, border.Top), paint);
                    if (border.Bottom > 0)
                        canvas.DrawRect(SKRect.Create(rect.Left, rect.Bottom - border.Bottom, rect.Width, border.Bottom), paint);
                    if (border.Left > 0)
                        canvas.DrawRect(SKRect.Create(rect.Left, rect.Top, border.Left, rect.Height), paint);
                    if (border.Right > 0)
                        canvas.DrawRect(SKRect.Create(rect.Right - border.Right, rect.Top, border.Right, rect.Height), paint);
                    break;

                case RenderCommandType.Text:
                    var textData = cmd.RenderData.Text;
                    var textColor = textData.TextColor;
                    paint.Color = new SKColor((byte)textColor.R, (byte)textColor.G, (byte)textColor.B, (byte)textColor.A);
                    paint.Style = SKPaintStyle.Fill;
                    font.Size = textData.FontSize;

                    string fullText = context.GetString(textData.TextIndex);
                    string lineText = fullText.Substring(textData.LineStart, textData.LineLength);
                    
                    var metrics = font.Metrics;
                    // Note: text rendering position might need refinement for vertical alignment
                    canvas.DrawText(lineText, rect.Left, rect.Top - metrics.Ascent, SKTextAlign.Left, font, paint);
                    break;

                case RenderCommandType.Image:
                    var image = cmd.RenderData.Image;
                    var bgColor = image.BackgroundColor;
                    var imgRadius = image.CornerRadius;

                    if (bgColor.A > 0)
                    {
                        paint.Color = new SKColor((byte)bgColor.R, (byte)bgColor.G, (byte)bgColor.B, (byte)bgColor.A);
                        paint.Style = SKPaintStyle.Fill;
                        
                        if (imgRadius.TopLeft > 0 || imgRadius.TopRight > 0 || imgRadius.BottomLeft > 0 || imgRadius.BottomRight > 0)
                        {
                            var roundRect = new SKRoundRect(rect);
                            roundRect.SetRectRadii(rect, new[] { 
                                new SKPoint(imgRadius.TopLeft, imgRadius.TopLeft),
                                new SKPoint(imgRadius.TopRight, imgRadius.TopRight),
                                new SKPoint(imgRadius.BottomRight, imgRadius.BottomRight),
                                new SKPoint(imgRadius.BottomLeft, imgRadius.BottomLeft) 
                            });
                            canvas.DrawRoundRect(roundRect, paint);
                        }
                        else
                        {
                            canvas.DrawRect(rect, paint);
                        }
                    }
                    
                    // Placeholder for actual image drawing
                    paint.Color = SKColors.Gray;
                    paint.Style = SKPaintStyle.Stroke;
                    canvas.DrawRect(rect, paint);
                    canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Bottom, paint);
                    canvas.DrawLine(rect.Right, rect.Top, rect.Left, rect.Bottom, paint);
                    break;

                case RenderCommandType.ScissorStart:
                    canvas.Save();
                    canvas.ClipRect(rect);
                    break;

                case RenderCommandType.ScissorEnd:
                    canvas.Restore();
                    break;

                case RenderCommandType.Custom:
                    // Hook for custom rendering if needed
                    break;
            }
        }
    }
}
