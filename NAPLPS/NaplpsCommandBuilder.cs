// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

/// <summary>
/// High-level factory that combines NAPLPS opcodes with encoded operands
/// to produce command byte sequences suitable for AddCommand().
/// Opcodes are derived from the GeneralPDISet at offset 0xA0.
/// </summary>
public static class NaplpsCommandBuilder
{
    // PDI opcodes (GeneralPDISet base = 0xA0)
    public const byte OpPointSetAbsolute = 0xA4;
    public const byte OpPointSetRelative = 0xA5;
    public const byte OpPointAbsolute = 0xA6;
    public const byte OpPointRelative = 0xA7;
    public const byte OpLineAbsolute = 0xA8;
    public const byte OpLineRelative = 0xA9;
    public const byte OpLineSetAbsolute = 0xAA;
    public const byte OpLineSetRelative = 0xAB;
    public const byte OpRectangleOutlined = 0xB0;
    public const byte OpRectangleFilled = 0xB1;
    public const byte OpArcOutlined = 0xAC;
    public const byte OpArcFilled = 0xAD;
    public const byte OpArcSetOutlined = 0xAE;
    public const byte OpArcSetFilled = 0xAF;
    public const byte OpPolygonOutlined = 0xB4;
    public const byte OpPolygonFilled = 0xB5;
    public const byte OpSelectColor = 0xBE;

    /// <summary>Move pen to absolute position (no draw). Creates PointSetAbsoluteCommand.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildPointSetAbsolute(float x, float y, int multiByteValue = 3)
    {
        return (OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(x, y, multiByteValue));
    }

    /// <summary>Draw line from current pen to absolute position. Creates LineAbsoluteCommand.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildLineAbsolute(float x, float y, int multiByteValue = 3)
    {
        return (OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(x, y, multiByteValue));
    }

    /// <summary>Draw connected lines through absolute positions. Creates LineSetAbsoluteCommand.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildLineSetAbsolute(Vector3[] points, int multiByteValue = 3)
    {
        return (OpLineSetAbsolute, NaplpsEncoder.EncodeVertices2D(points, multiByteValue));
    }

    /// <summary>Draw filled rectangle with given dimensions relative to pen. Creates RectangleFilledCommand.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildRectangleFilled(float width, float height, int multiByteValue = 3)
    {
        return (OpRectangleFilled, NaplpsEncoder.EncodeVertex2D(width, height, multiByteValue));
    }

    /// <summary>Draw outlined rectangle with given dimensions relative to pen. Creates RectangleOutlinedCommand.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildRectangleOutlined(float width, float height, int multiByteValue = 3)
    {
        return (OpRectangleOutlined, NaplpsEncoder.EncodeVertex2D(width, height, multiByteValue));
    }

    /// <summary>Draw filled polygon with relative vertices from pen. Creates PolygonFilledCommand.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildPolygonFilled(Vector3[] relativeVertices, int multiByteValue = 3)
    {
        return (OpPolygonFilled, NaplpsEncoder.EncodeVertices2D(relativeVertices, multiByteValue));
    }

    /// <summary>Draw outlined polygon with relative vertices from pen. Creates PolygonOutlinedCommand.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildPolygonOutlined(Vector3[] relativeVertices, int multiByteValue = 3)
    {
        return (OpPolygonOutlined, NaplpsEncoder.EncodeVertices2D(relativeVertices, multiByteValue));
    }

    /// <summary>Draw outlined arc. Operands are mid-point and end-point relative to pen.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildArcOutlined(float midRelX, float midRelY, float endRelX, float endRelY, int multiByteValue = 3)
    {
        var operands = NaplpsEncoder.EncodeVertex2D(midRelX, midRelY, multiByteValue);
        var endOperands = NaplpsEncoder.EncodeVertex2D(endRelX, endRelY, multiByteValue);
        operands.AddRange(endOperands);
        return (OpArcOutlined, operands);
    }

    /// <summary>Draw filled arc. Operands are mid-point and end-point relative to pen.</summary>
    public static (byte opcode, NaplpsOperands operands) BuildArcFilled(float midRelX, float midRelY, float endRelX, float endRelY, int multiByteValue = 3)
    {
        var operands = NaplpsEncoder.EncodeVertex2D(midRelX, midRelY, multiByteValue);
        var endOperands = NaplpsEncoder.EncodeVertex2D(endRelX, endRelY, multiByteValue);
        operands.AddRange(endOperands);
        return (OpArcFilled, operands);
    }

    /// <summary>Set foreground color by palette index (color mode 1).</summary>
    public static (byte opcode, NaplpsOperands operands) BuildSelectColor(byte fgIndex)
    {
        return (OpSelectColor, NaplpsEncoder.EncodeSelectColorForeground(fgIndex));
    }

    /// <summary>Set foreground and background colors by palette index (color mode 2).</summary>
    public static (byte opcode, NaplpsOperands operands) BuildSelectColor(byte fgIndex, byte bgIndex)
    {
        return (OpSelectColor, NaplpsEncoder.EncodeSelectColorForegroundBackground(fgIndex, bgIndex));
    }

    public const byte OpText = 0xA2;

    /// <summary>
    /// Build a TEXT command that sets character size and spacing mode.
    /// Fixed byte 1: bits 6,5=spacing, bits 4,3=path, bits 2,1=rotation
    /// Fixed byte 2: bits 6,5=cursor, bits 4,3=moveAttrs, bits 2,1=interrow
    /// Then multi-value vertex for charSize (dx, dy).
    /// </summary>
    public static (byte opcode, NaplpsOperands operands) BuildText(
        float charWidth, float charHeight,
        TextCommand.TextSpacing spacing = TextCommand.TextSpacing.One,
        TextCommand.TextPath path = TextCommand.TextPath.Right,
        int multiByteValue = 3)
    {
        var operands = new NaplpsOperands();

        // Fixed byte 1: rotation(bits 2,1) + path(bits 4,3) + spacing(bits 6,5)
        byte fixed1 = 0xC0; // base for numerical data
        int spacingBits = (int)spacing;
        int pathBits = (int)path;
        // bit 6 = spacing high, bit 5 = spacing low
        if ((spacingBits & 2) != 0) fixed1 |= (1 << 5); // bit 6 (1-indexed)
        if ((spacingBits & 1) != 0) fixed1 |= (1 << 4); // bit 5
        // bit 4 = path high, bit 3 = path low
        if ((pathBits & 2) != 0) fixed1 |= (1 << 3); // bit 4
        if ((pathBits & 1) != 0) fixed1 |= (1 << 2); // bit 3
        operands.Add(fixed1);

        // Fixed byte 2: interrow(bits 2,1) + moveAttrs(bits 4,3) + cursor(bits 6,5)
        // All defaults = 0
        operands.Add(0xC0);

        // Multi-value vertex for char size
        var sizeOperands = NaplpsEncoder.EncodeVertex2D(charWidth, charHeight, multiByteValue);
        operands.AddRange(sizeOperands);

        return (OpText, operands);
    }
}
