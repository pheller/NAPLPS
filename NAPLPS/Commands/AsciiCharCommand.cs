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

    public char AsciiCharacter { get; }

    /// <summary>
    /// True if this character is a non-spacing accent from the supplementary set.
    /// Non-spacing accents don't advance the cursor.
    /// </summary>
    public bool IsNonSpacing { get; }

    public AsciiCharCommand(char asciiCharacter, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        AsciiCharacter = asciiCharacter;

        // Non-spacing accents from the supplementary set don't advance the cursor.
        // Only treat as non-spacing if the character is NOT in the basic ASCII range
        // (avoids false positives for ~ and _ which exist in both sets).
        IsNonSpacing = asciiCharacter > 0x7E && NonSpacingAccents.Contains(asciiCharacter);

        if (!IsNonSpacing)
        {
            MovePen(state);
        }
    }

    private void MovePen(NaplpsState state)
    {
        var pen = state.Pen;

        if (state.TextSpacing == TextSpacing.Proportional)
        {
            // ANSI X3.110 full proportional spacing algorithm
            // Displacement is calculated from character field width and width class
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

    public override string ToString()
    {
        return $"ASCII({AsciiCharacter})";
    }
}
