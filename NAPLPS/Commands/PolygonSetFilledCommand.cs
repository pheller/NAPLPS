// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Diagnostics;

namespace NAPLPS.Commands;

public class PolygonSetFilledCommand : PolygonSetCommand
{
    public PolygonSetFilledCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        ShouldFill = true;
    }
}