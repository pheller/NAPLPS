// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Diagnostics;

namespace NAPLPS.Commands;

public class WaitCommand : NaplpsCommand
{
    /// <summary>The time to wait in 1/10 second increments</summary>
    public byte WaitTime { get; }

    public List<byte> WaitTimes { get; } = new();

    public WaitCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        if (operands.Count < 2)
        {
            throw new Exception("Wait command must have at least 2 operands");
        }

        if (operands[0] != 92)
        {
            throw new Exception("Wait command requies a fixed format byte, and it must be 92, 0x5C, 01011100");
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