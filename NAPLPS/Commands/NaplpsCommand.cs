// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;
using static NAPLPS.Commands.EscapeCommands;
using static NAPLPS.Commands.NaplpsCommands;

namespace NAPLPS.Commands;

public class NaplpsCommand
{
    public byte OpCode { get; }

    public List<byte> Operands { get; }

    public NaplpsCommands Command => (NaplpsCommands)OpCode;

    public ushort MultiByteValue => 4; // TODO: Default, use statemachine to determine

    public ushort SingleByteValue => 1; // TODO: Default, use statemachine to determine

    public ushort Dimensionality => 2;

    public NaplpsCommand(byte opcode, List<byte> operands)
    {
        OpCode = opcode;
        Operands = operands;
    }

    

    public static NaplpsCommand Factory(byte opcode, List<byte> operands)
    {
        return (NaplpsCommands)opcode switch
        {
            CANCEL => new NaplpsCommand(opcode, operands),
            NSR => new NaplpsCommand(opcode, operands),
            ESC => ProcessEscapeSequence(opcode, operands),
            SHIFT_OUT => new NaplpsCommand(opcode, operands),
            SHIFT_IN => new NaplpsCommand(opcode, operands),
            RESET => new ResetCommand(opcode, operands),
            DOMAIN => new DomainCommand(opcode, operands),
            WAIT => new WaitCommand(opcode, operands),
            SELECT_COLOR => new SelectColorCommand(opcode, operands),
            SET_COLOR => new SetColorCommand(opcode, operands),
            TEXTURE => new TextureCommand(opcode, operands),
            POLYGON_OUTLINED => new PolygonOutlinedCommand(opcode, operands),
            POLYGON_FILLED => new PolygonFilledCommand(opcode, operands),
            POLYGON_SET_OUTLINED => new PolygonSetOutlinedCommand(opcode, operands),
            POLYGON_SET_FILLED => new PolygonSetFilledCommand(opcode, operands),
            RECTANGLE_OUTLINED => new RectangleOutlinedCommand(opcode, operands),
            RECTANGLE_FILLED => new RectangleFilledCommand(opcode, operands),
            RECTANGLE_SET_OUTLINED => new RectangleSetOutlinedCommand(opcode, operands),
            RECTANGLE_SET_FILLED => new RectangleSetFilledCommand(opcode, operands),
            POINT_ABS => new PointAbsoluteCommand(opcode, operands),
            POINT_REL => new PointRelativeCommand(opcode, operands),
            POINT_SET_ABS => new PointSetAbsoluteCommand(opcode, operands),
            POINT_SET_REL => new PointSetRelativeCommand(opcode, operands),
            LINE_ABS => new LineAbsoluteCommand(opcode, operands),
            LINE_REL => new LineRelativeCommand(opcode, operands),
            LINE_SET_ABS => new LineSetAbsoluteCommand(opcode, operands),
            LINE_SET_REL => new LineSetRelativeCommand(opcode, operands),
            _ => BreakAndReturn(opcode, operands),
        };
    }

    private static NaplpsCommand ProcessEscapeSequence(byte opcode, List<byte> operands)
    {
        if (operands.Count == 0)
        {
            return new EscCommand(opcode, operands);
        }

        return (EscapeCommands)operands[0] switch
        {
            DEF_TEXTURE => new DefTextureCommand(opcode, operands),
            END => new EndCommand(opcode, operands),
            _ => BreakAndReturn(opcode, operands),
        };
    }

    private static NaplpsCommand BreakAndReturn(byte opcode, List<byte> operands)
    {
        var newUnknownCommand = new NaplpsCommand(opcode, operands);

        Debugger.Break();

        return newUnknownCommand;
    }

    public static byte ConvertBitsToByte(params bool[] booleans)
    {
        byte result = 0;
        int maxBits = 8;

        for (int i = 0; i < Math.Min(booleans.Length, maxBits); i++)
        {
            if (booleans[i])
            {
                result |= (byte)(1 << i);
            }
        }

        return result;
    }

    public static float ConvertBitsToFraction(List<bool> boolList)
    {
        float fraction = 0f;
        float baseValue = 0.5f; // Starting from the MSB just right of the decimal point

        foreach (bool bit in boolList)
        {
            fraction += bit ? baseValue : 0f;
            baseValue /= 2f;
        }

        // Interpret the list as a two's complement fixed-point number
        // Assuming the first bit is the sign bit and the rest are fractional bits
        if (boolList[0]) // If the number is negative
        {
            fraction = -1 + fraction; // Adjust for two's complement notation
        }

        return fraction;
    }

    static int ConvertBitsToInt(List<bool> binaryArray)
    {
        if (binaryArray == null || binaryArray.Count == 0)
        {
            throw new ArgumentException("Array must not be empty or null.");
        }

        int point = 0;

        for (int i = 0; i < binaryArray.Count; i++)
        {
            if (binaryArray[i])
            {
                point |= 1 << binaryArray.Count - 1 - i;
            }
        }

        // Handling two's complement for negative numbers
        if (binaryArray[0]) // Check if the number is negative (MSB is true/1)
        {
            // Invert and add 1 to handle two's complement
            point = -(int)((~(uint)point & (1U << binaryArray.Count) - 1) + 1);
        }

        return point;
    }

    public static bool IsOpcode(byte c)
    {
        return (c & 1 << 6) == 0;
    }
}