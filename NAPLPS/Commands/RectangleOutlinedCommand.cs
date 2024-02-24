// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

public class RectangleOutlinedCommand : RectangleCommand
{
    public RectangleOutlinedCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        ShouldOutline = true;
    }
}