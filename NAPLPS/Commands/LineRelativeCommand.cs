// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineRelativeCommand : LineCommand
{
    public LineRelativeCommand(NaplpsState state, NaplpsOperands operands) : base(state, LINE_REL, operands)
    {

    }
}