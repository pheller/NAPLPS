// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NAPLPS.Commands;

public abstract class PolygonSetCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; }

    public PolygonSetCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        StartPoint = ProcessVerticies(operands.Take(MultiByteValue).ToList()).FirstOrDefault();

        Vertices = ProcessVerticies(operands.Skip(MultiByteValue).ToList());
    }
}