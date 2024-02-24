// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Numerics;

namespace NAPLPS.Commands;

public abstract class GeometricDrawingCommandBase : NaplpsCommand
{
    public List<Vector3> Vertices { get; internal set; } = new();

    public GeometricDrawingCommandBase(byte opcode, List<byte> operands) : base(opcode, operands) { }

    internal List<Vector3> ProcessVerticies(List<byte> operands)
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

            if ((idx + 1) % MultiByteValue == 0)
            {
                var dx = ConvertBitsToFraction(x);
                var dy = ConvertBitsToFraction(y);
                var dz = Dimensionality == 3 ? ConvertBitsToFraction(z) : 0;

                x.Clear();
                y.Clear();
                z.Clear();

                verts.Add(new Vector3(dx, dy, dz));
            }
        }

        return verts;
    }
}