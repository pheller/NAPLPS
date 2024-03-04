// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NAPLPS.Commands;

public abstract class PointCommand : GeometricDrawingCommandBase
{
    public Vector3 Point { get; internal set; }

    public PointCommand(NaplpsState state, NaplpsCommands opcode, List<byte> operands) : base(state,opcode, operands)
    {
        Point = ProcessVerticies(operands).FirstOrDefault();
    }
}