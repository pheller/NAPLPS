// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NAPLPS.Commands;

public abstract class PolygonCommand : FillableGeometricDrawingCommandBase
{
    public PolygonCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        Vertices = ProcessVerticies(operands);
    }
}