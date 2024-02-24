// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

public class PointRelativeCommand : PointCommand
{
    public PointRelativeCommand(byte opcode, List<byte> operands) : base(opcode, operands) { }
}