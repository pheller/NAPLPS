// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class GeometricDrawingCommandBase : NaplpsCommand
{
    private static readonly bool[] _twoDimensionalZero = [false, false, false];
    private static readonly bool[] _threeDimensionalZero = [false, false];

    public List<Vector3> Vertices { get; internal set; } = [];

    public List<Vector3> Points { get; private set; } = [];

    public NaplpsTexture Texture { get; set; }

    public GeometricDrawingCommandBase(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands) {
        this.Texture = state.Texture;
    }

    internal void SetPen(Vector3 point)
    {
        State.Pen = point;
        Points.Add(point);
    }

    internal void MovePen(Vector3 point)
    {
        State.Pen += point;
        Points.Add(Points.LastOrDefault() + point);
    }

    internal List<Vector3> ProcessVertices(NaplpsOperands operands, bool isInt = false)
    {
        var verts = new List<Vector3>();

        var x = new List<bool>();
        var y = new List<bool>();
        var z = new List<bool>();

        var zeroFill = false;

        for (int idx = 0; idx < operands.Count; idx++)
        {
            if (State.Dimensionality == 2)
            {
                x.AddRange([operands[idx, 6], operands[idx, 5], operands[idx, 4]]);
                y.AddRange([operands[idx, 3], operands[idx, 2], operands[idx, 1]]);
            }
            else if (State.Dimensionality == 3)
            {
                x.AddRange([operands[idx, 6], operands[idx, 5]]);
                y.AddRange([operands[idx, 4], operands[idx, 3]]);
                z.AddRange([operands[idx, 2], operands[idx, 1]]);
            }

            var delta = (idx + 1) % State.MultiByteValue;

            if ((idx + 1) == operands.Count && delta != 0)
            {
                var remaining = State.MultiByteValue - delta;
                
                for (int i = 0; i < remaining; i++)
                {
                    if (State.Dimensionality == 2)
                    {
                        x.AddRange(_twoDimensionalZero);
                        y.AddRange(_twoDimensionalZero);
                    }
                    else if (State.Dimensionality == 3)
                    {
                        x.AddRange(_threeDimensionalZero);
                        y.AddRange(_threeDimensionalZero);
                        z.AddRange(_threeDimensionalZero);
                    }
                }
                
                zeroFill = true;
            }

            if (zeroFill || delta == 0)
            {
                if (isInt)
                {
                    x.Reverse();
                    y.Reverse();
                    z.Reverse();
                }

                var dx = isInt ? ConvertBitsToByte(x) : ConvertBitsToFraction(x);
                var dy = isInt ? ConvertBitsToByte(y) : ConvertBitsToFraction(y);
                var dz = State.Dimensionality == 3 ? (isInt ? ConvertBitsToByte(z) : ConvertBitsToFraction(z)) : 0;

                x.Clear();
                y.Clear();
                z.Clear();

                verts.Add(new Vector3(dx, dy, dz));
            }
        }

        return verts;
    }
}
