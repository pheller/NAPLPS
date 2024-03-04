// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class EndCommand(NaplpsState state, List<byte> operands) : NaplpsCommand(state, ESC, operands)
{
} 