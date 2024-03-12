// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ArcSetOutlinedCommand : ArcCommand
{
    public ArcSetOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_SET_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}