// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// BLINK command (0x3F) - Sets up palette animation/blinking.
/// Creates color cycling effects by modifying palette entries over time.
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

        // Parse blink parameters from operands
        // Format: [control byte] [palette entry] [timing bytes...]
        var controlByte = operands[0];

        // Bits 7-4: phase (0-7 for gradual transitions)
        // Bits 3-0: cycle count (0 = infinite)
        int phase = (controlByte >> 4) & 0x07;
        int cycleCount = controlByte & 0x0F;

        byte paletteEntry = operands.Count > 1 ? operands[1] : (byte)0;

        // Get current color as blink-from color
        var blinkFromColor = state.ColorMode == 0
            ? state.Foreground
            : state.ColorMap.GetValueOrDefault(state.ColorMapForeground, new NaplpsColor(1, 1, 1));

        // Blink-to color is black (or background in color mode 2)
        var blinkToColor = state.ColorMode == 2
            ? state.ColorMap.GetValueOrDefault(state.ColorMapBackground, new NaplpsColor(0, 0, 0))
            : new NaplpsColor(0, 0, 0);

        // Parse timing if provided
        int onInterval = 30;  // Default ~0.5 seconds at 60Hz
        int offInterval = 30;
        int startDelay = 0;

        if (operands.Count > 2)
        {
            onInterval = operands[2];
        }
        if (operands.Count > 3)
        {
            offInterval = operands[3];
        }
        if (operands.Count > 4)
        {
            startDelay = operands[4];
        }

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
