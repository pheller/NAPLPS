// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class EndCommand(NaplpsState state, NaplpsOperands operands) : NaplpsCommand(state, ESC, operands)
{
} 