// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class PointSetAbsoluteCommand(NaplpsState state, List<byte> operands) : PointCommand(state, POINT_SET_ABS, operands)
{
}