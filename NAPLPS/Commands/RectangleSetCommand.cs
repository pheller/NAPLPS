// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public abstract class RectangleSetCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; }

    public Vector3 Dimensions { get; }

    public RectangleSetCommand(NaplpsCommands opcode, List<byte> operands) : base(opcode, operands)
    {
        var verts = ProcessVerticies(operands);

        StartPoint = verts[0];
        Dimensions = verts[1];
    }
}