// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class CancelCommand(NaplpsState state, List<byte> operands) : NaplpsCommand(state, CANCEL, operands)
{
}