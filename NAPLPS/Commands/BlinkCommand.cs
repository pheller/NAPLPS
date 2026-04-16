// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// BLINK command (0x3F/0xBF) - Sets up palette animation/blinking.
/// Creates color cycling effects by modifying palette entries over time.
/// The blink-from palette entry comes from the current foreground color index
/// (set by the preceding SelectColor command).
/// Operand format: [blink-to palette entry (single value)] [on-interval] [off-interval] [start-delay]
/// If additional data bytes follow the start delay, another blink process is started implicitly
/// with the palette entry incremented using the SET COLOR incrementing algorithm.
/// </summary>
[AddCommand(240, "Blink", "Animate a palette entry between two colors over specified on/off intervals.", Category = CommandCategory.Attribute, DslKeyword = "blink")]
internal class BlinkCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.FixedAndSingleValue;

    public List<BlinkProcess> Processes { get; } = new();

    public BlinkCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (operands.Count == 0)
        {
            // No operands - stop all blink processes
            state.BlinkProcesses.Clear();
            return;
        }

        // The blink-from palette entry is the CURRENT foreground color index, set by the preceding SelectColor command.
        byte paletteEntry = state.ColorMapForeground;

        // Operand 0: blink-to color as a palette entry (single value operand)
        // The number of bytes consumed by a single value is State.SingleByteValue.
        int svBytes = state.SingleByteValue;
        byte blinkToPaletteIndex = ExtractPaletteIndex(operands, 0, svBytes, state);
        var blinkToColor = state.ColorMap.GetValueOrDefault(blinkToPaletteIndex, new NaplpsColor(0, 0, 0));

        // Parse blink processes from the remaining fixed-format operands.
        // Each blink process group is 3 fixed bytes: ON interval, OFF interval, start delay.
        // NAPLPS operand bytes have header bits (0xC0 in 8-bit). Data is in bits 5-0 only.
        int fixedStart = svBytes;
        int remaining = operands.Count - fixedStart;

        if (remaining < 3)
        {
            // Not enough operands for a full blink process - use defaults
            var blinkFromColor = state.ColorMap.GetValueOrDefault(paletteEntry, new NaplpsColor(1, 1, 1));

            int onInterval = remaining > 0 ? operands[fixedStart] & 0x3F : 30;
            int offInterval = remaining > 1 ? operands[fixedStart + 1] & 0x3F : 30;

            var process = new BlinkProcess
            {
                PaletteEntry = paletteEntry,
                BlinkFromColor = blinkFromColor,
                BlinkToColor = blinkToColor,
                BlinkToPaletteEntry = blinkToPaletteIndex,
                OnInterval = onInterval,
                OffInterval = offInterval,
                StartDelay = 0,
                CycleCount = 0,
                Phase = 0
            };

            Processes.Add(process);
            state.BlinkProcesses.Add(process);
            return;
        }

        // Parse groups of 3 fixed-format operands (ON, OFF, start delay).
        // Each subsequent group implicitly increments the blink-from palette entry.
        int idx = fixedStart;

        while (idx + 2 < operands.Count)
        {
            int onInterval = operands[idx] & 0x3F;
            int offInterval = operands[idx + 1] & 0x3F;
            int startDelay = operands[idx + 2] & 0x3F;

            var blinkFromColor = state.ColorMap.GetValueOrDefault(paletteEntry, new NaplpsColor(1, 1, 1));

            var process = new BlinkProcess
            {
                PaletteEntry = paletteEntry,
                BlinkFromColor = blinkFromColor,
                BlinkToColor = blinkToColor,
                BlinkToPaletteEntry = blinkToPaletteIndex,
                OnInterval = onInterval,
                OffInterval = offInterval,
                StartDelay = startDelay,
                CycleCount = 0,
                Phase = 0
            };

            Processes.Add(process);
            state.BlinkProcesses.Add(process);

            idx += 3;

            // If more data follows, increment the palette entry for the next implicit blink process
            if (idx + 2 < operands.Count)
            {
                paletteEntry = IncrementPaletteIndex(paletteEntry, svBytes * 4);
            }
        }
    }

    /// <summary>
    /// Extracts a palette index from a single-value operand within the operand list.
    /// Follows the same bit extraction pattern as SelectColorCommand.
    /// </summary>
    private static byte ExtractPaletteIndex(NaplpsOperands operands, int startByte, int svBytes, NaplpsState state)
    {
        if (startByte >= operands.Count)
        {
            return 0;
        }

        if (svBytes == 1)
        {
            // 1-byte single value: palette index is in bits 3-6 (1-indexed)
            return ConvertBitsToByte([operands[startByte, 3], operands[startByte, 4], operands[startByte, 5], operands[startByte, 6]]);
        }
        else if (svBytes == 2)
        {
            // 2-byte single value: palette index from first byte bits 3-6
            return ConvertBitsToByte([operands[startByte, 3], operands[startByte, 4], operands[startByte, 5], operands[startByte, 6]]);
        }
        else
        {
            // 3 or 4 byte single values - extract from more bytes
            byte colorIndex = 0;

            for (int i = 0; i < Math.Min(svBytes, operands.Count - startByte); i++)
            {
                colorIndex = (byte)((colorIndex << 4) | (operands[startByte + i] & 0x0F));
            }

            return colorIndex;
        }
    }

    /// <summary>
    /// Increments a palette index using the NAPLPS SET COLOR palette incrementing algorithm.
    /// Algorithm: Take the palette entry number in binary (within the given bit width),
    /// find the most significant zero bit, change it to a one, then change all ones
    /// to the left of it to zeroes.
    /// Example (6-bit): 010100 -> 110100 -> 001100
    /// </summary>
    /// <param name="index">The current palette index.</param>
    /// <param name="bitWidth">The number of bits used for the palette index (e.g. 4 for a 16-entry palette).</param>
    public static byte IncrementPaletteIndex(byte index, int bitWidth = 4)
    {
        // Mask to the relevant bit width
        int mask = (1 << bitWidth) - 1;
        int value = index & mask;

        // Find the most significant zero bit within the bit width (scanning from MSB to LSB)
        int msZero = -1;

        for (int bit = bitWidth - 1; bit >= 0; bit--)
        {
            if ((value & (1 << bit)) == 0)
            {
                msZero = bit;
                break;
            }
        }

        if (msZero == -1)
        {
            // All bits within the width are 1 - wrap to 0
            return 0;
        }

        // Set that bit to 1
        value |= (1 << msZero);

        // Clear all bits to the left of it (higher bits within the width)
        for (int bit = msZero + 1; bit < bitWidth; bit++)
        {
            value &= ~(1 << bit);
        }

        return (byte)(value & mask);
    }
}
