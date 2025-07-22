// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Collections;

namespace NAPLPS.Commands;

public class MosaicElementCommand : NaplpsCommand
{
    public bool Bit1 { get; }

    public bool Bit2 { get; }

    public bool Bit3 { get; }

    public bool Bit4 { get; }

    public bool Bit5 { get; }

    public bool Bit6 { get; }

    public MosaicElementCommand(bool[] element, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (element.Length != 6)
        {
            throw new ArgumentException("Mosaic element must be 6 bits long");
        }

        Bit1 = element[0];
        Bit2 = element[1];
        Bit3 = element[2];
        Bit4 = element[3];
        Bit5 = element[4];
        Bit6 = element[5];
    }

    public MosaicElementCommand(byte element, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var bits = new BitArray(element);

        Bit1 = bits[0];
        Bit2 = bits[1];
        Bit3 = bits[2];
        Bit4 = bits[3];
        Bit5 = bits[4];
        Bit6 = bits[6];
    }
}
