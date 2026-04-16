// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// This command is used to modify parameters
/// that describe the manner in which subsequent alphanumeric characters, mosaic
/// characters, and ORCS are presented. The TEXT opcode takes a two byte,
/// fixed format operand, followed by a multi-value operand
/// </summary>
[AddCommand(260, "Text", "Set text rotation, path, spacing, interrow, move, cursor, and character size.", Category = CommandCategory.Text, DslKeyword = "text")]
public class TextCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.FixedAndMultiValue;

    public TextCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (Operands.Count == 0)
        {
            IsValid = false;

            return;
        }

        State.TextRotation = ParseRotation(Operands[0, 1], Operands[0, 2]);
        State.TextPath = ParsePath(Operands[0, 3], Operands[0, 4]);
        State.TextSpacing = ParseSpacing(Operands[0, 5], Operands[0, 6]);

        if (Operands.Count >= 2)
        {
            State.TextInterrowSpacing = ParseInterrowSpacing(Operands[1, 1], Operands[1, 2]);
            State.TextMoveAttributes = ParseMoveAttributes(Operands[1, 3], Operands[1, 4]);
            State.TextCursorStyle = ParseCursorStyle(Operands[1, 5], Operands[1, 6]);
        }

        // If the character field dimensions are omitted from the operand, then the
        // current character field dimensions remain unchanged.
        if (Operands.Count == 2 + State.MultiByteValue)
        {
            Vertices = ProcessVertices(Operands[2..]);

            State.CharSize = new Vector2(Vertices[0].X, Vertices[0].Y);
        }
    }

    static TextRotation ParseRotation(bool bit1, bool bit2)
    {
        if (bit2 && bit1) { return TextRotation.TwoSeventy; }
        else if (bit2 && !bit1) { return TextRotation.OneEighty; }
        else if (!bit2 && bit1) { return TextRotation.Ninety; }
        else { return TextRotation.Zero; } // Default
    }

    static TextPath ParsePath(bool bit3, bool bit4)
    {
        if (bit4 && bit3) { return TextPath.Down; }
        else if (bit4 && !bit3) { return TextPath.Up; }
        else if (!bit4 && bit3) { return TextPath.Left; }
        else { return TextPath.Right; } // Default
    }

    static TextSpacing ParseSpacing(bool bit5, bool bit6)
    {
        if (bit6 && bit5) { return TextSpacing.Proportional; }
        else if (bit6 && !bit5) { return TextSpacing.ThreeHalves; }
        else if (!bit6 && bit5) { return TextSpacing.FiveQuarters; }
        else { return TextSpacing.One; } // Default
    }

    static TextInterrowSpacing ParseInterrowSpacing(bool bit1, bool bit2)
    {
        if (bit2 && bit1) { return TextInterrowSpacing.Two; }
        else if (bit2 && !bit1) { return TextInterrowSpacing.ThreeHalves; }
        else if (!bit2 && bit1) { return TextInterrowSpacing.FiveQuarters; }
        else { return TextInterrowSpacing.One; } // Default
    }

    static TextMoveAttributes ParseMoveAttributes(bool bit3, bool bit4)
    {
        if (bit4 && bit3) { return TextMoveAttributes.MoveIndependently; }
        else if (bit4 && !bit3) { return TextMoveAttributes.DrawingPointLeads; }
        else if (!bit4 && bit3) { return TextMoveAttributes.CursorLeads; }
        else { return TextMoveAttributes.MoveTogether; } // Default
    }

    static TextCursorStyle ParseCursorStyle(bool bit5, bool bit6)
    {
        if (bit6 && bit5) { return TextCursorStyle.Custom; }
        else if (bit6 && !bit5) { return TextCursorStyle.Crosshair; }
        else if (!bit6 && bit5) { return TextCursorStyle.Block; }
        else { return TextCursorStyle.Underscore; } // Default
    }

    public enum TextRotation : byte
    {
        Zero = 0,
        Ninety = 1,
        OneEighty = 2,
        TwoSeventy = 3
    }

    public enum TextPath : byte
    {
        Right = 0,
        Left = 1,
        Up = 2,
        Down = 3
    }

    public enum TextSpacing : byte
    {
        One = 0,
        FiveQuarters = 1,
        ThreeHalves = 2,
        Proportional = 3
    }

    public enum TextInterrowSpacing : byte
    {
        One = 0,
        FiveQuarters = 1,
        ThreeHalves = 2,
        Two = 3
    }

    public enum TextMoveAttributes : byte
    {
        MoveTogether = 0,
        CursorLeads = 1,
        DrawingPointLeads = 2,
        MoveIndependently = 3
    }

    public enum TextCursorStyle : byte
    {
        Underscore = 0,
        Block = 1,
        Crosshair = 2,
        Custom = 3
    }
}
