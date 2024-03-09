// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineRelativeCommand(NaplpsState state, NaplpsOperands operands) : LineCommand(state, LINE_REL, operands)
{
}