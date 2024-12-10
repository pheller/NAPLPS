// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>Required to logically delimit the end of the unprotected field transmission</summary>
public class SDCCommand : NaplpsCommand
{
    public SDCCommand(NaplpsState state, NaplpsOperands operands) : base(state, SDC, operands)
    {
    }
}
