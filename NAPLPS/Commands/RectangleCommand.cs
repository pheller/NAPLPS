// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public abstract class RectangleCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 Dimensions { get; }

    public RectangleCommand(NaplpsCommands opcode, List<byte> operands) : base(opcode, operands)
    {
        Dimensions = ProcessVerticies(operands).FirstOrDefault();
    }
}