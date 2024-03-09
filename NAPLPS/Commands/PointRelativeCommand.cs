// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PointRelativeCommand(NaplpsState state, NaplpsOperands operands) : PointCommand(state, POINT_REL, operands)
{
}