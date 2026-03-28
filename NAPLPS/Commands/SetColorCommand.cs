// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The SET COLOR command is used to specify color values applied to all subsequent graphics commands.
/// </summary>
public class SetColorCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValue;

    public NaplpsColor Color { get; set; }

    public SetColorCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (State.ColorMode == 0 && Operands.Count == 0)
        {
            State.IsTransparent = true;
        }
        else if (State.ColorMode == 0)
        {
            State.Foreground = Color = ParseColorComponents(operands);

            // Mode 0 palette auto-allocation per NAPLPS spec:
            // Check if this color already exists in the palette
            byte? matchingEntry = null;

            foreach (var kvp in State.ColorMap)
            {
                if (kvp.Value.Equals(Color))
                {
                    if (matchingEntry == null || kvp.Key < matchingEntry.Value)
                    {
                        matchingEntry = kvp.Key;
                    }
                }
            }

            if (matchingEntry != null)
            {
                // Color already exists - use the lowest matching palette entry
                State.ColorMapForeground = matchingEntry.Value;
                State.UsedPaletteEntries.Add(matchingEntry.Value);
            }
            else
            {
                // Find the lowest unused palette entry (skip 0x00 nominal black and 0x07 nominal white)
                byte? freeEntry = null;

                for (byte i = 0; i <= 0xF; i++)
                {
                    if (i == 0x00 || i == 0x07)
                    {
                        continue;
                    }

                    if (!State.UsedPaletteEntries.Contains(i))
                    {
                        freeEntry = i;
                        break;
                    }
                }

                if (freeEntry != null)
                {
                    // Define the free entry with the new color and use it
                    State.ColorMap[freeEntry.Value] = Color;
                    State.ColorMapForeground = freeEntry.Value;
                    State.UsedPaletteEntries.Add(freeEntry.Value);
                }
                else
                {
                    // No unused entries - implementation-dependent: just use the color directly
                    // State.Foreground is already set above
                }
            }
        }
        else if ((State.ColorMode == 1 || State.ColorMode == 2) && Operands.Count != 0)
        {
            Color = ParseColorComponents(operands);

            State.ColorMap[State.ColorMapForeground] = Color;
        }
        else if (Operands.Count == 0)
        {
            State.IsTransparent = true;
            State.ColorMap[State.ColorMapForeground] = NaplpsColor.Black;
        }
        else
        {
            // Unknown color mode combination - treat as transparent
            State.IsTransparent = true;
        }
    }

    private static NaplpsColor ParseColorComponents(NaplpsOperands operands)
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