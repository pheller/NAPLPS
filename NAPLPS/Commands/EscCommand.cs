// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class EscCommand(List<byte> operands) : NaplpsCommand(ESC, operands)
{
}