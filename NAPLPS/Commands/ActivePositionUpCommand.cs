// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ActivePositionUpCommand : NaplpsCommand
{
    public ActivePositionUpCommand(NaplpsState state, NaplpsOperands operands) : base(state, AP_UP, operands)
    {
        // State.MovePen(State.TextInterrowSpacing)
    }
}
