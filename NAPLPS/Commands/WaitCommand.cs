// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class WaitCommand : NaplpsCommand
{
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

        var rawWaitTime = operands[1];

        WaitTime = (byte)(rawWaitTime - 64);

        if (operands.Count > 2)
        {
            for (int i = 2; i < operands.Count; i++)
            {
                rawWaitTime = operands[i];
                var waitTime = (byte)(rawWaitTime - 64);

                WaitTimes.Add(waitTime);
            }
        }
    }
}