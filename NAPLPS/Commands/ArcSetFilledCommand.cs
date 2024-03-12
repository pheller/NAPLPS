// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ArcSetFilledCommand : ArcCommand
{
    public ArcSetFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}