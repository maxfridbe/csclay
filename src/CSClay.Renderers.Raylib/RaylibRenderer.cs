using Raylib_cs;
using CSClay;

namespace CSClay.Renderers.Raylib;

public class RaylibRenderer
{
    public static void Render(Span<RenderCommand> commands, ClayContext context, Font? font = null, Action<RenderCommand, Rectangle>? customRenderHandler = null)
    {
        foreach (var cmd in commands)
        {
            var rect = new Rectangle(cmd.BoundingBox.X, cmd.BoundingBox.Y, cmd.BoundingBox.Width, cmd.BoundingBox.Height);

            switch (cmd.CommandType)
            {
                case RenderCommandType.Rectangle:
                    var clayColor = cmd.RenderData.Rectangle.Color;
                    var rlColor = new Raylib_cs.Color((byte)clayColor.R, (byte)clayColor.G, (byte)clayColor.B, (byte)clayColor.A);
                    var radius = cmd.RenderData.Rectangle.CornerRadius;
                    
                    // Raylib-cs DrawRectangleRounded uses a single radius factor (0 to 1)
                    float maxRadius = Math.Max(radius.TopLeft, Math.Max(radius.TopRight, Math.Max(radius.BottomLeft, radius.BottomRight)));
                    if (maxRadius > 0)
                    {
                        float roundness = (maxRadius * 2.0f) / Math.Min(rect.Width, rect.Height);
                        Raylib_cs.Raylib.DrawRectangleRounded(rect, roundness, 16, rlColor);
                    }
                    else
                    {
                        Raylib_cs.Raylib.DrawRectangleRec(rect, rlColor);
                    }
                    break;

                case RenderCommandType.Border:
                    var border = cmd.RenderData.Border.Config;
                    var bColor = new Raylib_cs.Color((byte)border.Color.R, (byte)border.Color.G, (byte)border.Color.B, (byte)border.Color.A);
                    
                    if (border.Top > 0)
                        Raylib_cs.Raylib.DrawRectangleRec(new Rectangle(rect.X, rect.Y, rect.Width, border.Top), bColor);
                    if (border.Bottom > 0)
                        Raylib_cs.Raylib.DrawRectangleRec(new Rectangle(rect.X, rect.Y + rect.Height - border.Bottom, rect.Width, border.Bottom), bColor);
                    if (border.Left > 0)
                        Raylib_cs.Raylib.DrawRectangleRec(new Rectangle(rect.X, rect.Y, border.Left, rect.Height), bColor);
                    if (border.Right > 0)
                        Raylib_cs.Raylib.DrawRectangleRec(new Rectangle(rect.X + rect.Width - border.Right, rect.Y, border.Right, rect.Height), bColor);
                    break;

                case RenderCommandType.Text:
                    var textData = cmd.RenderData.Text;
                    string fullText = context.GetString(textData.TextIndex);
                    string lineText = fullText.Substring(textData.LineStart, textData.LineLength);
                    var tColor = new Raylib_cs.Color((byte)textData.TextColor.R, (byte)textData.TextColor.G, (byte)textData.TextColor.B, (byte)textData.TextColor.A);
                    
                    if (font.HasValue)
                    {
                        Raylib_cs.Raylib.DrawTextEx(font.Value, lineText, new System.Numerics.Vector2(rect.X, rect.Y), textData.FontSize, 0, tColor);
                    }
                    else
                    {
                        Raylib_cs.Raylib.DrawText(lineText, (int)rect.X, (int)rect.Y, textData.FontSize, tColor);
                    }
                    break;

                case RenderCommandType.Image:
                    var image = cmd.RenderData.Image;
                    var imgBgColor = new Raylib_cs.Color((byte)image.BackgroundColor.R, (byte)image.BackgroundColor.G, (byte)image.BackgroundColor.B, (byte)image.BackgroundColor.A);
                    
                    if (imgBgColor.A > 0)
                    {
                        Raylib_cs.Raylib.DrawRectangleRec(rect, imgBgColor);
                    }
                    
                    // Placeholder for actual image: draw a crossed box
                    Raylib_cs.Raylib.DrawRectangleLinesEx(rect, 1, Raylib_cs.Color.Gray);
                    Raylib_cs.Raylib.DrawLine((int)rect.X, (int)rect.Y, (int)(rect.X + rect.Width), (int)(rect.Y + rect.Height), Raylib_cs.Color.Gray);
                    Raylib_cs.Raylib.DrawLine((int)(rect.X + rect.Width), (int)rect.Y, (int)rect.X, (int)(rect.Y + rect.Height), Raylib_cs.Color.Gray);
                    break;

                case RenderCommandType.ScissorStart:
                    Raylib_cs.Raylib.BeginScissorMode((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                    break;

                case RenderCommandType.ScissorEnd:
                    Raylib_cs.Raylib.EndScissorMode();
                    break;

                case RenderCommandType.Custom:
                    customRenderHandler?.Invoke(cmd, rect);
                    break;
            }
        }
    }
}
