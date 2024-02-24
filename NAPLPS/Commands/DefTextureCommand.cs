// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System;
using System.Collections.Generic;

namespace NAPLPS.Commands;

internal class DefTextureCommand : NaplpsCommand
{
    public ushort MaskId { get; }

    public DefTextureCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        if (operands.Count != 2 && (EscapeCommands)operands[0] != EscapeCommands.DEF_TEXTURE)
        {
            throw new ArgumentOutOfRangeException(nameof(operands));
        }

        if (operands[1] == 0x41)
        {
            MaskId = 0;
        }
        else if (operands[1] == 0x42)
        {
            MaskId = 1;
        }
        else if (operands[1] == 0x43)
        {
            MaskId = 2;
        }
        else if (operands[1] == 0x44)
        {
            MaskId = 3;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(operands));
        }
    }
}