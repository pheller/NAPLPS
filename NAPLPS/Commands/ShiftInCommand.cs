// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Text;

namespace NAPLPS.Commands;

public class ShiftInCommand : NaplpsCommand
{
    public ShiftInCommand(NaplpsState state, NaplpsOperands operands) : base(state, SHIFT_IN, operands)
    {
        State.GL = 0;
        State.InLockingManner = true;
        Text = Encoding.ASCII.GetString(Operands.ToArray()).Replace(((char)26).ToString(), string.Empty);
    }

    public string Text { get; }
}