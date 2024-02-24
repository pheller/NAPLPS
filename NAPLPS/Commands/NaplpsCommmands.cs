// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System;

namespace NAPLPS.Commands;

public static class NaplpsExtensions
{
    public static bool GetBit(this byte b, int bitNumber)
    {
        if (bitNumber < 1 || bitNumber > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(bitNumber), "Bit number must be between 1 and 8.");
        }

        return (b & 1 << bitNumber - 1) != 0;
    }
}

public enum NaplpsCommands : byte
{
    // Control
    SHIFT_OUT = 0x0E, // Shift Out - Switch to Graphics Mode
    SHIFT_IN = 0x0F, // Shift In - Switch to Text Mode
    CANCEL = 0x18, // Cancel
    ESC = 0x1B, // Escape
    NSR = 0x1F, // Non-Selective Reset

    // Points
    POINT_SET_ABS = 0x24, // Point Set Absolute, Invisible
    POINT_SET_REL = 0x25, // Point Set Relative, Invisible
    POINT_ABS = 0x26, // Point Absolute
    POINT_REL = 0x27, // Point Relative

    // Lines
    LINE_ABS = 0x28, // Line Absolute
    LINE_REL = 0x29, // Line Relative
    LINE_SET_ABS = 0x2A, // Line Set Absolute
    LINE_SET_REL = 0x2B, // Line Set Relative

    // Arcs
    ARC_OUTLINED = 0x2C, // Arc Outlined
    ARC_FILLED = 0x2D, // Arc Filled
    ARC_SET_OUTLINED = 0x2E, // Arc Set Outlined
    ARC_SET_FILLED = 0x2F, // Arc Set Filled

    // Rectangles
    RECTANGLE_OUTLINED = 0x30, // Rectangle Outlined
    RECTANGLE_FILLED = 0x31, // Rectangle Filled
    RECTANGLE_SET_OUTLINED = 0x32, // Rectangle Set Outlined
    RECTANGLE_SET_FILLED = 0x33, // Rectangle Set Filled

    // Polygons
    POLYGON_OUTLINED = 0x34, // Polygon Outlined
    POLYGON_FILLED = 0x35, // Polygon Filled
    POLYGON_SET_OUTLINED = 0x36, // Polygon Set Outlined
    POLYGON_SET_FILLED = 0x37, // Polygon Set Filled

    // Incrementals
    INCREMENTAL_FIELD = 0x38, // Incremental Field
    INCREMENTAL_POINT = 0x39, // Incremental Point
    INCREMENTAL_LINE = 0x3A, // Incremental Line
    INCREMENTAL_POLYGON_FILLED = 0x3B, // Incremental Polygon Filled

    // Environment
    RESET = 0x20, // Reset
    DOMAIN = 0x21, // Domain
    TEXT = 0x22, // Text - Formats text, doesn't contain it
    TEXTURE = 0x23, // Texture
    SET_COLOR = 0x3C, // Set Color
    WAIT = 0x3D, // Wait
    SELECT_COLOR = 0x3E, // Select Color
    BLINK = 0x3F, // Blink
}