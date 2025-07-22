// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The DOMAIN command is used to control the precision of
/// single-value and multi-value operands, the dimensionality of
/// coordinate specifications, and the size of the logical pel.
///
/// Once set, these parameters do not change until acted upon by either the
/// RESET command, another DOMAIN command, or the NSR control code
/// </summary>
public class DomainCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.FixedAndMultiValue;

    public DomainCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (Operands.Count == 0)
        {
            return;
        }

        var (singleByteValue, multiByteValue, dimensionality) = ProcessFixedByte(Operands);

        State.SingleByteValue = singleByteValue;
        State.MultiByteValue = multiByteValue;
        State.Dimensionality = dimensionality;

        if (operands.Count > 1)
        {
            Vertices = ProcessVertices(Operands[1..], false);

            State.LogicalPel = Vertices != null ? new Vector2(Vertices[0].X, Vertices[0].Y) : Vector2.Zero;
        }
    }

    public static (byte, byte, byte) ProcessFixedByte(byte operand)
    {
        return ProcessFixedByte(new NaplpsOperands([operand]));
    }

    public static (byte, byte, byte) ProcessFixedByte(NaplpsOperands operands)
    {
        if (operands.Count < 1)
        {
            return (0, 0, 0);
        }

        byte singleByteValue = ConvertBitsToByte([operands[0, 1], operands[0, 2]]);
        singleByteValue++;

        byte multiByteValue = ConvertBitsToByte([operands[0, 3], operands[0, 4], operands[0, 5]]);
        multiByteValue++;

        byte dimensionality = (byte)(operands[0, 6] ? 3 : 2);

        return (singleByteValue, multiByteValue, dimensionality);
    }
}
