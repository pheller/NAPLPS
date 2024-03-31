// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PointSetAbsoluteCommand : PointCommand
{
    public PointSetAbsoluteCommand(NaplpsState state, NaplpsOperands operands) : base(state, POINT_SET_ABS, operands)
    {
    }
}