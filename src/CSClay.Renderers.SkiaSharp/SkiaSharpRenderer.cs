using SkiaSharp;
using Clay;

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
                        canvas.DrawRoundRect(rect, radius.TopLeft, radius.TopLeft, paint);
                    }
                    else
                    {
                        canvas.DrawRect(rect, paint);
                    }
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
                    canvas.DrawText(lineText, rect.Left, rect.Top - metrics.Ascent, SKTextAlign.Left, font, paint);
                    break;

                case RenderCommandType.ScissorStart:
                    canvas.Save();
                    canvas.ClipRect(rect);
                    break;

                case RenderCommandType.ScissorEnd:
                    canvas.Restore();
                    break;
            }
        }
    }
}
