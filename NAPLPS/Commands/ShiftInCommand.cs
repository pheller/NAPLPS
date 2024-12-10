// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Text;

namespace NAPLPS.Commands;

public class ShiftInCommand : GeometricDrawingCommandBase
{
    public ShiftInCommand(NaplpsState state, NaplpsOperands operands) : base(state, SHIFT_IN, operands)
    {
        State.GL = 0;
        State.InLockingManner = true;
        Text = Encoding.ASCII.GetString(Operands.ToArray()).Replace(((char)26).ToString(), string.Empty);
        Field = State.Field;
    }

    public string Text { get; }

    public NaplpsField Field { get; }
}