// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

/// <summary>
/// Renders INCREMENTAL LINE (scribble) commands.
/// Draws polylines with motion-based segments.
/// </summary>
public class DrawableIncrementalLine : Drawable, IDrawable
{
    private readonly IncrementalLineCommand _command;

    public DrawableIncrementalLine(IncrementalLineCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        if (!_command.IsValid || _command.Segments.Count == 0)
        {
            return;
        }

        // Get starting position from current pen + offset
        var startPos = state.Pen;
        startPos.X += _command.StartOffset.X;
        startPos.Y += _command.StartOffset.Y;

        var startPoint = ConvertNormalizedToPoint(size, startPos.X, startPos.Y);
        float currentX = startPoint.X;
        float currentY = startPoint.Y;

        // Get pen color and width
        var (fgColor, _) = GetISColorFromState(state);
        float penWidth = GetPenWidth(size);
        var pen = Pens.Solid(fgColor, penWidth);

        image.Mutate(ctx =>
        {
            foreach (var segment in _command.Segments)
            {
                // Dx and Dy are in normalized coordinate space with correct sign
                // Convert to screen pixel displacement
                var (dx, dy) = ConvertNormalizedToScreenScale(size, segment.Dx, segment.Dy);

                // Note: Y is inverted in screen coordinates
                float nextX = currentX + dx;
                float nextY = currentY - dy; // Subtract because screen Y is inverted

                if (segment.Draw)
                {
                    // Draw line from current to next
                    ctx.DrawLine(pen, new PointF(currentX, currentY), new PointF(nextX, nextY));
                }

                currentX = nextX;
                currentY = nextY;
            }
        });

        // Update pen position to last point (convert back to normalized)
        var (normX, normY) = ConvertScreenToNormalizedF(size, currentX, currentY);
        state.Pen = new Vector3(normX, normY, 0);
    }

    // Uses centralized NaplpsUtils.ConvertScreenToNormalizedF
}
