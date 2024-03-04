// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class PointAbsoluteCommand(NaplpsState state, List<byte> operands) : PointCommand(state, POINT_ABS, operands)
{
}