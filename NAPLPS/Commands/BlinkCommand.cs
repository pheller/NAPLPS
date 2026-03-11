// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// BLINK command (0x3F/0xBF) - Sets up palette animation/blinking.
/// Creates color cycling effects by modifying palette entries over time.
/// The palette entry to animate comes from the current foreground color index
/// (set by the preceding SelectColor command), NOT from the Blink operands.
/// Operand format: [control] [on-interval] [off-interval] [start-delay]
/// Control byte (6-bit data after stripping header): bits 5-3 = phase (0-7), bits 2-0 = cycle count (0 = infinite)
/// </summary>
internal class BlinkCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.FixedAndSingleValue;

    public BlinkProcess? Process { get; }

    public BlinkCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (operands.Count == 0)
        {
            // No operands - stop all blink processes
            state.BlinkProcesses.Clear();
            return;
        }

        // The palette entry is the CURRENT foreground color index, set by the preceding SelectColor command.
        byte paletteEntry = state.ColorMapForeground;

        // Get the current color at this palette entry as the blink-from color
        var blinkFromColor = state.ColorMap.GetValueOrDefault(paletteEntry, new NaplpsColor(1, 1, 1));

        // Blink-to color is black by default
        var blinkToColor = new NaplpsColor(0, 0, 0);

        // NAPLPS operand bytes have header bits set (0xC0 in 8-bit, 0x40 in 7-bit).
        // Actual data is in bits 5-0 only. Strip with & 0x3F.

        // Operand 0: Control byte — bits 5-3 = phase (0-7), bits 2-0 = cycle count (0 = infinite)
        byte controlData = (byte)(operands[0] & 0x3F);
        int phase = (controlData >> 3) & 0x07;
        int cycleCount = controlData & 0x07;

        // Operand 1: ON interval (in time units)
        int onInterval = operands.Count > 1 ? operands[1] & 0x3F : 30;

        // Operand 2: OFF interval (in time units)
        int offInterval = operands.Count > 2 ? operands[2] & 0x3F : 30;

        // Operand 3: Start delay (in time units)
        int startDelay = operands.Count > 3 ? operands[3] & 0x3F : 0;

        Process = new BlinkProcess
        {
            PaletteEntry = paletteEntry,
            BlinkFromColor = blinkFromColor,
            BlinkToColor = blinkToColor,
            OnInterval = onInterval,
            OffInterval = offInterval,
            StartDelay = startDelay,
            CycleCount = cycleCount,
            Phase = phase
        };

        state.BlinkProcesses.Add(Process);
    }
}
