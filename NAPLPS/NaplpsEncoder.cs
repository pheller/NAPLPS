// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

/// <summary>
/// Static utility class that encodes high-level values into NAPLPS operand bytes.
/// This is the reverse (write path) of the decoding done in GeometricDrawingCommandBase.ProcessVertices().
/// </summary>
public static class NaplpsEncoder
{
    // Operand bytes use 0xC0 base (bits 7+8 set) so they fall in the 8-bit numerical data range
    // (0xC0-0xFF). This ensures they're recognized by IsValidNumericalDataNext during reparse.
    // ProcessVertices only reads bits 1-6, so the high bits don't affect coordinate values.
    private const byte NumericalDataBase = 0xC0;

    /// <summary>
    /// Encodes a 2D vertex (x, y) into NAPLPS operand bytes.
    /// Reverse of ProcessVertices() for 2D mode.
    /// For multiByteValue=3: produces 3 bytes where each byte has bits [6,5,4]=x and [3,2,1]=y,
    /// with bits 7+8 set (numerical data range 0xC0-0xFF for 8-bit mode).
    /// </summary>
    public static NaplpsOperands EncodeVertex2D(float x, float y, int multiByteValue = 3)
    {
        // Each byte contributes 3 bits per axis in 2D mode
        int bitsPerAxis = multiByteValue * 3; // 9 bits for multiByteValue=3

        var xBits = ConvertFractionToBits(x, bitsPerAxis);
        var yBits = ConvertFractionToBits(y, bitsPerAxis);

        var operands = new NaplpsOperands();

        for (int i = 0; i < multiByteValue; i++)
        {
            byte b = NumericalDataBase;

            // X bits go into positions 6, 5, 4 (1-indexed) = bit positions 5, 4, 3 (0-indexed)
            int xOffset = i * 3;
            if (xBits[xOffset]) b |= 1 << 5;     // bit 6 (1-indexed)
            if (xBits[xOffset + 1]) b |= 1 << 4;  // bit 5
            if (xBits[xOffset + 2]) b |= 1 << 3;  // bit 4

            // Y bits go into positions 3, 2, 1 (1-indexed) = bit positions 2, 1, 0 (0-indexed)
            int yOffset = i * 3;
            if (yBits[yOffset]) b |= 1 << 2;      // bit 3 (1-indexed)
            if (yBits[yOffset + 1]) b |= 1 << 1;   // bit 2
            if (yBits[yOffset + 2]) b |= 1 << 0;   // bit 1

            operands.Add(b);
        }

        return operands;
    }

    /// <summary>
    /// Encodes multiple 2D vertices into concatenated NAPLPS operand bytes.
    /// </summary>
    public static NaplpsOperands EncodeVertices2D(Vector3[] vertices, int multiByteValue = 3)
    {
        var operands = new NaplpsOperands();

        foreach (var vertex in vertices)
        {
            var vertexOperands = EncodeVertex2D(vertex.X, vertex.Y, multiByteValue);
            operands.AddRange(vertexOperands);
        }

        return operands;
    }

    /// <summary>
    /// Encodes a SelectColor operand for color mode 1 (foreground only).
    /// The color index is stored in bits 6,5,4,3 (1-indexed) of a single operand byte.
    /// </summary>
    public static NaplpsOperands EncodeSelectColorForeground(byte fgIndex)
    {
        var operands = new NaplpsOperands();

        // Color index goes into bits 6,5,4,3 (1-indexed) = bit positions 5,4,3,2 (0-indexed)
        byte b = NumericalDataBase;
        b |= (byte)((fgIndex & 0x0F) << 2); // shift 4-bit index into bits 5,4,3,2

        operands.Add(b);

        return operands;
    }

    /// <summary>
    /// Encodes SelectColor operands for color mode 2 (foreground + background).
    /// Each color index is stored in bits 6,5,4,3 (1-indexed) of separate operand bytes.
    /// </summary>
    public static NaplpsOperands EncodeSelectColorForegroundBackground(byte fgIndex, byte bgIndex)
    {
        var operands = new NaplpsOperands();

        byte fgByte = NumericalDataBase;
        fgByte |= (byte)((fgIndex & 0x0F) << 2);
        operands.Add(fgByte);

        byte bgByte = NumericalDataBase;
        bgByte |= (byte)((bgIndex & 0x0F) << 2);
        operands.Add(bgByte);

        return operands;
    }
}
