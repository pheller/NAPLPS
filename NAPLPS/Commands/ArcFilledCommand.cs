// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ArcFilledCommand : ArcCommand
{
    public ArcFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_FILLED, operands)
    {
        ShouldFill = true;
    }
}