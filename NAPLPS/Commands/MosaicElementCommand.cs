// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Collections;

namespace NAPLPS.Commands;

[AddCommand(120, "Mosaic Element", "A 2x3 block-graphics mosaic cell (6 bits).", Category = CommandCategory.Mosaic, DslKeyword = "mosaic")]
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

    /// <summary>
    /// Reflection-friendly constructor used by NaplpsFormat.TryInstantiateCommand when
    /// the NCR table stores the 6 bits as 6 separate <c>params object[]</c> elements
    /// (the inline collection expressions in NaplpsState's MosaicSet do this).
    /// </summary>
    public MosaicElementCommand(bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Bit1 = b1;
        Bit2 = b2;
        Bit3 = b3;
        Bit4 = b4;
        Bit5 = b5;
        Bit6 = b6;
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
