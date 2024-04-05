// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS.Commands;

/// <summary>
/// </summary>
public class SetColorCommand : NaplpsCommand
{
    public NaplpsColor Color;

    public SetColorCommand(NaplpsState state, NaplpsOperands operands) : base(state, SET_COLOR, operands)
    {
        if (State.ColorMode == 0 && Operands.Count == 0)
        {
            State.IsTransparent = true;
        }
        else if (State.ColorMode == 0)
        {
            State.Foreground = Color = ParseColorComponents(operands);
        }
        else if (State.ColorMode == 1 || State.ColorMode == 2)
        {
            Color = ParseColorComponents(operands);

            State.ColorMap[State.ColorMapForegroundSelected] = Color;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private NaplpsColor ParseColorComponents(NaplpsOperands operands)
    {
        var color = new NaplpsColor();

        while (operands.Count < 3)
        {
            operands.Add(operands[^1]); // Adds the last element in the list
        }

        foreach (var b in operands)
        {
            // Extract bits for each color component from the first triplet
            color.Green = (byte)(color.Green << 1 | b >> 5 & 0x1);
            color.Red = (byte)(color.Red << 1 | b >> 4 & 0x1);
            color.Blue = (byte)(color.Blue << 1 | b >> 3 & 0x1);

            // Extract bits for each color component from the second triplet
            color.Green = (byte)(color.Green << 1 | b >> 2 & 0x1);
            color.Red = (byte)(color.Red << 1 | b >> 1 & 0x1);
            color.Blue = (byte)(color.Blue << 1 | b & 0x1);
        }

        return color;
    }
}