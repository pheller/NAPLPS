// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ArcOutlinedCommand : ArcCommand
{
    public ArcOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}