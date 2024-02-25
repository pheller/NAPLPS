// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;
using static NAPLPS.NaplpsEscapeCommands;
using static NAPLPS.NaplpsCommands;
using System.Drawing;

namespace NAPLPS.Commands;

public class NaplpsCommand(NaplpsCommands opcode, List<byte> operands)
{
    public NaplpsCommands OpCode { get; } = opcode;

    public List<byte> Operands { get; } = operands;



    public ushort MultiByteValue { get; internal set; } = 3; // TODO: Default, use statemachine to determine

    public ushort SingleByteValue { get; internal set; } = 1; // TODO: Default, use statemachine to determine

    public ushort Dimensionality { get; internal set; } = 2;
    
    public Point LogicalPel { get; internal set; } = new (1, 1);


    public static NaplpsCommand Factory(NaplpsCommands opcode, List<byte> operands)
    {
        return opcode switch
        {
            CANCEL => new NaplpsCommand(opcode, operands),
            NSR => new NaplpsCommand(opcode, operands),
            ESC => ProcessEscapeSequence(opcode, operands),
            SHIFT_OUT => new NaplpsCommand(opcode, operands),
            SHIFT_IN => new NaplpsCommand(opcode, operands),
            RESET => new ResetCommand(operands),
            DOMAIN => new DomainCommand(operands),
            WAIT => new WaitCommand(operands),
            SELECT_COLOR => new SelectColorCommand(operands),
            SET_COLOR => new SetColorCommand(operands),
            TEXTURE => new TextureCommand(operands),
            POLYGON_OUTLINED => new PolygonOutlinedCommand(operands),
            POLYGON_FILLED => new PolygonFilledCommand(operands),
            POLYGON_SET_OUTLINED => new PolygonSetOutlinedCommand(operands),
            POLYGON_SET_FILLED => new PolygonSetFilledCommand(operands),
            RECTANGLE_OUTLINED => new RectangleOutlinedCommand(operands),
            RECTANGLE_FILLED => new RectangleFilledCommand(operands),
            RECTANGLE_SET_OUTLINED => new RectangleSetOutlinedCommand(operands),
            RECTANGLE_SET_FILLED => new RectangleSetFilledCommand(operands),
            POINT_ABS => new PointAbsoluteCommand(operands),
            POINT_REL => new PointRelativeCommand(operands),
            POINT_SET_ABS => new PointSetAbsoluteCommand(operands),
            POINT_SET_REL => new PointSetRelativeCommand(operands),
            LINE_ABS => new LineAbsoluteCommand(operands),
            LINE_REL => new LineRelativeCommand(operands),
            LINE_SET_ABS => new LineSetAbsoluteCommand(operands),
            LINE_SET_REL => new LineSetRelativeCommand(operands),
            _ => BreakAndReturn(opcode, operands),
        };
    }

    private static NaplpsCommand ProcessEscapeSequence(NaplpsCommands opcode, List<byte> operands)
    {
        if (operands.Count == 0)
        {
            return new EscCommand(operands);
        }

        return (NaplpsEscapeCommands)operands[0] switch
        {
            DEF_TEXTURE => new DefTextureCommand(operands),
            END => new EndCommand(operands),
            _ => BreakAndReturn(opcode, operands),
        };
    }

    private static NaplpsCommand BreakAndReturn(NaplpsCommands opcode, List<byte> operands)
    {
        var newUnknownCommand = new NaplpsCommand(opcode, operands);

        Debugger.Break();

        return newUnknownCommand;
    }

    public static byte ConvertBitsToByte(List<bool> booleans)
    {
        byte result = 0;
        int maxBits = 8;

        for (int i = 0; i < Math.Min(booleans.Count, maxBits); i++)
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