// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public abstract class GeometricDrawingCommandBase(NaplpsCommands opcode, List<byte> operands) : NaplpsCommand(opcode, operands)
{
    private static readonly bool[] _twoDimensionalZero = [false, false, false];
    private static readonly bool[] _threeDimensionalZero = [false, false];

    public List<Vector3> Vertices { get; internal set; } = [];

    internal List<Vector3> ProcessVerticies(List<byte> operands, bool isInt = false)
    {
        var verts = new List<Vector3>();

        var x = new List<bool>();
        var y = new List<bool>();
        var z = new List<bool>();

        for (int idx = 0; idx < operands.Count; idx++)
        {
            var operand = operands[idx];

            if (Dimensionality == 2)
            {
                x.AddRange(new[] { operand.GetBit(6), operand.GetBit(5), operand.GetBit(4) });
                y.AddRange(new[] { operand.GetBit(3), operand.GetBit(2), operand.GetBit(1) });
            }
            else if (Dimensionality == 3)
            {
                x.AddRange(new[] { operand.GetBit(6), operand.GetBit(5) });
                y.AddRange(new[] { operand.GetBit(4), operand.GetBit(3) });
                z.AddRange(new[] { operand.GetBit(2), operand.GetBit(1) });
            }

            // We're short bytes, fill with zeros and walk away
            if (idx == operands.Count && (idx + 1) % MultiByteValue != 0)
            {
                if (Dimensionality == 2)
                {
                    x.AddRange(_twoDimensionalZero);
                    y.AddRange(_twoDimensionalZero);
                }
                else if (Dimensionality == 3)
                {
                    x.AddRange(_threeDimensionalZero);
                    y.AddRange(_threeDimensionalZero);
                    z.AddRange(_threeDimensionalZero);
                }

                if (isInt)
                {
                    x.Reverse();
                    y.Reverse();
                    z.Reverse();
                }

                var dx = isInt ? ConvertBitsToByte(x) : ConvertBitsToFraction(x);
                var dy = isInt ? ConvertBitsToByte(x) : ConvertBitsToFraction(y);
                var dz = Dimensionality == 3 ? isInt ? ConvertBitsToByte(x) : ConvertBitsToFraction(z) : 0;

                x.Clear();
                y.Clear();
                z.Clear();

                verts.Add(new Vector3(dx, dy, dz));
            }
            else if ((idx + 1) % MultiByteValue == 0)
            {
                if (isInt)
                {
                    x.Reverse();
                    y.Reverse();
                    z.Reverse();
                }

                var dx = isInt ? ConvertBitsToByte(x) : ConvertBitsToFraction(x);
                var dy = isInt ? ConvertBitsToByte(x) : ConvertBitsToFraction(y);
                var dz = Dimensionality == 3 ? isInt ? ConvertBitsToByte(x) : ConvertBitsToFraction(z) : 0;

                x.Clear();
                y.Clear();
                z.Clear();

                verts.Add(new Vector3(dx, dy, dz));
            }
        }

        return verts;
    }
}
