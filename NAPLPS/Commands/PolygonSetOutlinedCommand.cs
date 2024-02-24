// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Diagnostics;

namespace NAPLPS.Commands;

public class PolygonSetOutlinedCommand : PolygonSetCommand
{
    public PolygonSetOutlinedCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        ShouldOutline = true;
    }
}