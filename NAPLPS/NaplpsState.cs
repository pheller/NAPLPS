// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.ComponentModel;
using AC = NAPLPS.Commands.AsciiCharCommand;
using CC = NAPLPS.Commands.ControlCommand;
using MC = NAPLPS.Commands.MosaicElementCommand;
using NCR = NAPLPS.NaplpsCommandReference;

namespace NAPLPS;

/// <summary>This was normally stored in some memory block, 4Kb iirc</summary>
public class NaplpsState
{
    public static JsonSerializerOptions GlobalJsonSerializerOptions { get; } = new()
    {
        Converters = { new NCRArrayJsonConverter(), new Vector3JsonConverter(), new Vector2JsonConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /* Initialization */

    public static readonly int C0 = 0;
    public static readonly int GLeft = 32;
    public static readonly int C1 = 128;
    public static readonly int GRight = 160;

    public NaplpsState()
    {
        Reset();
    }

    /* In-Use Table Manipulation */

    public void Reset()
    {
        // 7-Bit Default In-Use Table
        C0Set.CopyTo(InUseTable, C0);
        PrimaryCharacterSet.CopyTo(InUseTable, GLeft);

        // 8-Bit Default In-Use Table
        C1Set.CopyTo(InUseTable, C1);
        GeneralPDISet.CopyTo(InUseTable, GRight);
    }

    public void DoShiftIn()
    {
        PrimaryCharacterSet.CopyTo(InUseTable, GLeft);
        InLockingManner = true;
    }

    public void DoShiftOut()
    {
        GeneralPDISet.CopyTo(InUseTable, GLeft);
        InLockingManner = true;
    }

    public bool DoEscape(NaplpsOperands sequence)
    {
        // 7-bit escape sequence for C-Set Designation requires two bytes
        if (sequence.Count < 2)
        {
            return false;
        }

        switch (sequence[0])
        {
            case CC.EscapeC0Set:
            {
                C0Set.CopyTo(InUseTable, C0);
            }
            break;
        }

        return true;
    }

    /* In-Use Tables */
    [Browsable(false)]
    public NCR[] InUseTable { get; set; } = new NCR[256];

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
    public Vector2 LogicalPel { get; set; } = new(0f, 0f);

    [Category("Drawing")]
    [ReadOnly(true)]
    public Vector3 Pen { get; set; } = new();

    [Category("Drawing")]
    [ReadOnly(true)]
    public NaplpsField Field { get; set; } = new();

    [Category("Drawing")]
    [ReadOnly(true)]
    public NaplpsTexture Texture { get; set; } = new();

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
    public Vector2 CharSize { get; set; } = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);

    /* C1 Control States */

    /// <summary>Reverse video mode swaps foreground/background for text</summary>
    [Category("C1 Controls")]
    [ReadOnly(true)]
    public bool IsReverseVideo { get; set; } = false;

    /// <summary>Underline mode draws underline under text characters</summary>
    [Category("C1 Controls")]
    [ReadOnly(true)]
    public bool IsUnderline { get; set; } = false;

    /// <summary>Text size mode: 0=normal, 1=small, 2=medium, 3=double height, 4=double size</summary>
    [Category("C1 Controls")]
    [ReadOnly(true)]
    public byte TextSizeMode { get; set; } = 0;

    /// <summary>Scroll mode enables scrolling when text reaches bottom of field</summary>
    [Category("C1 Controls")]
    [ReadOnly(true)]
    public bool IsScrollMode { get; set; } = false;

    /// <summary>Set to true when an APD in scroll mode would move pen below field origin</summary>
    [Browsable(false)]
    [JsonIgnore]
    public bool ScrollEventOccurred { get; set; } = false;

    /// <summary>Word wrap mode wraps text at word boundaries</summary>
    /// <summary>
    /// ANSI X3.110 §5.3.2.3.6: "If an explicit APR APD (or APD APR) sequence is received
    /// after an automatic APR APD is executed but before the character field origin is moved,
    /// aligned, or set by any other received command or sequence, the explicit APR APD (or
    /// APD APR) sequence shall be executed as a null operation."
    /// Set after auto-wrap fires. Cleared by any command that moves/aligns/sets the cursor,
    /// or after the explicit APR+APD pair is consumed as a no-op.
    /// </summary>
    [Browsable(false)]
    [JsonIgnore]
    public bool AutoWrapJustOccurred { get; set; } = false;

    [Category("C1 Controls")]
    [ReadOnly(true)]
    public bool IsWordWrapMode { get; set; } = false;

    /// <summary>Pen position at the last word break point (space or special char) for word wrap</summary>
    [Browsable(false)]
    [JsonIgnore]
    public Vector3 LastWordBreakPen { get; set; } = new();

    /// <summary>Blink mode causes subsequent text to blink</summary>
    [Category("C1 Controls")]
    [ReadOnly(true)]
    public bool IsBlinkMode { get; set; } = false;

    /// <summary>Protect mode prevents field from being modified</summary>
    [Category("C1 Controls")]
    [ReadOnly(true)]
    public bool IsProtectMode { get; set; } = false;

    /* Macro States */

    /// <summary>Storage for defined macros (keyed by macro name character)</summary>
    [Browsable(false)]
    public Dictionary<char, byte[]> Macros { get; set; } = new();

    /// <summary>Currently defining a macro (null if not in macro definition mode)</summary>
    [Browsable(false)]
    public char? MacroBeingDefined { get; set; } = null;

    /// <summary>Type of macro being defined: 0=DEF MACRO, 1=DEFP MACRO, 2=DEFT MACRO</summary>
    [Browsable(false)]
    public byte MacroDefType { get; set; } = 0;

    /// <summary>Buffer for collecting macro bytes during definition</summary>
    [Browsable(false)]
    public List<byte> MacroBuffer { get; set; } = new();

    /* DRCS States */

    /// <summary>Storage for DRCS character bitmaps (keyed by character code)</summary>
    [Browsable(false)]
    public Dictionary<byte, bool[,]> DrcsCharacters { get; set; } = new();

    /// <summary>Currently defining DRCS characters (null if not in DRCS definition mode)</summary>
    [Browsable(false)]
    public byte? DrcsStartCode { get; set; } = null;

    /// <summary>Buffer for collecting DRCS bitmap data during definition</summary>
    [Browsable(false)]
    public List<byte> DrcsBuffer { get; set; } = new();

    /* Texture Pattern States */

    /// <summary>Programmable texture mask A (default: vertical hatching)</summary>
    [Browsable(false)]
    public bool[,]? TextureMaskA { get; set; } = null;

    /// <summary>Programmable texture mask B (default: horizontal hatching)</summary>
    [Browsable(false)]
    public bool[,]? TextureMaskB { get; set; } = null;

    /// <summary>Programmable texture mask C (default: cross hatching)</summary>
    [Browsable(false)]
    public bool[,]? TextureMaskC { get; set; } = null;

    /// <summary>Programmable texture mask D (default: solid)</summary>
    [Browsable(false)]
    public bool[,]? TextureMaskD { get; set; } = null;

    /// <summary>Currently defining texture pattern (null if not in texture definition mode)</summary>
    [Browsable(false)]
    public byte? TextureBeingDefined { get; set; } = null;

    /// <summary>Buffer for collecting texture pattern data during definition</summary>
    [Browsable(false)]
    public List<byte> TextureBuffer { get; set; } = new();

    /* Blink Animation States */

    /// <summary>List of active blink processes</summary>
    [Browsable(false)]
    public List<BlinkProcess> BlinkProcesses { get; set; } = new();

    /* Color States */

    [Category("Color")]
    [ReadOnly(true)]
    public byte ColorMode { get; set; }

    [Category("Color")]
    [ReadOnly(true)]
    public Dictionary<byte, NaplpsColor> ColorMap { get; set; } = new(ColorMapDefaults);

    [Category("Color")]
    [ReadOnly(true)]
    public byte ColorMapForeground { get; set; } = 0x07;

    [Category("Color")]
    [ReadOnly(true)]
    public byte ColorMapBackground { get; set; }

    [Category("Color")]
    [ReadOnly(true)]
    public bool IsTransparent { get; set; }

    /// <summary>Tracks which palette entries have been used by SET COLOR or SELECT COLOR since last reset (for mode 0 auto-allocation).</summary>
    [Browsable(false)]
    public HashSet<byte> UsedPaletteEntries { get; set; } = new();

    [Category("Color")]
    [ReadOnly(true)]
    public NaplpsColor Foreground { get; set; } = NaplpsColor.White;

    [Category("Color")]
    [ReadOnly(true)]
    public NaplpsColor Background { get; set; } = new();

    /* Error Tracking */

    [JsonIgnore]
    public List<NaplpsError> Errors { get; } = [];

    public void RecordError(NaplpsErrorSeverity severity, NaplpsErrorType type, string message, byte? opcode = null, long? streamPosition = null)
    {
        Errors.Add(new NaplpsError(severity, type, message, opcode, streamPosition));

#if DEBUG
        if (severity == NaplpsErrorSeverity.Error)
        {
            System.Diagnostics.Debugger.Break();
        }
#endif
    }

    /* Helpers */

    public string ToJson() => JsonSerializer.Serialize(this, GlobalJsonSerializerOptions);

    public static NaplpsState FromJson(string json) => JsonSerializer.Deserialize<NaplpsState>(json, GlobalJsonSerializerOptions) ?? new();

    public NaplpsState Clone()
    {
        var json = ToJson();

        return FromJson(json);
    }

    // This is the "viewer" property that will show in the PropertyGrid.
    [JsonIgnore]
    [Category("In-Use Tables")]
    [ReadOnly(true)]
    [DisplayName("In-Use Table")]
    [Description("Human-readable view of the 256-entry in-use table.")]
    public string InUseTableView => FormatInUseTable();

    private string FormatInUseTable()
    {
        static bool Matches(NCR[] table, int offset, NCR[] set)
        {
            if (offset + set.Length > table.Length)
            {
                return false;
            }

            for (int i = 0; i < set.Length; i++)
            {
                if (!ReferenceEquals(table[offset + i], set[i]))
                {
                    return false;
                }
            }

            return true;
        }

        string Resolve(int offset, params (string name, NCR[] set)[] candidates)
        {
            foreach (var (name, set) in candidates)
            {
                if (Matches(InUseTable, offset, set))
                {
                    return name;
                }
            }

            return "Unknown";
        }

        var c0 = Resolve(C0, ("C0Set", C0Set));
        var gLeft = Resolve(GLeft, ("PrimaryCharacterSet", PrimaryCharacterSet), ("SupplementaryCharacterSet", SupplementaryCharacterSet), ("GeneralPDISet", GeneralPDISet), ("MosiacSet", MosiacSet));
        var c1 = Resolve(C1, ("C1Set", C1Set));
        var gRight = Resolve(GRight, ("GeneralPDISet", GeneralPDISet), ("MosiacSet", MosiacSet));

        return $"C0={c0}\nGLeft={gLeft}\nC1={c1}\nGRight={gRight}";
    }

    public override string ToString()
    {
        return $"{MultiByteValue}/{SingleByteValue} <{ColorMode},<{ColorMapForeground:D2}, {ColorMapBackground:D2}> F:{Foreground:D2} B:{Background:D2} <{Pen.X},{Pen.Y}>({LogicalPel.X},{LogicalPel.Y})";
    }

    /* Defaults */

    /// <summary>
    /// Generates a default palette per ANSI X3.110 algorithm:
    /// First half = uniformly spaced greyscale (R=G=B).
    /// Second half = hues equally spaced around the hue circle
    /// (Blue at 0, Red at 120, Green at 240 degrees).
    /// </summary>
    public static Dictionary<byte, NaplpsColor> GenerateDefaultPalette(int entryCount)
    {
        var palette = new Dictionary<byte, NaplpsColor>();
        int half = entryCount / 2;

        // First half: greyscale
        for (int i = 0; i < half; i++)
        {
            byte val = (byte)(i * 255 / Math.Max(1, half - 1));
            palette[(byte)i] = new NaplpsColor(val, val, val);
        }

        // Second half: hues around the circle
        // Blue=0, Red=120, Green=240 degrees
        for (int i = 0; i < half; i++)
        {
            float angle = i * 360.0f / half;
            float blueAngle = 0f, redAngle = 120f, greenAngle = 240f;

            // Find P1 (closest primary), P2 (second closest), P3 (farthest)
            float distB = Math.Min(Math.Abs(angle - blueAngle), 360f - Math.Abs(angle - blueAngle));
            float distR = Math.Min(Math.Abs(angle - redAngle), 360f - Math.Abs(angle - redAngle));
            float distG = Math.Min(Math.Abs(angle - greenAngle), 360f - Math.Abs(angle - greenAngle));

            var primaries = new (float dist, float angleP, int idx)[] { (distB, blueAngle, 2), (distR, redAngle, 1), (distG, greenAngle, 0) };
            Array.Sort(primaries, (a, b) => a.dist.CompareTo(b.dist));

            float[] rgb = new float[3]; // [G, R, B]
            rgb[primaries[0].idx] = 1.0f;
            rgb[primaries[1].idx] = Math.Clamp(Math.Abs(angle - primaries[0].angleP) / 60f, 0f, 1f);

            // Handle wrap-around for angle distance
            float p2dist = Math.Min(Math.Abs(angle - primaries[0].angleP), 360f - Math.Abs(angle - primaries[0].angleP));
            rgb[primaries[1].idx] = Math.Clamp(p2dist / 60f, 0f, 1f);
            rgb[primaries[2].idx] = 0f;

            palette[(byte)(half + i)] = new NaplpsColor((byte)(rgb[0] * 255), (byte)(rgb[1] * 255), (byte)(rgb[2] * 255));
        }

        return palette;
    }

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

    // Prodigy color palette - NaplpsColor constructor is (green, red, blue)
    public static readonly Dictionary<byte, NaplpsColor> ColorMapProdigyDefaults = new()
    {
        {0x0, new NaplpsColor(0x00, 0x00, 0x00)},  // Black
        {0x1, new NaplpsColor(0x00, 0xAA, 0x00)},  // Red
        {0x2, new NaplpsColor(0x55, 0x55, 0x55)},  // Grey
        {0x3, new NaplpsColor(0x00, 0x00, 0xAA)},  // Blue
        {0x4, new NaplpsColor(0xAA, 0xAA, 0xAA)},  // Light Grey
        {0x5, new NaplpsColor(0x55, 0xAA, 0x00)},  // Brown
        {0x6, new NaplpsColor(0xAA, 0x00, 0x00)},  // Green
        {0x7, new NaplpsColor(0xFF, 0xFF, 0xFF)},  // White
        {0x8, new NaplpsColor(0x55, 0x55, 0xFF)},  // Light Blue
        {0x9, new NaplpsColor(0x00, 0xAA, 0xAA)},  // Magenta
        {0xA, new NaplpsColor(0x55, 0xFF, 0xFF)},  // Light Magenta
        {0xB, new NaplpsColor(0x55, 0xFF, 0x55)},  // Light Red
        {0xC, new NaplpsColor(0xFF, 0xFF, 0x55)},  // Yellow
        {0xD, new NaplpsColor(0xFF, 0x55, 0x55)},  // Light Green
        {0xE, new NaplpsColor(0xFF, 0x55, 0xFF)},  // Light Cyan
        {0xF, new NaplpsColor(0xAA, 0x00, 0xAA)},  // Cyan
    };

    public static readonly NCR[] C0Set =
    [
        new NCR(typeof(CC), Null),
        new NCR(typeof(CC), StartOfHeading),
        new NCR(typeof(CC), StartOfText),
        new NCR(typeof(CC), EndOfText),
        new NCR(typeof(CC), EndOfTransmission),
        new NCR(typeof(CC), Enquiry),
        new NCR(typeof(CC), Acknowledge),
        new NCR(typeof(CC), Bell),
        new NCR(typeof(CC), ActivePositionBackward),
        new NCR(typeof(CC), ActivePositionForward),
        new NCR(typeof(CC), ActivePositionDown),
        new NCR(typeof(CC), ActivePositionUp),
        new NCR(typeof(CC), ClearScreen),
        new NCR(typeof(CC), ActivePositionReturn),
        new NCR(typeof(CC), ShiftOut),
        new NCR(typeof(CC), ShiftIn),
        new NCR(typeof(CC), DataLinkEscape),
        new NCR(typeof(CC), DeviceControl1),
        new NCR(typeof(CC), DeviceControl2),
        new NCR(typeof(CC), DeviceControl3),
        new NCR(typeof(CC), DeviceControl4),
        new NCR(typeof(CC), NegativeAcknowledge),
        new NCR(typeof(CC), SynchronousIdle),
        new NCR(typeof(CC), EndOfBlock),
        new NCR(typeof(CC), Cancel),
        new NCR(typeof(CC), SingleShiftTwo),
        new NCR(typeof(CC), ServiceDelimiterCharacter),
        new NCR(typeof(CC), Escape),
        new NCR(typeof(CC), ActivePositionSet),
        new NCR(typeof(CC), SingleShiftThree),
        new NCR(typeof(CC), ActivePositionHome),
        new NCR(typeof(CC), NonSelectiveReset),
    ];

    public static readonly NCR[] C1Set =
    [
        new NCR(typeof(CC), DefMacro),
        new NCR(typeof(CC), DefPMacro),
        new NCR(typeof(CC), DefTMacro),
        new NCR(typeof(CC), DefDRCS),
        new NCR(typeof(CC), DefTexture),
        new NCR(typeof(CC), End),
        new NCR(typeof(CC), Repeat),
        new NCR(typeof(CC), RepeatToEOL),
        new NCR(typeof(CC), ReverseVideo),
        new NCR(typeof(CC), NormalVideo),
        new NCR(typeof(CC), SmallText),
        new NCR(typeof(CC), MedText),
        new NCR(typeof(CC), NormalText),
        new NCR(typeof(CC), DoubleHeight),
        new NCR(typeof(CC), BlinkStart),
        new NCR(typeof(CC), DoubleSize),
        new NCR(typeof(CC), Protect),
        new NCR(typeof(CC), EDC1),
        new NCR(typeof(CC), EDC2),
        new NCR(typeof(CC), EDC3),
        new NCR(typeof(CC), EDC4),
        new NCR(typeof(CC), WordWrapOn),
        new NCR(typeof(CC), WordWrapOff),
        new NCR(typeof(CC), ScrollOn),
        new NCR(typeof(CC), ScrollOff),
        new NCR(typeof(CC), UnderLineStart),
        new NCR(typeof(CC), UnderLineStop),
        new NCR(typeof(CC), FlashCursor),
        new NCR(typeof(CC), SteadyCursor),
        new NCR(typeof(CC), CursorOff),
        new NCR(typeof(CC), BlinkStop),
        new NCR(typeof(CC), Unprotect)
    ];

    public static readonly NCR[] PrimaryCharacterSet =
    [
        new NCR(typeof(AC), ' '),
        new NCR(typeof(AC), '!'),
        new NCR(typeof(AC), '"'),
        new NCR(typeof(AC), '#'),
        new NCR(typeof(AC), '$'),
        new NCR(typeof(AC), '%'),
        new NCR(typeof(AC), '&'),
        new NCR(typeof(AC), '\''),
        new NCR(typeof(AC), '('),
        new NCR(typeof(AC), ')'),
        new NCR(typeof(AC), '*'),
        new NCR(typeof(AC), '+'),
        new NCR(typeof(AC), ','),
        new NCR(typeof(AC), '-'),
        new NCR(typeof(AC), '.'),
        new NCR(typeof(AC), '/'),

        new NCR(typeof(AC), '0'),
        new NCR(typeof(AC), '1'),
        new NCR(typeof(AC), '2'),
        new NCR(typeof(AC), '3'),
        new NCR(typeof(AC), '4'),
        new NCR(typeof(AC), '5'),
        new NCR(typeof(AC), '6'),
        new NCR(typeof(AC), '7'),
        new NCR(typeof(AC), '8'),
        new NCR(typeof(AC), '9'),
        new NCR(typeof(AC), ':'),
        new NCR(typeof(AC), ';'),
        new NCR(typeof(AC), '<'),
        new NCR(typeof(AC), '='),
        new NCR(typeof(AC), '>'),
        new NCR(typeof(AC), '?'),

        new NCR(typeof(AC), '@'),
        new NCR(typeof(AC), 'A'),
        new NCR(typeof(AC), 'B'),
        new NCR(typeof(AC), 'C'),
        new NCR(typeof(AC), 'D'),
        new NCR(typeof(AC), 'E'),
        new NCR(typeof(AC), 'F'),
        new NCR(typeof(AC), 'G'),
        new NCR(typeof(AC), 'H'),
        new NCR(typeof(AC), 'I'),
        new NCR(typeof(AC), 'J'),
        new NCR(typeof(AC), 'K'),
        new NCR(typeof(AC), 'L'),
        new NCR(typeof(AC), 'M'),
        new NCR(typeof(AC), 'N'),
        new NCR(typeof(AC), 'O'),

        new NCR(typeof(AC), 'P'),
        new NCR(typeof(AC), 'Q'),
        new NCR(typeof(AC), 'R'),
        new NCR(typeof(AC), 'S'),
        new NCR(typeof(AC), 'T'),
        new NCR(typeof(AC), 'U'),
        new NCR(typeof(AC), 'V'),
        new NCR(typeof(AC), 'W'),
        new NCR(typeof(AC), 'X'),
        new NCR(typeof(AC), 'Y'),
        new NCR(typeof(AC), 'Z'),
        new NCR(typeof(AC), '['),
        new NCR(typeof(AC), '\\'),
        new NCR(typeof(AC), ']'),
        new NCR(typeof(AC), '^'),
        new NCR(typeof(AC), '_'),

        new NCR(typeof(AC), '`'),
        new NCR(typeof(AC), 'a'),
        new NCR(typeof(AC), 'b'),
        new NCR(typeof(AC), 'c'),
        new NCR(typeof(AC), 'd'),
        new NCR(typeof(AC), 'e'),
        new NCR(typeof(AC), 'f'),
        new NCR(typeof(AC), 'g'),
        new NCR(typeof(AC), 'h'),
        new NCR(typeof(AC), 'i'),
        new NCR(typeof(AC), 'j'),
        new NCR(typeof(AC), 'k'),
        new NCR(typeof(AC), 'l'),
        new NCR(typeof(AC), 'm'),
        new NCR(typeof(AC), 'n'),
        new NCR(typeof(AC), 'o'),

        new NCR(typeof(AC), 'p'),
        new NCR(typeof(AC), 'q'),
        new NCR(typeof(AC), 'r'),
        new NCR(typeof(AC), 's'),
        new NCR(typeof(AC), 't'),
        new NCR(typeof(AC), 'u'),
        new NCR(typeof(AC), 'v'),
        new NCR(typeof(AC), 'w'),
        new NCR(typeof(AC), 'x'),
        new NCR(typeof(AC), 'y'),
        new NCR(typeof(AC), 'z'),
        new NCR(typeof(AC), '{'),
        new NCR(typeof(AC), '|'),
        new NCR(typeof(AC), '}'),
        new NCR(typeof(AC), '~'),
        new NCR(typeof(DeleteCommand))
    ];

    public static readonly NCR[] SupplementaryCharacterSet =
    [
        new NCR(typeof(AC), ' '),
        new NCR(typeof(AC), '¡'),
        new NCR(typeof(AC), '¢'),
        new NCR(typeof(AC), '£'),
        new NCR(typeof(AC), '$'),
        new NCR(typeof(AC), '¥'),
        new NCR(typeof(AC), '#'),
        new NCR(typeof(AC), '§'),
        new NCR(typeof(AC), '¤'),
        new NCR(typeof(AC), '‘'),
        new NCR(typeof(AC), '“'),
        new NCR(typeof(AC), '«'),
        new NCR(typeof(AC), '←'),
        new NCR(typeof(AC), '↑'),
        new NCR(typeof(AC), '→'),
        new NCR(typeof(AC), '↓'),

        new NCR(typeof(AC), '°'),
        new NCR(typeof(AC), '±'),
        new NCR(typeof(AC), '²'),
        new NCR(typeof(AC), '³'),
        new NCR(typeof(AC), '×'),
        new NCR(typeof(AC), 'µ'),
        new NCR(typeof(AC), '¶'),
        new NCR(typeof(AC), '·'),
        new NCR(typeof(AC), '÷'),
        new NCR(typeof(AC), '’'),
        new NCR(typeof(AC), '”'),
        new NCR(typeof(AC), '»'),
        new NCR(typeof(AC), '¼'),
        new NCR(typeof(AC), '½'),
        new NCR(typeof(AC), '¾'),
        new NCR(typeof(AC), '¿'),

        // Non Spacing Diacritical Marks
        new NCR(typeof(AC), '→'),
        new NCR(typeof(AC), '`'),
        new NCR(typeof(AC), '´'),
        new NCR(typeof(AC), 'ˆ'),
        new NCR(typeof(AC), '~'),
        new NCR(typeof(AC), '¯'),
        new NCR(typeof(AC), '˘'),
        new NCR(typeof(AC), '˙'),
        new NCR(typeof(AC), '¨'),
        new NCR(typeof(AC), '/'),
        new NCR(typeof(AC), '˚'),
        new NCR(typeof(AC), '¸'),
        new NCR(typeof(AC), '_'),
        new NCR(typeof(AC), '˝'),
        new NCR(typeof(AC), '˛'),
        new NCR(typeof(AC), 'ˇ'),

        new NCR(typeof(AC), '―'),
        new NCR(typeof(AC), '¹'),
        new NCR(typeof(AC), '®'),
        new NCR(typeof(AC), '©'),
        new NCR(typeof(AC), '™'),
        new NCR(typeof(AC), '♪'),
        new NCR(typeof(AC), '─'),
        new NCR(typeof(AC), '│'),
        new NCR(typeof(AC), '╱'),
        new NCR(typeof(AC), '╲'),
        new NCR(typeof(AC), '◢'),
        new NCR(typeof(AC), '◣'),
        new NCR(typeof(AC), '⅛'),
        new NCR(typeof(AC), '⅜'),
        new NCR(typeof(AC), '⅝'),
        new NCR(typeof(AC), '⅞'),

        new NCR(typeof(AC), 'Ω'),
        new NCR(typeof(AC), 'Æ'),
        new NCR(typeof(AC), 'Ð'),
        new NCR(typeof(AC), 'ª'),
        new NCR(typeof(AC), 'Ħ'),
        new NCR(typeof(AC), '┼'),
        new NCR(typeof(AC), 'Ĳ'),
        new NCR(typeof(AC), 'Ŀ'),
        new NCR(typeof(AC), 'Ł'),
        new NCR(typeof(AC), 'Ø'),
        new NCR(typeof(AC), 'Œ'),
        new NCR(typeof(AC), 'º'),
        new NCR(typeof(AC), 'Þ'),
        new NCR(typeof(AC), 'Ŧ'),
        new NCR(typeof(AC), 'Ŋ'),
        new NCR(typeof(AC), 'ŉ'),

        new NCR(typeof(AC), 'ĸ'),
        new NCR(typeof(AC), 'æ'),
        new NCR(typeof(AC), 'đ'),
        new NCR(typeof(AC), 'ð'),
        new NCR(typeof(AC), 'ħ'),
        new NCR(typeof(AC), 'ı'),
        new NCR(typeof(AC), 'ĳ'),
        new NCR(typeof(AC), 'ŀ'),
        new NCR(typeof(AC), 'ł'),
        new NCR(typeof(AC), 'ø'),
        new NCR(typeof(AC), 'œ'),
        new NCR(typeof(AC), 'ß'),
        new NCR(typeof(AC), 'þ'),
        new NCR(typeof(AC), 'ŧ'),
        new NCR(typeof(AC), 'ŋ'),
        new NCR(typeof(DeleteCommand))
    ];

    public static readonly NCR[] GeneralPDISet =
    [
        new NCR(typeof(ResetCommand)),
        new NCR(typeof(DomainCommand)),
        new NCR(typeof(TextCommand)),
        new NCR(typeof(TextureCommand)),
        new NCR(typeof(PointSetAbsoluteCommand)),
        new NCR(typeof(PointSetRelativeCommand)),
        new NCR(typeof(PointAbsoluteCommand)),
        new NCR(typeof(PointRelativeCommand)),
        new NCR(typeof(LineAbsoluteCommand)),
        new NCR(typeof(LineRelativeCommand)),
        new NCR(typeof(LineSetAbsoluteCommand)),
        new NCR(typeof(LineSetRelativeCommand)),
        new NCR(typeof(ArcOutlinedCommand)),
        new NCR(typeof(ArcFilledCommand)),
        new NCR(typeof(ArcSetOutlinedCommand)),
        new NCR(typeof(ArcSetFilledCommand)),

        new NCR(typeof(RectangleOutlinedCommand)),
        new NCR(typeof(RectangleFilledCommand)),
        new NCR(typeof(RectangleSetOutlinedCommand)),
        new NCR(typeof(RectangleSetFilledCommand)),
        new NCR(typeof(PolygonOutlinedCommand)),
        new NCR(typeof(PolygonFilledCommand)),
        new NCR(typeof(PolygonSetOutlinedCommand)),
        new NCR(typeof(PolygonSetFilledCommand)),
        new NCR(typeof(IncrementalFieldCommand)),
        new NCR(typeof(IncrementalPointCommand)),
        new NCR(typeof(IncrementalLineCommand)),
        new NCR(typeof(IncrementalPolygonFilledCommand)),
        new NCR(typeof(SetColorCommand)),
        new NCR(typeof(WaitCommand)),
        new NCR(typeof(SelectColorCommand)),
        new NCR(typeof(BlinkCommand)),

        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),

        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),

        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),

        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
    ];

    /// <summary>
    /// Telidon v699 PDI set. Same as GeneralPDISet but with Incremental commands
    /// and Blink replaced with NaplpsCommand (no-op) — these didn't exist in the
    /// original 1978 Telidon specification.
    /// </summary>
    public static readonly NCR[] TelidonPDISet =
    [
        new NCR(typeof(ResetCommand)),          // 0x20/0xA0
        new NCR(typeof(DomainCommand)),         // 0x21/0xA1
        new NCR(typeof(TextCommand)),           // 0x22/0xA2
        new NCR(typeof(TextureCommand)),        // 0x23/0xA3
        new NCR(typeof(PointSetAbsoluteCommand)),
        new NCR(typeof(PointSetRelativeCommand)),
        new NCR(typeof(PointAbsoluteCommand)),
        new NCR(typeof(PointRelativeCommand)),
        new NCR(typeof(LineAbsoluteCommand)),
        new NCR(typeof(LineRelativeCommand)),
        new NCR(typeof(LineSetAbsoluteCommand)),
        new NCR(typeof(LineSetRelativeCommand)),
        new NCR(typeof(ArcOutlinedCommand)),
        new NCR(typeof(ArcFilledCommand)),
        new NCR(typeof(ArcSetOutlinedCommand)),
        new NCR(typeof(ArcSetFilledCommand)),

        new NCR(typeof(RectangleOutlinedCommand)),
        new NCR(typeof(RectangleFilledCommand)),
        new NCR(typeof(RectangleSetOutlinedCommand)),
        new NCR(typeof(RectangleSetFilledCommand)),
        new NCR(typeof(PolygonOutlinedCommand)),
        new NCR(typeof(PolygonFilledCommand)),
        new NCR(typeof(PolygonSetOutlinedCommand)),
        new NCR(typeof(PolygonSetFilledCommand)),
        new NCR(typeof(IncrementalFieldCommand)),
        new NCR(typeof(NaplpsCommand)),         // Telidon: no IncrementalPoint
        new NCR(typeof(NaplpsCommand)),         // Telidon: no IncrementalLine
        new NCR(typeof(NaplpsCommand)),         // Telidon: no IncrementalPolygonFilled
        new NCR(typeof(SetColorCommand)),
        new NCR(typeof(WaitCommand)),
        new NCR(typeof(SelectColorCommand)),
        new NCR(typeof(NaplpsCommand)),         // Telidon: no Blink

        // Remainder: NumericalDataCommand (same as GeneralPDISet)
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),

        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),

        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),

        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
        new NCR(typeof(NumericalDataCommand)),
    ];

    public static readonly NCR[] MosiacSet =
    [
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [ true, false, false, false, false, false]),
        new NCR(typeof(MC), [false,  true, false, false, false, false]),
        new NCR(typeof(MC), [ true,  true, false, false, false, false]),
        new NCR(typeof(MC), [false, false,  true, false, false, false]),
        new NCR(typeof(MC), [ true, false,  true, false, false, false]),
        new NCR(typeof(MC), [false,  true,  true, false, false, false]),
        new NCR(typeof(MC), [ true,  true,  true, false, false, false]),
        new NCR(typeof(MC), [false, false, false,  true, false, false]),
        new NCR(typeof(MC), [ true, false, false,  true, false, false]),
        new NCR(typeof(MC), [false,  true, false,  true, false, false]),
        new NCR(typeof(MC), [ true,  true, false,  true, false, false]),
        new NCR(typeof(MC), [false, false,  true,  true, false, false]),
        new NCR(typeof(MC), [ true, false,  true,  true, false, false]),
        new NCR(typeof(MC), [false,  true,  true,  true, false, false]),
        new NCR(typeof(MC), [ true,  true,  true,  true, false, false]),

        new NCR(typeof(MC), [false, false, false, false,  true, false]),
        new NCR(typeof(MC), [ true, false, false, false,  true, false]),
        new NCR(typeof(MC), [false,  true, false, false,  true, false]),
        new NCR(typeof(MC), [ true,  true, false, false,  true, false]),
        new NCR(typeof(MC), [false, false,  true, false,  true, false]),
        new NCR(typeof(MC), [ true, false,  true, false,  true, false]),
        new NCR(typeof(MC), [false,  true,  true, false,  true, false]),
        new NCR(typeof(MC), [ true,  true,  true, false,  true, false]),
        new NCR(typeof(MC), [false, false, false,  true,  true, false]),
        new NCR(typeof(MC), [ true, false, false,  true,  true, false]),
        new NCR(typeof(MC), [false,  true, false,  true,  true, false]),
        new NCR(typeof(MC), [ true,  true, false,  true,  true, false]),
        new NCR(typeof(MC), [false, false,  true,  true,  true, false]),
        new NCR(typeof(MC), [ true, false,  true,  true,  true, false]),
        new NCR(typeof(MC), [false,  true,  true,  true,  true, false]),
        new NCR(typeof(MC), [ true,  true,  true,  true,  true, false]),

        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),

        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [false, false, false, false, false, false]),
        new NCR(typeof(MC), [ true,  true,  true,  true,  true,  true]),

        new NCR(typeof(MC), [false, false, false, false, false,  true]), // 0
        new NCR(typeof(MC), [ true, false, false, false, false,  true]), // 1
        new NCR(typeof(MC), [false,  true, false, false, false,  true]), // 2
        new NCR(typeof(MC), [ true,  true, false, false, false,  true]), // 3
        new NCR(typeof(MC), [false, false,  true, false, false,  true]), // 4
        new NCR(typeof(MC), [ true, false,  true, false, false,  true]), // 5
        new NCR(typeof(MC), [false,  true,  true, false, false,  true]), // 6
        new NCR(typeof(MC), [ true,  true,  true, false, false,  true]), // 7
        new NCR(typeof(MC), [false, false, false,  true, false,  true]), // 8
        new NCR(typeof(MC), [ true, false, false,  true, false,  true]), // 9
        new NCR(typeof(MC), [false,  true, false,  true, false,  true]), // 10
        new NCR(typeof(MC), [ true,  true, false,  true, false,  true]), // 11
        new NCR(typeof(MC), [false, false,  true,  true, false,  true]), // 12
        new NCR(typeof(MC), [ true, false,  true,  true, false,  true]), // 13
        new NCR(typeof(MC), [false,  true,  true,  true, false,  true]), // 14
        new NCR(typeof(MC), [ true,  true,  true,  true, false,  true]), // 15

        new NCR(typeof(MC), [false, false, false, false,  true,  true]), // 0
        new NCR(typeof(MC), [ true, false, false, false,  true,  true]), // 1
        new NCR(typeof(MC), [false,  true, false, false,  true,  true]), // 2
        new NCR(typeof(MC), [ true,  true, false, false,  true,  true]), // 3
        new NCR(typeof(MC), [false, false,  true, false,  true,  true]), // 4
        new NCR(typeof(MC), [ true, false,  true, false,  true,  true]), // 5
        new NCR(typeof(MC), [false,  true,  true, false,  true,  true]), // 6
        new NCR(typeof(MC), [ true,  true,  true, false,  true,  true]), // 7
        new NCR(typeof(MC), [false, false, false,  true,  true,  true]), // 8
        new NCR(typeof(MC), [ true, false, false,  true,  true,  true]), // 9
        new NCR(typeof(MC), [false,  true, false,  true,  true,  true]), // 10
        new NCR(typeof(MC), [ true,  true, false,  true,  true,  true]), // 11
        new NCR(typeof(MC), [false, false,  true,  true,  true,  true]), // 12
        new NCR(typeof(MC), [ true, false,  true,  true,  true,  true]), // 13
        new NCR(typeof(MC), [false,  true,  true,  true,  true,  true]), // 14
        new NCR(typeof(MC), [ true,  true,  true,  true,  true,  true]), // 15
    ];
}
