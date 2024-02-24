// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NAPLPS.Commands;

public abstract class LineCommand : GeometricDrawingCommandBase
{
    public Vector3 Point { get; internal set; }

    public LineCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        Point = ProcessVerticies(operands).FirstOrDefault();
    }
}