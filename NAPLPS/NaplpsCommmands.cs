// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public enum NaplpsCommands : byte
{
    // Control Zero Set
    NULL = 0x0, // This has no effect, can be used on the protocol layer

    START_OF_HEADER = 0x01, // SOH
    START_OF_TEXT = 0x02, // STX
    END_OF_TEXT = 0x03, // ETX
    END_OF_TRANSMISSION = 0x04, // EOT
    ENQUIRY = 0x05, // ENQ
    ACKNOWLEDGE = 0x06, // ACK
    BELL = 0x07, // BEL

    AP_BACKWARD = 0x08, // Active Position Backward
    AP_FORWARD = 0x09, // Active Position Forward
    AP_DOWN = 0x0A, // Active Position Down
    AP_UP = 0x0B, // Active Position Up

    CLEAR_SCREEN = 0x0C, // CS
    AP_RETURN = 0x0D, // Active Position Return

    SHIFT_OUT = 0x0E, // Shift Out - Switch to Graphics Mode
    SHIFT_IN = 0x0F, // Shift In - Switch to Text Mode
    
    DATA_LINK_ESCAPE = 0x10, // DLE
    
    DATA_CONTROL_1 = 0x11, // Data Control Characters
    DATA_CONTROL_2 = 0x12, // Data Control Characters
    DATA_CONTROL_3 = 0x13, // Data Control Characters
    DATA_CONTROL_4 = 0x14, // Data Control Characters

    NEGATIVE_ACKNOWLEDGE = 0x15, // NAK
    SYNC_IDLE = 0x16, // SYN
    END_OF_BLOCK = 0x17, // EOB

    CANCEL = 0x18, // Cancel
    SHIFT_TWO = 0x19, // Single-Shift Two
    SERVICE_DELIM_CHAR = 0x1A, // Service Delimiter Character
    ESC = 0x1B, // Escape
    AP_SET = 0x1C, // Active Position Set
    SHIFT_THREE = 0x1D, // Single-Shift Three
    AP_HOME = 0x1E, // Active Position Home
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