using SixLabors.ImageSharp.Processing;

namespace NAPLPS.Drawing;

public class DrawableTextCommand : Drawable, IDrawable
{
    private readonly TextCommand _command;

    public DrawableTextCommand(TextCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        image.Mutate(ctx =>
        {
            if (Options.DebugTextDrawing)
            {
                // draw a rectangle where the text would be drawn using imagesharp


            }
        });
    }
}
