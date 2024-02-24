// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

public class LineSetRelativeCommand : LineCommand
{
    public LineSetRelativeCommand(byte opcode, List<byte> operands) : base(opcode, operands) { }
}