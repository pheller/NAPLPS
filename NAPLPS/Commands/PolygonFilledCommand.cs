// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

public class PolygonFilledCommand : PolygonCommand
{
    public PolygonFilledCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        ShouldFill = true;
    }
}