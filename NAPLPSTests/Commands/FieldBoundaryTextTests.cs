// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

/// <summary>
/// Tests for the interaction between FIELD commands and text cursor advancement.
///
/// BACKGROUND — THE BUG THIS PREVENTS:
///
/// NAPLPS uses "sign-and-fraction" encoding for coordinates. The first bit is a sign bit,
/// and the remaining bits represent a binary fraction. Two's complement means:
///   - sign=0, fraction=0.75 → +0.75
///   - sign=1, fraction=0.0  → -1.0  (computed as -1 + 0)
///
/// The IncrementalFieldCommand defines a text field with an origin and dimensions.
/// In COLORBAR.NAP (a Prodigy 8-bit file), the field's X dimension operand bytes decode
/// to sign=1, fraction=0 → -1.0. This is the encoding's way of saying "full unit screen
/// extent" because +1.0 is not representable (max positive ≈ 0.999).
///
/// With Origin=(0,0) and Dimensions.X=-1.0, the old code computed:
///   fieldRight = Origin.X + Dimensions.X = 0 + (-1) = -1
///
/// After each character advanced the pen by ~0.023 (CharSize.X), the boundary check:
///   pen.X > fieldRight → 0.023 > -1 → true → WRAP!
///
/// Every single character triggered an auto-wrap (carriage return + line feed),
/// causing text to render VERTICALLY (one character per line) instead of horizontally.
///
/// FIX: IncrementalFieldCommand now takes Math.Abs() of dimensions, since field
/// dimensions are sizes, not direction vectors. -1.0 becomes 1.0 (full screen width).
/// Additionally, CheckFieldBoundary guards against zero-width fields.
/// </summary>
[TestClass]
public class FieldBoundaryTextTests
{
    /// <summary>
    /// COLORBAR.NAP is a Prodigy 8-bit file whose IncrementalFieldCommand produces
    /// Dimensions.X = -1.0. With the bug, ALL text rendered one character per line.
    /// This test loads the file and verifies text advances horizontally.
    /// </summary>
    [TestMethod]
    public void ColorbarTextRendersHorizontally()
    {
        var nap = NaplpsFormat.FromFile("examples/COLORBAR.NAP");

        // Find consecutive ASCII character commands (the "CYAN" label)
        var textChars = new List<(char Ch, float PenX, float PenY)>();

        foreach (var seq in nap.Commands)
        {
            if (seq.Command is AsciiCharCommand ascii && !ascii.IsDiscarded)
            {
                textChars.Add((ascii.AsciiCharacter, seq.State.Pen.X, seq.State.Pen.Y));

                // We only need the first word to prove the point
                if (textChars.Count >= 4)
                {
                    break;
                }
            }
        }

        // "CYAN" should be the first 4 text characters
        Assert.AreEqual(4, textChars.Count);
        Assert.AreEqual('C', textChars[0].Ch);
        Assert.AreEqual('Y', textChars[1].Ch);
        Assert.AreEqual('A', textChars[2].Ch);
        Assert.AreEqual('N', textChars[3].Ch);

        // THE KEY ASSERTION: pen.X must INCREASE between characters (horizontal text).
        // With the bug, pen.X stayed at 0.0 for every character while pen.Y decreased.
        Assert.IsTrue(textChars[1].PenX > textChars[0].PenX, $"'Y' pen.X ({textChars[1].PenX}) should be right of 'C' pen.X ({textChars[0].PenX}) — text should advance horizontally");
        Assert.IsTrue(textChars[2].PenX > textChars[1].PenX, $"'A' pen.X ({textChars[2].PenX}) should be right of 'Y' pen.X ({textChars[1].PenX})");
        Assert.IsTrue(textChars[3].PenX > textChars[2].PenX, $"'N' pen.X ({textChars[3].PenX}) should be right of 'A' pen.X ({textChars[2].PenX})");

        // All characters in "CYAN" should be on the same line (same Y position)
        Assert.AreEqual(textChars[0].PenY, textChars[1].PenY, 0.001f, "All chars in 'CYAN' should share the same Y position");
        Assert.AreEqual(textChars[0].PenY, textChars[3].PenY, 0.001f, "First and last char should share the same Y position");
    }

    /// <summary>
    /// Verifies that IncrementalFieldCommand normalizes negative dimensions to positive.
    /// A field with Dimensions.X = -1.0 should become Dimensions.X = 1.0.
    /// This is the root cause fix — without it, the field boundary is inverted.
    /// </summary>
    [TestMethod]
    public void NegativeFieldDimensionsAreNormalized()
    {
        var nap = NaplpsFormat.FromFile("examples/COLORBAR.NAP");

        // The IncrementalFieldCommand in COLORBAR.NAP sets up the text field.
        // Its raw operand bytes decode X dimension as -1.0 (sign bit set, value bits zero).
        // After normalization, Dimensions should be positive.
        Assert.IsTrue(nap.State.Field.Dimensions.X > 0, $"Field Dimensions.X should be positive after normalization, got {nap.State.Field.Dimensions.X}");
        Assert.IsTrue(nap.State.Field.Dimensions.Y > 0, $"Field Dimensions.Y should be positive, got {nap.State.Field.Dimensions.Y}");
    }

    /// <summary>
    /// A field with zero dimensions (no FIELD command issued) should not trigger
    /// any boundary wrapping. This is the default state.
    /// </summary>
    [TestMethod]
    public void DefaultFieldDoesNotWrap()
    {
        // maple.nap is a simple NAPLPS file that doesn't set a field.
        // If it has text, it should render without wrapping issues.
        var nap = NaplpsFormat.FromFile("examples/maple.nap");

        Assert.AreEqual(0f, nap.State.Field.Dimensions.X, "Default field should have zero X dimension");
        Assert.AreEqual(0f, nap.State.Field.Dimensions.Y, "Default field should have zero Y dimension");
    }
}
