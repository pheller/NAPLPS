// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The RESET command is used to selectively reinitialize the control
/// and attribute parameters to their default values, clear the screen, set the
/// border color, home the cursor, and clear the DRCS set, texture attributes,
/// macros, and unprotected fields.
/// </summary>
public class ResetCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.Fixed;

    public static new readonly int OperandCount = 2;

    /// <summary>
    /// If bit bl of byte 1 equals 1, the DOMAIN parameters are reset to their
    /// default values. If b1 is 0, the DOMAIN parameters are not changed.
    /// </summary>
    public bool IsDomainReset { get; }

    public bool IsTextReset { get; }

    public bool IsBlinkReset { get; }

    public bool IsProtectedFields { get; }

    public bool IsTextureAttributesReset { get; }

    public bool IsMacrosReset { get; }

    public bool IsDRCSCharsReset { get; }

    public ColorModeReset ColorMode { get; }

    public ScreenBorderReset ColorScreenBorder { get; }

    public ResetCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        /*	If the RESET command is received with no operands, it is interpreted as if it
		/*	had been sent with bits b6 to b1 in both bytes set equal to 0. If only one byte
		/*	is received, the second operand is then interpreted as if it had been received
		/*	with bits b6 to b1 set equal to 0. If more than two data bytes are received,
		/*	the additional byte(s) are reserved for future standardization and shall be
		/*	ignored. */
        if (operands.Count == 0)
        {
            // Debugger.Break();
            Operands.AddRange([0, 0]);
        }
        else if (operands.Count == 1)
        {
            // Debugger.Break();
            Operands.Add(0);
        }

        // If bit bl of byte 1 equals 1, the
        // DOMAIN parameters are reset to their default values. If b1 is 0, the DOMAIN
        // parameters are not changed.
        IsDomainReset = Operands[0, 1];

        // Bits b3 and b2 of byte 1 modify the color mode and/or current drawing color
        ColorMode = (ColorModeReset)ConvertBitsToByte([Operands[0, 2], Operands[0, 3]]);

        // Bits b6, b5, and b4 of byte 1 clear the display area and/or border area to the colors
        ColorScreenBorder = (ScreenBorderReset)ConvertBitsToByte([Operands[0, 4], Operands[0, 5], Operands[0, 6]]);

        // If bit b1 of byte 2 equals 1, the cursor is sent to its home position
        // (top left character position in the display area) and all text parameters
        // (from the TEXT opcode, from the C1 set and the active field) are reset
        // to their default values. If b1 is 0, the text parameters and the
        // cursor position are not changed.
        IsTextReset = Operands[1, 1];

        // If bit b2 of byte 2 equals 1, all blink processes are terminated. If b2 is 0, then
        // blink processes are not changed.
        IsBlinkReset = Operands[1, 2];

        // If bit b3 of byte 2 equals 1, all unprotected fields are changed to protected
        // status but the displayed contents are unaffected. However, the field
        // definitions (except that of the active field) are lost, as well as any data
        // structures maintained for user editing and transmission. If b3 is 0, unprotected
        // fields are not changed.
        IsProtectedFields = Operands[1, 3];

        // If bit b4 of byte 2 equals 1, all texture attributes are set to their default
        // values. The four programmable texture masks are not cleared. If b4 is 0,
        // current texture attributes are not changed.
        IsTextureAttributesReset = Operands[1, 4];

        // If bit b5 of byte 2 equals 1, all macros are cleared. This includes transmit macros.
        // If b5 is 0, macros are not changed.
        IsMacrosReset = Operands[1, 5];

        // If bit b6 of byte 2 equals 1, all ORCS characters are cleared, that is, all
        // character positions are set to the space character. If b6 is 0, the ORCS
        // characters are not changed.
        IsDRCSCharsReset = Operands[1, 6];
    }

    public enum ColorModeReset : byte
    {
        NoAction,
        /// <summary>
        /// Select color mode 0, set color map to default colors, and
        /// set the in-use drawing color to white.
        /// </summary>
        SelectZero,
        /// <summary>
        /// Select color mode 1 and set color map to default colors.
        /// If this is executed while in color mode 0, then it has the
        /// same effect as "SelectOneAndDefaultsDrawWhite".
        /// </summary>
        SelectOneAndDefaults,
        /// <summary>
        /// Select color mode 1, set color map to default colors, and
        /// set the in-use drawing color to white.
        /// </summary>
        SelectOneAndDefaultsDrawWhite
    }

    /// <summary>The border area surrounds the display area and may only be set to one color at a time.</summary>
    public enum ScreenBorderReset : byte
    {
        NoAction,
        /// <summary>Display area to nominal black.</summary>
        ScreenBlack,

        /// <summary>Display area to current drawing color.</summary>
        ScreenDrawing,

        /// <summary>Border area to nominal black.</summary>
        BorderBlack,

        /// <summary>Border area to current drawing color.</summary>
        BorderDrawing,

        /// <summary>Display area and border area to current drawing color.</summary>
        ScreenBorderDrawing,

        /// <summary>Display area to current drawing color and border area to nominal black.</summary>
        ScreenDrawingBorderBlack,

        /// <summary>Display area and border area to nominal black.</summary>
        ScreenBorderBlack
    }
}