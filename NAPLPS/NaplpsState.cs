// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.ComponentModel;

using static NAPLPS.NaplpsCSet;
using static NAPLPS.NaplpsGSet;

namespace NAPLPS;

/// <summary>This was normally stored in some memory block, 4Kb iirc</summary>
public class NaplpsState
{
    public static JsonSerializerOptions GlobalJsonSerializerOptions { get; } = new()
    {
        Converters = { new Vector3Converter(), new Vector2Converter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /* State Recording Utilities */

    public string ToJson() => JsonSerializer.Serialize(this, GlobalJsonSerializerOptions);

    public static NaplpsState FromJson(string json) => JsonSerializer.Deserialize<NaplpsState>(json, GlobalJsonSerializerOptions) ?? new();

    /* In-Use Tables */

    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public NaplpsCSet C0 { get; set; } = C0Set;

    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public NaplpsCSet C1 { get; set; } = C1Set;

    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public byte GL { get; set; } = 0;

    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public NaplpsGSet G0 { get; set; } = PrimaryCharSet;

    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public NaplpsGSet G1 { get; set; } = PDISet;
    
    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public NaplpsGSet G2 { get; set; } = SupplementalSet;

    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public NaplpsGSet G3 { get; set; } = MosiacSet;

    [Category("In-Use Tables")]
    [ReadOnly(true)]
    public bool InLockingManner { get; set; } = false;

    /* Parsing States */

    /// <summary>Future Spec [2 = x,y,0 3 = x,y,z]</summary>
    /// <figure>11</figure>
    [Category("Parsing")]
    [ReadOnly(true)]
    public byte Dimensionality { get; set; } = 2;

    [Category("Parsing")]
    [ReadOnly(true)]
    public byte MultiByteValue { get; set; } = 3;

    [Category("Parsing")]
    [ReadOnly(true)]
    public byte SingleByteValue { get; set; } = 1;

    /* Drawing States */

    [Category("Drawing")]
    [ReadOnly(true)]
    public Point LogicalPel { get; set; } = new(1, 1);

    [Category("Drawing")]
    [ReadOnly(true)]
    public Vector3 Pen { get; set; } = new();

    [Category("Drawing")]
    [ReadOnly(true)]
    public NaplpsField Field { get; set; } = new();

    /* Text States */

    /// <summary>
    /// Rotation causes the character field and the cursor to rotate counterclockwise
    /// about the character field origin.This rotation is measured relative to
    /// horizontal within the unit screen and is independent of the character path.
    /// The character field origin is the lower left corner of the character field at the
    /// default 0 degree rotation regardless of the sign of the character field
    /// dimensions dx and dy. All alphanumeric characters (including
    /// diacritical marks and underlines), DRCS, mosaics, and separated mosaic
    /// characters, and the underline produced when underline mode (see 6.2.7.15) is in
    /// effect, are affected by rotation so that the relative position of the images
    /// within the character field is unchanged.
    /// </summary>
    [Category("Text")]
    [ReadOnly(true)]
    public TextRotation TextRotation { get; set; }

    /// <summary>
    /// This determines the direction of the character path, that is, the direction in 
    /// which the cursor is advanced after a character is deposited. The character path is 
    /// defined relative to horizontal within the unit screen and is independent of the character rotation.
    /// The default character path is right.
    /// </summary>
    [Category("Text")]
    [ReadOnly(true)]
    public TextPath TextPath { get; set; }

    /// <summary>
    /// This determines the distance the cursor is moved after a character is displayed or
    /// after a SPACE or APB(backspace) or APF(horizontal tab) character is
    /// received. The distance the cursor is moved is in multiples of the character
    /// field width(dx) or height(dy), whichever lies parallel to the character path,
    /// depending on the character path and character rotation. This is known as the
    /// intercharacter spacing.
    /// </summary>
    [Category("Text")]
    [ReadOnly(true)]
    public TextSpacing TextSpacing { get; set; }

    [Category("Text")]
    [ReadOnly(true)]
    public TextInterrowSpacing TextInterrowSpacing { get; set; }

    [Category("Text")]
    [ReadOnly(true)]
    public TextMoveAttributes TextMoveAttributes { get; set; }

    [Category("Text")]
    [ReadOnly(true)]
    public TextCursorStyle TextCursorStyle { get; set; }

    /// <summary>
    /// The default dimensions of the character field are dx = 1/40 and dy = 5/128,
    /// consistent with the physical resolution.
    /// </summary>
    [Category("Text")]
    [ReadOnly(true)]
    public Vector2 TextFieldSize { get; set; } = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);

    /* Color States */

    [Category("Color")]
    [ReadOnly(true)]
    public byte ColorMode { get; set; }

    [Category("Color")]
    [ReadOnly(true)]
    public Dictionary<byte, NaplpsColor> ColorMap { get; set; } = new(ColorMapDefaults);

    [Category("Color")]
    [ReadOnly(true)]
    public byte ColorMapForeground { get; set; }

    [Category("Color")]
    [ReadOnly(true)]
    public byte ColorMapBackground { get; set; }

    [Category("Color")]
    [ReadOnly(true)]
    public bool IsTransparent { get; set; }

    [Category("Color")]
    [ReadOnly(true)]
    public NaplpsColor Foreground { get; set; } = new(1, 1, 1);

    [Category("Color")]
    [ReadOnly(true)]
    public NaplpsColor Background { get; set; } = new();

    public override string ToString()
    {
        return $"{MultiByteValue}/{SingleByteValue} <{Pen.X},{Pen.Y}>({LogicalPel.X},{LogicalPel.Y}) <{ColorMode},<{ColorMapForeground}, {ColorMapBackground}> F:{Foreground} B:{Background}";
    }

    public NaplpsState Clone()
    {
        return FromJson(ToJson());
    }

    /* Defaults */

    public static readonly Dictionary<byte, NaplpsColor> ColorMapDefaults = new()
    {
        {0x0, NaplpsColor.From3BitGRB(0, 0, 0)},
        {0x1, NaplpsColor.From3BitGRB(1, 1, 1)},
        {0x2, NaplpsColor.From3BitGRB(2, 2, 2)},
        {0x3, NaplpsColor.From3BitGRB(3, 3, 3)},
        {0x4, NaplpsColor.From3BitGRB(4, 4, 4)},
        {0x5, NaplpsColor.From3BitGRB(5, 5, 5)},
        {0x6, NaplpsColor.From3BitGRB(6, 6, 6)},
        {0x7, NaplpsColor.From3BitGRB(7, 7, 7)},
        {0x8, NaplpsColor.From3BitGRB(0, 0, 7)},
        {0x9, NaplpsColor.From3BitGRB(0, 5, 7)},
        {0xA, NaplpsColor.From3BitGRB(0, 7, 4)},
        {0xB, NaplpsColor.From3BitGRB(2, 7, 0)},
        {0xC, NaplpsColor.From3BitGRB(7, 7, 0)},
        {0xD, NaplpsColor.From3BitGRB(7, 2, 0)},
        {0xE, NaplpsColor.From3BitGRB(7, 0, 4)},
        {0xF, NaplpsColor.From3BitGRB(5, 0, 7)},
    };
}
