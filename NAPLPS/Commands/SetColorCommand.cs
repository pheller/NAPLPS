// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The SET COLOR command is used to specify color values applied to all subsequent graphics commands.
/// </summary>
[AddCommand(220, "Set Color", "Define an RGB color or allocate a palette entry (mode-dependent).", Category = CommandCategory.Attribute, DslKeyword = "setColor")]
public class SetColorCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValue;

    public NaplpsColor Color { get; set; }

    public SetColorCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (State.SystemType == NaplpsSystemType.Prodigy)
        {
            ApplyProdigy(operands);
            return;
        }

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

    /// <summary>
    /// Prodigy MVDI has a fixed 16-color hardware palette (see
    /// <see cref="NaplpsState.ColorMapProdigyDefaults"/>) and ignores SET COLOR palette
    /// redefinition (color modes 1 and 2) - the device never reprograms the CLUT. Drawing
    /// colors come from SELECT COLOR indexing the fixed palette. Confirmed against the reference
    /// render.
    /// </summary>
    private void ApplyProdigy(NaplpsOperands operands)
    {
        if (Operands.Count == 0)
        {
            // No operands: transparent, as in the generic path. Never a palette write.
            State.IsTransparent = true;
            return;
        }

        // Always decode for round-trip fidelity / inspection, but do NOT write the CLUT.
        Color = ParseColorComponents(operands);

        if (State.ColorMode == 0)
        {
            // Mode 0 sets the drawing color directly. The hardware palette is fixed, so the
            // color resolves to the nearest of the 16 fixed entries rather than an off-grid
            // RGB. (The Prodigy ad/screen corpus is entirely mode 1; mode 0 is rare and the
            // exact device mapping is not yet pinned - nearest-entry is the safe default.)
            var entry = NearestProdigyEntry(Color);
            State.ColorMapForeground = entry;
            State.Foreground = State.ColorMap.TryGetValue(entry, out var fixedColor) ? fixedColor : Color;
            State.IsTransparent = false;
        }
        // Mode 1 / mode 2: palette redefinition - ignored by the fixed-palette hardware. No-op.
    }

    /// <summary>Index of the fixed Prodigy palette entry closest to <paramref name="c"/> (Euclidean RGB).</summary>
    private byte NearestProdigyEntry(NaplpsColor c)
    {
        byte best = State.ColorMapForeground;
        long bestDist = long.MaxValue;

        foreach (var kvp in State.ColorMap)
        {
            long dr = kvp.Value.Red - c.Red;
            long dg = kvp.Value.Green - c.Green;
            long db = kvp.Value.Blue - c.Blue;
            long dist = dr * dr + dg * dg + db * db;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = kvp.Key;
            }
        }

        return best;
    }

    private static NaplpsColor ParseColorComponents(NaplpsOperands operands)
    {
        int green = 0, red = 0, blue = 0;

        // Do NOT mutate `operands` (shared with the command's Operands collection so the
        // serializer round-trips exact bytes). Use a local padded view only for bit extraction.
        var padded = new List<byte>(operands);
        while (padded.Count < 3)
        {
            padded.Add(padded.Count > 0 ? padded[^1] : (byte)0);
        }

        foreach (var b in padded)
        {
            // Extract bits for each color component from the first triplet
            green = green << 1 | (b >> 5 & 0x1);
            red = red << 1 | (b >> 4 & 0x1);
            blue = blue << 1 | (b >> 3 & 0x1);

            // Extract bits for each color component from the second triplet
            green = green << 1 | (b >> 2 & 0x1);
            red = red << 1 | (b >> 1 & 0x1);
            blue = blue << 1 | (b & 0x1);
        }

        // ANSI X3.110 §5.3.2.5.2: "the maximum color fraction attainable,
        // given the number of bits specified, shall be interpreted as full intensity."
        // Scale from N-bit range to 8-bit (0-255). Use padded.Count since bits extracted above.
        int bitsPerComponent = padded.Count * 2;
        int maxVal = (1 << bitsPerComponent) - 1;

        if (maxVal > 0 && maxVal < 255)
        {
            green = green * 255 / maxVal;
            red = red * 255 / maxVal;
            blue = blue * 255 / maxVal;
        }

        return new NaplpsColor((byte)green, (byte)red, (byte)blue);
    }
}