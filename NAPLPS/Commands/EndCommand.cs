// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

public class EndCommand : NaplpsCommand
{
    public EndCommand(byte opcode, List<byte> operands) : base(opcode, operands) { }
}