// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineAbsoluteCommand : LineCommand
{
    public LineAbsoluteCommand(NaplpsState state, NaplpsOperands operands) : base(state, LINE_ABS, operands)
    {
    }
}