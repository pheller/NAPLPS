// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Text;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class ShiftInCommand(NaplpsState state, List<byte> operands) : NaplpsCommand(state, SHIFT_IN, operands)
{
    public string Text => Encoding.ASCII.GetString(Operands.ToArray());
}