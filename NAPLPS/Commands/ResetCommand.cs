// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The RESET command is used to selectively reinitialize the control
/// and attribute parameters to their default values, clear the screen, set the
/// border color, home the cursor, and clear the DRCS set, texture attributes,
/// macros, and unprotected fields.
/// </summary>
[AddCommand(200, "Reset", "Selectively reinitialize control/attribute parameters to defaults.", Category = CommandCategory.System, DslKeyword = "reset")]
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
        // Do NOT mutate Operands: keep it as what was actually read so ToBytes round-trips.
        // Bit decoding below uses safe fallbacks (bit = false) when an operand byte is absent.

        if (operands.Count == 0)
        {
            State.RecordError(NaplpsErrorSeverity.Warning, NaplpsErrorType.InvalidCommand, "Reset command received with no operands, treating all bits as 0", opcode);
        }
        else if (operands.Count == 1)
        {
            State.RecordError(NaplpsErrorSeverity.Warning, NaplpsErrorType.InvalidCommand, "Reset command received with only 1 operand, treating byte 2 as 0", opcode);
        }

        // Local bit-extraction helper: operands[i,b] throws when i is out of range; we
        // default to false (bit 0) for missing operand bytes, matching the spec's "as if
        // b6-b1 equal 0" rule without writing phantom bytes into Operands.
        bool Bit(int byteIdx, int bitPos) => operands.Count > byteIdx && operands[byteIdx, bitPos];

        IsDomainReset = Bit(0, 1);
        ColorMode = (ColorModeReset)ConvertBitsToByte([Bit(0, 2), Bit(0, 3)]);
        ColorScreenBorder = (ScreenBorderReset)ConvertBitsToByte([Bit(0, 4), Bit(0, 5), Bit(0, 6)]);
        IsTextReset = Bit(1, 1);
        IsBlinkReset = Bit(1, 2);
        IsProtectedFields = Bit(1, 3);
        IsTextureAttributesReset = Bit(1, 4);
        IsMacrosReset = Bit(1, 5);
        IsDRCSCharsReset = Bit(1, 6);
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