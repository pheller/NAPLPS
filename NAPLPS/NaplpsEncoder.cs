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

    /// <summary>
    /// Decode a 2D vertex from NAPLPS operand bytes. Exact inverse of
    /// <see cref="EncodeVertex2D"/>. Returns the (x, y) pair.
    /// </summary>
    public static (float x, float y) DecodeVertex2D(NaplpsOperands operands, int offset = 0, int multiByteValue = 3)
    {
        if (operands.Count == 0 || offset >= operands.Count)
        {
            return (0f, 0f);
        }

        int bitsPerAxis = multiByteValue * 3;
        var xBits = new List<bool>(bitsPerAxis);
        var yBits = new List<bool>(bitsPerAxis);

        for (int i = 0; i < multiByteValue; i++)
        {
            if (offset + i >= operands.Count)
            {
                break;
            }

            byte b = operands[offset + i];

            xBits.Add((b & (1 << 5)) != 0);
            xBits.Add((b & (1 << 4)) != 0);
            xBits.Add((b & (1 << 3)) != 0);

            yBits.Add((b & (1 << 2)) != 0);
            yBits.Add((b & (1 << 1)) != 0);
            yBits.Add((b & (1 << 0)) != 0);
        }

        float x = NaplpsUtils.ConvertBitsToFraction(xBits);
        float y = NaplpsUtils.ConvertBitsToFraction(yBits);

        return (x, y);
    }

    /// <summary>
    /// Pack 6 data bits (bits 1-6, 1-indexed) into a single operand byte with the
    /// 0xC0 numerical-data base. Use for fixed-format bytes that the parser reads
    /// via <c>operands[i, N]</c> bit accessors.
    /// </summary>
    public static byte EncodeFixedFormatByte(byte dataBits)
    {
        return (byte)(NumericalDataBase | (dataBits & 0x3F));
    }

    /// <summary>
    /// Encode a 4-bit palette index into a single operand byte for SelectColor /
    /// Blink-to-entry operands. Index occupies bits 3-6 (1-indexed).
    /// </summary>
    public static byte EncodePaletteIndexByte(byte paletteIndex)
    {
        return (byte)(NumericalDataBase | ((paletteIndex & 0x0F) << 2));
    }

    /// <summary>
    /// Encode a 6-bit "wait interval" or "blink interval" value (range 0-63, in 1/10 second
    /// units for Wait or animation-frame units for Blink). Goes in bits 1-6 of an operand byte.
    /// </summary>
    public static byte EncodeIntervalByte(byte tenthsOrFrames)
    {
        return (byte)(NumericalDataBase | (tenthsOrFrames & 0x3F));
    }

    /// <summary>
    /// Encode the Domain command's first fixed byte.
    /// Bits 1-2: singleByteValue-1 (1-4 stored as 0-3).
    /// Bits 3-5: multiByteValue-1 (1-8 stored as 0-7).
    /// Bit 6:    dimensionality (true if 3D, false if 2D).
    /// </summary>
    public static byte EncodeDomainFixedByte(byte singleByteValue, byte multiByteValue, byte dimensionality)
    {
        if (singleByteValue < 1 || singleByteValue > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(singleByteValue), "Must be 1-4.");
        }

        if (multiByteValue < 1 || multiByteValue > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(multiByteValue), "Must be 1-8.");
        }

        if (dimensionality != 2 && dimensionality != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(dimensionality), "Must be 2 or 3.");
        }

        byte data = (byte)((singleByteValue - 1) & 0x03);
        data |= (byte)(((multiByteValue - 1) & 0x07) << 2);
        data |= (byte)((dimensionality == 3 ? 1 : 0) << 5);

        return EncodeFixedFormatByte(data);
    }

    /// <summary>
    /// Encode the Texture command's first fixed byte.
    /// Bits 1-2: line texture (0-3).
    /// Bit 3:    highlight (outline filled objects).
    /// Bits 4-6: texture pattern (0-7).
    /// </summary>
    public static byte EncodeTextureFixedByte(byte lineTexture, bool highlight, byte texturePattern)
    {
        byte data = (byte)(lineTexture & 0x03);
        data |= (byte)((highlight ? 1 : 0) << 2);
        data |= (byte)((texturePattern & 0x07) << 3);

        return EncodeFixedFormatByte(data);
    }

    /// <summary>
    /// Encode the Text command's first fixed byte (rotation, path, spacing).
    /// Bits 1-2: rotation (0-3).
    /// Bits 3-4: path (0-3).
    /// Bits 5-6: spacing (0-3).
    /// </summary>
    public static byte EncodeTextFixedByte1(byte rotation, byte path, byte spacing)
    {
        byte data = (byte)(rotation & 0x03);
        data |= (byte)((path & 0x03) << 2);
        data |= (byte)((spacing & 0x03) << 4);

        return EncodeFixedFormatByte(data);
    }

    /// <summary>
    /// Encode the Text command's second fixed byte (interrow, move attrs, cursor style).
    /// Bits 1-2: interrow spacing (0-3).
    /// Bits 3-4: move attributes (0-3).
    /// Bits 5-6: cursor style (0-3).
    /// </summary>
    public static byte EncodeTextFixedByte2(byte interrowSpacing, byte moveAttributes, byte cursorStyle)
    {
        byte data = (byte)(interrowSpacing & 0x03);
        data |= (byte)((moveAttributes & 0x03) << 2);
        data |= (byte)((cursorStyle & 0x03) << 4);

        return EncodeFixedFormatByte(data);
    }

    /// <summary>
    /// Encode the Reset command's first byte.
    /// Bit 1:    domain reset.
    /// Bits 2-3: color mode reset (0-3).
    /// Bits 4-6: screen/border color reset (0-7).
    /// </summary>
    public static byte EncodeResetByte1(bool domainReset, byte colorMode, byte screenBorder)
    {
        byte data = (byte)(domainReset ? 1 : 0);
        data |= (byte)((colorMode & 0x03) << 1);
        data |= (byte)((screenBorder & 0x07) << 3);

        return EncodeFixedFormatByte(data);
    }

    /// <summary>
    /// Encode the Reset command's second byte.
    /// Bit 1: text/cursor reset.
    /// Bit 2: blink reset.
    /// Bit 3: protect unprotected fields.
    /// Bit 4: texture attributes reset.
    /// Bit 5: macros reset.
    /// Bit 6: DRCS chars reset.
    /// </summary>
    public static byte EncodeResetByte2(bool textReset, bool blinkReset, bool protectFields, bool textureReset, bool macrosReset, bool drcsReset)
    {
        byte data = 0;
        if (textReset) { data |= 1 << 0; }
        if (blinkReset) { data |= 1 << 1; }
        if (protectFields) { data |= 1 << 2; }
        if (textureReset) { data |= 1 << 3; }
        if (macrosReset) { data |= 1 << 4; }
        if (drcsReset) { data |= 1 << 5; }

        return EncodeFixedFormatByte(data);
    }
}
