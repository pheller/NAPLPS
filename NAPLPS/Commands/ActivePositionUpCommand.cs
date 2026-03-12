// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ActivePositionUpCommand : NaplpsCommand
{
    public ActivePositionUpCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        // State.MovePen(State.TextInterrowSpacing)
    }
}
