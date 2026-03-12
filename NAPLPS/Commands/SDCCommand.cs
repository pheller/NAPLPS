// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>Required to logically delimit the end of the unprotected field transmission</summary>
public class SDCCommand : NaplpsCommand
{
    public SDCCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
