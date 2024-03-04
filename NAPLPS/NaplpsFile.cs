// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Numerics;
using static NAPLPS.NaplpsCommands;
using NaplpsSequence = System.Tuple<NAPLPS.NaplpsState, NAPLPS.Commands.NaplpsCommand>;

namespace NAPLPS;

public class NaplpsFile
{
    public bool IsErrored => Errors.Count > 0;

    public bool Is8Bit => !Is7Bit;

    public bool Is7Bit { get; private set; } = true;

    public bool IsValid {  get; private set; }

    public IReadOnlyList<NaplpsError> Errors { get; } = [];

    public IReadOnlyList<NaplpsSequence> Commands { get; }

    private NaplpsFile(BinaryReader reader)
    {
        Commands = ReadStream(reader, new());

        // Analysis
        Is7Bit = !Commands.Any(cmd => (byte)cmd.Item2.OpCode > 0x80);

        IsValid = true;
    }

    public bool SavePNG(string filename = "test")
    {
        int width = 320, height = 240;

        using var image = new Image<Rgba32>(width, height);

        var point = PointF.Empty;

        foreach (var seq in Commands)
        {
            if (seq.Item2 is PointSetAbsoluteCommand cmd)
            {
                point = new PointF(cmd.Point.X, cmd.Point.Y);
            }
            else if (seq.Item2 is LineRelativeCommand cmd2)
            {
                var startPoint = ConvertNormalizedToPoint(new Size(width, height), point.X, point.Y);

                var endPoint = ConvertNormalizedToPoint(new Size(width, height), point.X + cmd2.Point.X, point.Y + cmd2.Point.Y);

                var pen = Pens.Solid(Color.Red, 1f);

                image.Mutate(x => x.DrawLine(pen, [new PointF(startPoint.X, startPoint.Y), new PointF(endPoint.X, endPoint.Y)]));
            }
            else if (seq.Item2 is PolygonSetFilledCommand cmd3)
            {
                var polygonPoints = new List<PointF>();

                var startPoint = ConvertNormalizedToPoint(new Size(width, height), cmd3.StartPoint.X, cmd3.StartPoint.Y);

                polygonPoints.Add(startPoint);

                point = new PointF(cmd3.StartPoint.X, cmd3.StartPoint.Y);

                foreach (var polyPoint in cmd3.Vertices)
                {
                    var polyPoint2 = ConvertNormalizedToPoint(new Size(width, height), point.X + polyPoint.X, point.Y + polyPoint.Y);

                    polygonPoints.Add(polyPoint2);

                    point = new PointF(point.X + polyPoint.X, point.Y + polyPoint.Y);
                }

                var pen = Pens.Solid(Color.Red, 1f);

                image.Mutate(x => x.FillPolygon(Brushes.Solid(Color.Red), polygonPoints.ToArray()));
            }
        }

        image.SaveAsPng($"{filename}.png");

        return true;
    }


    public static (int, int) ConvertCoordinates(int width, int height, int x, int y)
    {
        // Convert x from top-right origin to bottom-left origin by subtracting from width
        int convertedX = x;

        // Convert y from top-right origin to bottom-left origin by subtracting from height
        int convertedY = height - y;

        return (convertedX, convertedY);
    }

    public static Point ConvertNormalizedToPoint(Size size, double normalizedX, double normalizedY)
    {
        if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 0.75)
        {
            normalizedX = Math.Clamp(normalizedX, 0, 1);
            normalizedY = Math.Clamp(normalizedY, 0, 0.75);
        }

        var shrunkY = normalizedY / 0.75;

        int actualX = (int)(normalizedX * size.Width);
        int actualY = (int)(shrunkY * size.Height);

        (actualX, actualY) = ConvertCoordinates(size.Width, size.Height, actualX, actualY);

        return new Point(actualX, actualY);
    }

    public static NaplpsFile FromFile(string fullpath)
    {
        var file = File.OpenRead(fullpath);

        return new NaplpsFile(new BinaryReader(file));
    }

    public static List<NaplpsSequence> ReadStream(BinaryReader reader, NaplpsState state)
    {
        var commands = new List<NaplpsSequence>();

        try
        {
            while (!reader.IsEOF())
            {
                var opcode = reader.ReadByte();
                var operands = new List<byte>();

                var shiftIn = opcode == (byte)SHIFT_IN;

                while (!reader.IsEOF() && (!((byte)reader.PeekChar()).IsOpcode() || shiftIn))
                {
                    var operand = reader.ReadByte();

                    if (operand > 0x80)
                    {
                        Debugger.Break();
                    }

                    operands.Add(operand);

                    if (shiftIn && (byte)reader.PeekChar() == (byte)SHIFT_OUT)
                    {
                        break;
                    }
                }

                var command = NaplpsCommand.Factory(state, (NaplpsCommands)opcode, operands);

                commands.Add(new NaplpsSequence(command.State.ShallowCopy(), command));
            }
        }
        catch (EndOfStreamException)
        {
            Debugger.Break();
        }

        return commands;
    }
}
