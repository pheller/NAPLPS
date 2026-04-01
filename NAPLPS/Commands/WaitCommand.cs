// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class WaitCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.Fixed;

    /// <summary>The time to wait in 1/10 second increments</summary>
    public byte WaitTime { get; }

    public List<byte> WaitTimes { get; } = [];

    public WaitCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (operands.Count < 2)
        {
            IsValid = false;

            return;
        }

        // Wait command requies a fixed format byte, and it must be 92, 0x5C, 01011100
        if (operands[0] != 92)
        {
            IsValid = false;

            return;
        }

        // ANSI X3.110: wait interval is in bits 6 through 1 (the 6 data bits) of each byte.
        // Bit 7 is the data byte marker. Value = rawByte & 0x3F, range 0-63, in 1/10 second units.
        // Multiple bytes are summed for longer delays. Same encoding as BLINK intervals.
        var rawWaitTime = operands[1];

        WaitTime = (byte)(rawWaitTime & 0x3F);

        if (operands.Count > 2)
        {
            for (int i = 2; i < operands.Count; i++)
            {
                rawWaitTime = operands[i];
                var waitTime = (byte)(rawWaitTime & 0x3F);

                WaitTimes.Add(waitTime);
            }
        }
    }
}