// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class LineRelativeCommand(NaplpsState state, List<byte> operands) : LineCommand(state, LINE_REL, operands)
{
}