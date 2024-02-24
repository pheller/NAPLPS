// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

/// <summary>
/// The DOMAIN command is used to control the precision of
/// single-value and multi-value operands, the dimensionality of
/// coordinate specifications, and the size of the logical pel.
///
/// Once set, these parameters do not change until acted upon by either the
/// RESET command, another DOMAIN command, or the NSR control code
/// </summary>
public class DomainCommand : NaplpsCommand
{
    public ushort SingleValueLength { get; }

    public ushort MultiValueLength { get; }

    public bool IsTwoDimensional { get; }

    public DomainCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        SingleValueLength = ConvertBitsToByte(Operands[0].GetBit(1), Operands[0].GetBit(2));
        SingleValueLength++;

        MultiValueLength = ConvertBitsToByte(Operands[0].GetBit(3), Operands[0].GetBit(4), Operands[0].GetBit(5));
        MultiValueLength++;

        IsTwoDimensional = Operands[0].GetBit(6);
    }
}