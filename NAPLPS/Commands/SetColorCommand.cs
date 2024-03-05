// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Drawing;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

/// <summary>
/// </summary>
public class SetColorCommand : NaplpsCommand
{
    public Color ForegroundColor;

    public SetColorCommand(NaplpsState state, List<byte> operands) : base(state, SET_COLOR, operands)
    {
        if (State.ColorMode == 0 && Operands.Count == 0)
        {
            State.IsTransparent = true;
        }
        else if (State.ColorMode == 0)
        {
            ParseColorComponents(operands);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void ParseColorComponents(List<byte> operands)
    {
        State.DrawForgroundGreen = 0;
        State.DrawForgroundRed = 0;
        State.DrawForgroundBlue = 0;

        while (operands.Count < 3)
        {
            operands.Add(operands[^1]); // Adds the last element in the list
        }

        foreach (var b in operands)
        {
            // Extract bits for each color component from the first triplet
            State.DrawForgroundGreen = (byte)(State.DrawForgroundGreen << 1 | b >> 5 & 0x1);
            State.DrawForgroundRed = (byte)(State.DrawForgroundRed << 1 | b >> 4 & 0x1);
            State.DrawForgroundBlue = (byte)(State.DrawForgroundBlue << 1 | b >> 3 & 0x1);

            // Extract bits for each color component from the second triplet
            State.DrawForgroundGreen = (byte)(State.DrawForgroundGreen << 1 | b >> 2 & 0x1);
            State.DrawForgroundRed = (byte)(State.DrawForgroundRed << 1 | b >> 1 & 0x1);
            State.DrawForgroundBlue = (byte)(State.DrawForgroundBlue << 1 | b & 0x1);
        }

        ForegroundColor = Color.Empty.From6BitRGB(State.DrawForgroundRed, State.DrawForgroundGreen, State.DrawForgroundBlue);
    }
}