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

        if (!IsNonSpacing)
        {
            // Check if auto-wrap is needed before advancing the pen
            if (CheckFieldBoundary(state))
            {
                // Space that doesn't fit in word wrap mode → discard
                if (state.IsWordWrapMode && asciiCharacter == ' ')
                {
                    IsDiscarded = true;
                    return;
                }
            }

            MovePen(state);

            // Track last break position for word wrap
            if (state.IsWordWrapMode)
            {
                if (asciiCharacter == ' ' || WordBreakChars.Contains(asciiCharacter))
                {
                    state.LastWordBreakPen = state.Pen;
                }
            }
        }
    }

    /// <summary>
    /// Checks if the next character would exceed the field boundary.
    /// If so, performs auto CR+LF. Returns true if wrap occurred.
    /// </summary>
    private static bool CheckFieldBoundary(NaplpsState state)
    {
        var pen = state.Pen;
        float fieldRight = state.Field.Origin.X + state.Field.Dimensions.X;
        float fieldLeft = state.Field.Origin.X;

        bool wouldExceed = false;

        switch (state.TextPath)
        {
            case TextPath.Right:
            {
                wouldExceed = pen.X + state.CharSize.X > fieldRight;
            }
            break;

            case TextPath.Left:
            {
                wouldExceed = pen.X - state.CharSize.X < fieldLeft;
            }
            break;

            case TextPath.Down:
            {
                wouldExceed = pen.Y - state.CharSize.Y < state.Field.Origin.Y;
            }
            break;

            case TextPath.Up:
            {
                float fieldTop = state.Field.Origin.Y + state.Field.Dimensions.Y;
                wouldExceed = pen.Y + state.CharSize.Y > fieldTop;
            }
            break;
        }

        if (wouldExceed)
        {
            PerformAutoWrap(state);
            return true;
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

        if (state.TextSpacing == TextSpacing.Proportional)
        {
            // ANSI X3.110 full proportional spacing algorithm
            float charFieldDim = (state.TextPath == TextPath.Right || state.TextPath == TextPath.Left)
                ? state.CharSize.X : state.CharSize.Y;
            float displacement = DrawableAsciiChar.GetProportionalDisplacement(MathF.Abs(charFieldDim), AsciiCharacter);

            switch (state.TextPath)
            {
                case TextPath.Right:
                {
                    pen.X += displacement;
                }
                break;

                case TextPath.Left:
                {
                    pen.X -= displacement;
                }
                break;

                case TextPath.Up:
                {
                    pen.Y += displacement;
                }
                break;

                case TextPath.Down:
                {
                    pen.Y -= displacement;
                }
                break;
            }
        }
        else
        {
            float spacingMultiplier = state.TextSpacing switch
            {
                TextSpacing.One => 1.0f,
                TextSpacing.FiveQuarters => 1.25f,
                TextSpacing.ThreeHalves => 1.5f,
                _ => 1.0f
            };

            switch (state.TextPath)
            {
                case TextPath.Right:
                {
                    pen.X += state.CharSize.X * spacingMultiplier;
                }
                break;

                case TextPath.Left:
                {
                    pen.X -= state.CharSize.X * spacingMultiplier;
                }
                break;

                case TextPath.Up:
                {
                    pen.Y += state.CharSize.Y * spacingMultiplier;
                }
                break;

                case TextPath.Down:
                {
                    pen.Y -= state.CharSize.Y * spacingMultiplier;
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
