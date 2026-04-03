// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Drawing;

namespace NAPLPS.Commands;

public class AsciiCharCommand : NaplpsCommand
{
    /// <summary>
    /// NAPLPS supplementary set non-spacing accent characters (positions 0x40-0x4F).
    /// These are drawn without advancing the cursor, allowing composite characters
    /// (accent + base letter at the same position).
    /// </summary>
    private static readonly HashSet<char> NonSpacingAccents = new()
    {
        '\u2192', // vector overbar (mapped to → in supplementary set)
        '\u0060', // grave accent `
        '\u00B4', // acute accent ´
        '\u02C6', // circumflex ˆ
        '\u007E', // tilde ~ (non-spacing when from supplementary set)
        '\u00AF', // macron ¯
        '\u02D8', // breve ˘
        '\u02D9', // dot above ˙
        '\u00A8', // diaeresis ¨
        // '\u002F' — slash / is also in this range but conflicts with ASCII
        '\u02DA', // ring above ˚
        '\u00B8', // cedilla ¸
        // '\u005F' — underscore _ conflicts with ASCII
        '\u02DD', // double acute ˝
        '\u02DB', // ogonek ˛
        '\u02C7', // caron ˇ
    };

    /// <summary>
    /// ANSI X3.110: Special characters that allow mid-word breaking when embedded
    /// within a word (not at beginning or end) during word wrap.
    /// </summary>
    private static readonly HashSet<char> WordBreakChars = new()
    {
        '!', '"', '$', '%', '(', ')', '[', ']', '<', '>', '{', '}',
        '^', '*', '+', '-', '/', ',', '.', ':', ';', '=', '?', '_', '~'
    };

    public char AsciiCharacter { get; }

    /// <summary>
    /// True if this character is a non-spacing accent from the supplementary set.
    /// Non-spacing accents don't advance the cursor.
    /// </summary>
    public bool IsNonSpacing { get; }

    /// <summary>
    /// True if this character was discarded (space at end of line during word wrap).
    /// </summary>
    public bool IsDiscarded { get; }

    public AsciiCharCommand(char asciiCharacter, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        AsciiCharacter = asciiCharacter;

        // Non-spacing accents from the supplementary set don't advance the cursor.
        // Only treat as non-spacing if the character is NOT in the basic ASCII range
        // (avoids false positives for ~ and _ which exist in both sets).
        IsNonSpacing = asciiCharacter > 0x7E && NonSpacingAccents.Contains(asciiCharacter);

        state.AutoWrapJustOccurred = false;

        if (!IsNonSpacing)
        {
            MovePen(state);
            state.SyncAfterTextMove();

            // ANSI X3.110: "if the subsequent cursor movement would cause part of the
            // character field to be outside the unit screen or outside the active field,
            // then an automatic <carriage return> <linefeed> is executed."
            // Check AFTER advancing — if the new pen position is outside the field, wrap.
            if (CheckFieldBoundary(state))
            {
                // In word wrap mode, discard trailing spaces instead of wrapping
                if (state.IsWordWrapMode && asciiCharacter == ' ')
                {
                    // Undo the pen advance — space is discarded
                    IsDiscarded = true;
                }

                PerformAutoWrap(state);
                state.AutoWrapJustOccurred = true;
            }

            // Track last break position for word wrap
            if (state.IsWordWrapMode && !IsDiscarded)
            {
                if (asciiCharacter == ' ' || WordBreakChars.Contains(asciiCharacter))
                {
                    state.LastWordBreakPen = state.Pen;
                }
            }
        }
    }

    /// <summary>
    /// Checks if the pen position (after character advance) is outside the field boundary.
    /// Returns true if it exceeds the boundary and a wrap is needed.
    /// Does NOT perform the wrap — caller decides (may discard space in word wrap mode).
    /// </summary>
    private static bool CheckFieldBoundary(NaplpsState state)
    {
        // Don't check if field hasn't been explicitly set (default struct has zero dimensions)
        if (state.Field.Dimensions.X == 0 && state.Field.Dimensions.Y == 0)
        {
            return false;
        }

        var pen = state.Pen;
        float x1 = state.Field.Origin.X;
        float x2 = state.Field.Origin.X + state.Field.Dimensions.X;
        float y1 = state.Field.Origin.Y;
        float y2 = state.Field.Origin.Y + state.Field.Dimensions.Y;
        float fieldRight = Math.Max(x1, x2);
        float fieldLeft = Math.Min(x1, x2);
        float fieldBottom = Math.Min(y1, y2);
        float fieldTop = Math.Max(y1, y2);

        // If the effective field size is zero or negative in the text direction, skip the check.
        // This handles cases where field dimensions are negative (extending off-screen).
        float fieldWidth = fieldRight - fieldLeft;
        float fieldHeight = fieldTop - fieldBottom;

        switch (state.TextPath)
        {
            case TextPath.Right:
            {
                return fieldWidth > 0 && pen.X > fieldRight;
            }

            case TextPath.Left:
            {
                return fieldWidth > 0 && pen.X < fieldLeft;
            }

            case TextPath.Down:
            {
                return fieldHeight > 0 && pen.Y < fieldBottom;
            }

            case TextPath.Up:
            {
                return fieldHeight > 0 && pen.Y > fieldTop;
            }
        }

        return false;
    }

    /// <summary>
    /// Performs automatic carriage return + line feed.
    /// Moves pen to start of character path and advances perpendicular to it.
    /// </summary>
    private static void PerformAutoWrap(NaplpsState state)
    {
        var pen = state.Pen;

        float interrowMultiplier = state.TextInterrowSpacing switch
        {
            TextInterrowSpacing.One => 1.0f,
            TextInterrowSpacing.FiveQuarters => 1.25f,
            TextInterrowSpacing.ThreeHalves => 1.5f,
            TextInterrowSpacing.Two => 2.0f,
            _ => 1.0f
        };

        switch (state.TextPath)
        {
            case TextPath.Right:
            {
                pen.X = state.Field.Origin.X;
                pen.Y -= state.CharSize.Y * interrowMultiplier;
            }
            break;

            case TextPath.Left:
            {
                pen.X = state.Field.Origin.X + state.Field.Dimensions.X;
                pen.Y -= state.CharSize.Y * interrowMultiplier;
            }
            break;

            case TextPath.Down:
            {
                pen.Y = state.Field.Origin.Y + state.Field.Dimensions.Y;
                pen.X += state.CharSize.X * interrowMultiplier;
            }
            break;

            case TextPath.Up:
            {
                pen.Y = state.Field.Origin.Y;
                pen.X += state.CharSize.X * interrowMultiplier;
            }
            break;
        }

        state.Pen = pen;
    }

    private void MovePen(NaplpsState state)
    {
        var pen = state.Pen;

        {
            // PP3 confirmed: Spacing=One advances by full charW (pixel-counted at 640px).
            // Spacing=Proportional uses charW * row8[class]/8 ratio.
            float advance;

            if (state.TextSpacing == TextSpacing.Proportional)
            {
                float widthRatio = DrawableAsciiChar.GetCharWidthRatio(AsciiCharacter);
                advance = state.CharSize.X * widthRatio;
            }
            else
            {
                float spacingMultiplier = state.TextSpacing switch
                {
                    TextSpacing.FiveQuarters => 1.25f,
                    TextSpacing.ThreeHalves => 1.5f,
                    _ => 1.0f
                };

                advance = state.CharSize.X * spacingMultiplier;
            }

            switch (state.TextPath)
            {
                case TextPath.Right:
                {
                    pen.X += advance;
                }
                break;

                case TextPath.Left:
                {
                    pen.X -= advance;
                }
                break;

                case TextPath.Up:
                {
                    pen.Y += state.CharSize.Y;
                }
                break;

                case TextPath.Down:
                {
                    pen.Y -= state.CharSize.Y;
                }
                break;
            }
        }

        state.Pen = pen;
    }

    /// <summary>
    /// Returns true if this character is a valid word break point for word wrap.
    /// </summary>
    public static bool IsWordBreakChar(char c) => c == ' ' || WordBreakChars.Contains(c);

    public override string ToString()
    {
        return $"ASCII({AsciiCharacter})";
    }
}
