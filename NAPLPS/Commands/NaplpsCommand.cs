// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;
using static NAPLPS.NaplpsEscapeCommands;
using static NAPLPS.NaplpsCommands;
using System.Drawing;

namespace NAPLPS.Commands;

public class NaplpsCommand(NaplpsState state, NaplpsCommands opcode, List<byte> operands)
{
    public NaplpsCommands OpCode { get; } = opcode;

    public List<byte> Operands { get; } = operands;

    public NaplpsState State { get; } = state;

    public static NaplpsCommand Factory(NaplpsState state, NaplpsCommands opcode, List<byte> operands)
    {
        return opcode switch
        {
            CANCEL => new CancelCommand(state, operands),
            NSR => new NonSelectiveResetCommand(state, operands),
            ESC => ProcessEscapeSequence(state, opcode, operands),
            SHIFT_OUT => new ShiftOutCommand(state, operands),
            SHIFT_IN => new ShiftInCommand(state, operands),
            RESET => new ResetCommand(state, operands),
            DOMAIN => new DomainCommand(state, operands),
            WAIT => new WaitCommand(state, operands),
            SELECT_COLOR => new SelectColorCommand(state, operands),
            SET_COLOR => new SetColorCommand(state, operands),
            TEXTURE => new TextureCommand(state, operands),
            POLYGON_OUTLINED => new PolygonOutlinedCommand(state, operands),
            POLYGON_FILLED => new PolygonFilledCommand(state, operands),
            POLYGON_SET_OUTLINED => new PolygonSetOutlinedCommand(state, operands),
            POLYGON_SET_FILLED => new PolygonSetFilledCommand(state, operands),
            RECTANGLE_OUTLINED => new RectangleOutlinedCommand(state, operands),
            RECTANGLE_FILLED => new RectangleFilledCommand(state, operands),
            RECTANGLE_SET_OUTLINED => new RectangleSetOutlinedCommand(state, operands),
            RECTANGLE_SET_FILLED => new RectangleSetFilledCommand(state, operands),
            POINT_ABS => new PointAbsoluteCommand(state, operands),
            POINT_REL => new PointRelativeCommand(state, operands),
            POINT_SET_ABS => new PointSetAbsoluteCommand(state, operands),
            POINT_SET_REL => new PointSetRelativeCommand(state, operands),
            LINE_ABS => new LineAbsoluteCommand(state, operands),
            LINE_REL => new LineRelativeCommand(state, operands),
            LINE_SET_ABS => new LineSetAbsoluteCommand(state, operands),
            LINE_SET_REL => new LineSetRelativeCommand(state, operands),
            _ => BreakAndReturn(state, opcode, operands),
        };
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

        foreach (bool bit in boolList.Skip(1))
        {
            fraction += bit ? baseValue : 0f;
            baseValue /= 2f;
        }

        // Interpret the list as a two's complement fixed-point number
        // Assuming the first bit is the sign bit and the rest are fractional bits
        if (boolList.First()) // If the number is negative
        {
            fraction = -1 + fraction; // Adjust for two's complement notation
        }

        return fraction;
    }

    public static int ConvertBitsToInt(List<bool> binaryArray)
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

    private static NaplpsCommand ProcessEscapeSequence(NaplpsState state, NaplpsCommands opcode, List<byte> operands)
    {
        if (operands.Count == 0)
        {
            return new EscCommand(state, operands);
        }

        return (NaplpsEscapeCommands)operands[0] switch
        {
            DEF_TEXTURE => new DefTextureCommand(state, operands),
            END => new EndCommand(state, operands),
            _ => BreakAndReturn(state, opcode, operands),
        };
    }

    private static NaplpsCommand BreakAndReturn(NaplpsState state, NaplpsCommands opcode, List<byte> operands)
    {
        var newUnknownCommand = new NaplpsCommand(state, opcode, operands);

        Debugger.Break();

        return newUnknownCommand;
    }

}