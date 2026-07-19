// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS
//
// Vector-stroke character generator font for Prodigy text. Each glyph is a list of straight-line
// stroke segments on an integer grid (x 0..6, y 0..9; baseline y=2, cap y=8, y increasing upward).
// Directory is 96 records (chars 0x20..0x7F), 6 bytes each: leftBearing, advanceWidth, u16 dataLen,
// u16 dataPtr (offset into the stroke blob). Rendered as integer strokes to match the original
// hard-edged glyphs.

namespace NAPLPS.Drawing;

public static class MvdiFont
{
    public const int Baseline = 2;
    public const int CapY = 8;
    public const int GridMaxX = 6;
    public const int GridMaxY = 9;
    public const int FirstChar = 0x20;
    public const int GlyphCount = 96;

    /// <summary>
    /// Grid em: glyph stroke coords and per-glyph advance widths are in grid units; one grid unit
    /// maps to (CharSize * ScreenWidth / Em) device pixels. Calibrated against the reference render. Both
    /// glyph rendering and the pen advance use these so shapes and spacing stay consistent.
    /// </summary>
    public const float EmW = 7.5f;
    public const float EmH = 10f;

    // Horizontal grid->device metrics, calibrated pixel-exactly at every corpus size. Indexed by the
    // X char-size register kx = round(CharSize.X*256). The glyph's device X for grid column gx is
    //   deviceX(gx) = round_half_up(640*penX + HStep[kx]*gx)
    // (the pen stays fractional inside the round). HStep is a NONLINEAR staircase, not a clean em:
    // kx<=5 tracks 0.25*kx, but kx>=6 diverges (kx6->2.0, kx8->2.5, kx10->3.5). PhX is the
    // vertical-stroke device pel width (also kx-keyed). Tables span k=0..40; out of range
    // extrapolates ~0.297*k.
    private static readonly double[] HStep =
    {
        0.75, 0.75, 0.75, 0.75, 1.0, 1.25, 2.0, 2.0, 2.5, 2.5, 3.5, 3.5, 3.5, 4.0, 4.25, 4.625,
        5.0, 5.25, 5.5, 6.0, 6.5, 6.75, 7.0, 7.25, 7.5, 7.75, 8.0, 8.25, 8.5, 8.75, 9.0, 9.25,
        9.5, 9.75, 10.0, 10.25, 10.5, 10.75, 11.0, 11.25, 11.5,
    };

    private static readonly int[] PhX =
    {
        1, 1, 1, 1, 1, 1, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 6, 7, 8, 8, 8, 8, 9, 9, 9,
        10, 10, 10, 10, 10, 11, 11, 11, 12, 12, 12,
    };

    /// <summary>Device px per glyph grid unit (horizontal) for X char-size register kx.</summary>
    public static double HorizStep(int kx) =>
        kx < 0 ? HStep[0] : kx < HStep.Length ? HStep[kx] : 0.2969 * kx;

    /// <summary>Vertical-stroke device pel width for X char-size register kx.</summary>
    public static int VertStrokePel(int kx) =>
        Math.Max(1, kx < 0 ? PhX[0] : kx < PhX.Length ? PhX[kx] : (int)Math.Round(0.30 * kx));

    /// <summary>
    /// Horizontal-stroke device pel thickness (the stroke HEIGHT) for the Y char-size register
    /// ky = round(CharSize.Y*256). Calibrated as a staircase (dense ky sweep, crossbar of 'H'):
    /// 1px through ky=8, 2px for ky 9..24, then +1 every 8 ky (3 at 25..32, 4 at 33..40, ...),
    /// verified against the sparse ky sweep out to ky=90. The old round(CharSize.Y*640/20)
    /// under-rounded the dominant body-text size (ky=10 -> 1.25 -> 1px, but the reference draws 2px),
    /// thinning every horizontal stroke and leaving a 1px red edge on
    /// body text across the corpus. The vertical shrink (0.779 display ratio) is why horizontal
    /// strokes stay far thinner than vertical (PhX) at the same register.
    /// </summary>
    public static int HorizStrokePel(int ky) =>
        ky <= 8 ? 1 : ky <= 24 ? 2 : 3 + (ky - 25) / 8;

    // Vertical row map, calibrated as a staircase (dense E + '=' sweep, band tops for grid rows
    // 2/4/5/6/8 across ky 8..40). The linear DevY formula (calibrated only at the baseline g=2 and
    // cap g=8) rounds interior rows a pixel off - e.g. the g=4 crossbars of a/e/s/G. The reference
    // per-row device offset from the pen is exactly the linear interpolation
    // between the baseline offset Off2(ky) and the cap offset Off8(ky), then rounded:
    //   offset(g,ky) = round(Off2 + (Off8-Off2)*(g-2)/6).
    // Both tables are themselves staircases (no clean closed form), indexed ky=8..40; outside that
    // range they extrapolate off the end slope.
    private static readonly int[] Off2 =
        { 5, 6, 7, 7, 8, 8, 9, 9, 10, 10, 10, 11, 12, 12, 12, 13, 14, 15, 15, 16, 16, 17, 17, 18, 18, 20, 20, 21, 21, 22, 22, 23, 23 };
    private static readonly int[] Off8 =
        { 15, 19, 21, 23, 24, 26, 29, 31, 32, 34, 36, 39, 40, 42, 43, 45, 48, 51, 52, 54, 56, 59, 61, 62, 64, 66, 69, 71, 73, 74, 76, 79, 81 };

    private static double RowRefOffset(int[] tab, int ky)
    {
        if (ky <= 8) return tab[0] * ky / 8.0;
        if (ky - 8 < tab.Length) return tab[ky - 8];
        int n = tab.Length;
        return tab[n - 1] + (tab[n - 1] - tab[n - 2]) * (ky - (8 + n - 1));
    }

    /// <summary>
    /// Recovered device-Y offset (from the pen) of a horizontal glyph stroke's top pixel at grid
    /// row <paramref name="gridY"/> for the Y char-size register <paramref name="ky"/>. Linear
    /// interpolation between the reference-measured baseline (g=2) and cap (g=8) offsets; fixes the
    /// interior-row crossbars the linear DevY formula mis-rounded.
    /// </summary>
    public static int HorizRowTopOffset(int gridY, int ky)
    {
        double o2 = RowRefOffset(Off2, ky);
        double o8 = RowRefOffset(Off8, ky);
        double v = o2 + (o8 - o2) * (gridY - 2) / 6.0;
        // At/above the baseline the reference-measured staircase rounds; below it (descenders g/j/p/q/y,
        // gridY 0..1) the offset extrapolates off the Off2 end where MVDI truncates, so round-half-up
        // overshoots by 1px and the descender-tail diagonal steps a row early. Floor there.
        return gridY < Baseline
            ? (int)Math.Floor(v)
            : (int)Math.Round(v, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// MVDI's own horizontal advance for a character in NAPLPS normalized units, given the
    /// character-field width (normalized): advanceWidth(grid) * fieldWidth / EmW.
    /// </summary>
    public static double AdvanceNormalized(char c, double charFieldWidthNormalized)
    {
        return ForChar(c).AdvanceWidth * charFieldWidthNormalized / EmW;
    }

    /// <summary>
    /// Exact Prodigy proportional horizontal advance. The advance depends only on the glyph
    /// AdvanceWidth (0..6) and the char-size register k = round(CharSize.X*256), expressed as an
    /// integer count of 1/256-screen units (device px = g*2.5 at 640 wide, so the returned normalized
    /// advance is g/256). There is no clean closed form: the metric is a per-glyph integer
    /// computation that is even non-monotonic in k for narrow glyphs. The table is the measured
    /// ground truth for
    /// k in [6,40]; outside that (rare) range fall back to the large-glyph asymptotic (AW+2)/8 cell.
    /// </summary>
    public static double ProdigyProportionalAdvanceNorm(char c, double charSizeX)
    {
        int k = (int)Math.Round(charSizeX * 256.0);
        // k=3,4: the advance is genuinely FRACTIONAL here (~6.3px for most letters), so integer 1/256 g
        // is too coarse - it rounds up and over-advances, drifting small labels (e.g. 1.nap's list).
        // Return the reference-measured advance stored in 0.1 device-px units: advance_norm = t/10/640.
        if (k >= 3 && k <= 4 && c >= 0x21 && c <= 0x7E)
        {
            return AdvancePx01K3K4[k - 3][c - 0x21] / 6400.0;
        }
        int g;
        // At k=5..7 the advance is genuinely PER-GLYPH, measured from the reference render, not a function of the
        // AdvanceWidth class: narrow-ink punctuation (',' ';') advances less and '#'/'Q' more than their
        // class. Those deviations vanish for k>=8, where the AW-class table is exact.
        if (k >= 5 && k <= 7 && c >= 0x21 && c <= 0x7E)
        {
            g = PerGlyphAdvanceG5to7[k - 5][c - 0x21];
        }
        else
        {
            int aw = Math.Clamp(ForChar(c).AdvanceWidth, 0, 6);
            g = (k >= 6 && k <= 40)
                ? ProportionalAdvanceG[k - 6][aw]
                : (int)Math.Round((aw + 2) * k / 8.0);
        }
        return g / 256.0;
    }

    // Per-glyph proportional advance for k=3,4 in 0.1 device-px units (advance_norm = t/10/640), chars
    // 0x21..0x7E. Measured from the reference render. Stored
    // fractionally because integer 1/256 g rounds up and drifts small labels at these sizes.
    private static readonly short[][] AdvancePx01K3K4 =
    {
        new short[] { 25, 51, 75, 75, 75, 75, 25, 37, 37, 75, 75, 37, 63, 25, 75, 63, 37, 63, 63, 63, 63, 63, 63, 63, 63, 25, 37, 63, 63, 63, 63, 75, 63, 63, 63, 63, 63, 63, 63, 63, 50, 63, 63, 63, 75, 63, 75, 63, 75, 63, 63, 75, 63, 75, 75, 75, 75, 75, 50, 75, 50, 50, 75, 37, 63, 63, 63, 63, 63, 63, 63, 63, 25, 50, 63, 25, 75, 63, 63, 63, 63, 63, 63, 50, 63, 75, 75, 75, 63, 63, 63, 25, 63, 75 },
        new short[] { 25, 63, 87, 100, 100, 100, 25, 37, 37, 100, 100, 50, 75, 25, 100, 75, 37, 75, 75, 75, 75, 75, 75, 75, 75, 25, 50, 75, 87, 75, 87, 100, 75, 75, 75, 75, 75, 75, 87, 75, 50, 75, 75, 75, 100, 75, 100, 75, 87, 75, 75, 100, 75, 100, 100, 100, 100, 100, 63, 100, 63, 50, 100, 37, 75, 75, 75, 75, 75, 75, 75, 75, 25, 63, 75, 25, 100, 75, 75, 75, 75, 75, 75, 50, 75, 100, 100, 100, 75, 75, 75, 25, 75, 100 },
    };

    // Per-glyph proportional advance g (1/256-screen units) for k=5,6,7, chars 0x21..0x7E.
    // Measured from the reference render; supersedes the AW-class table at these sizes where per-glyph deviation
    // matters. Rows: k=5, k=6, k=7.
    private static readonly byte[][] PerGlyphAdvanceG5to7 =
    {
        new byte[] { 2, 3, 4, 5, 5, 5, 2, 2, 2, 5, 5, 3, 4, 2, 5, 4, 2, 4, 4, 4, 4, 4, 4, 4, 4, 2, 3, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4, 3, 4, 4, 4, 5, 4, 5, 4, 4, 4, 4, 5, 4, 5, 5, 5, 5, 5, 3, 5, 3, 3, 5, 2, 4, 4, 4, 4, 4, 4, 4, 4, 2, 3, 4, 2, 5, 4, 4, 4, 4, 4, 4, 3, 4, 5, 5, 5, 4, 4, 4, 2, 4, 5 },
        new byte[] { 2, 4, 6, 6, 6, 6, 2, 3, 3, 6, 6, 3, 5, 2, 6, 5, 3, 5, 5, 5, 5, 5, 5, 5, 5, 2, 3, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 6, 5, 6, 5, 6, 5, 5, 6, 5, 6, 6, 6, 6, 6, 4, 6, 4, 4, 6, 3, 5, 5, 5, 5, 5, 5, 5, 5, 2, 4, 5, 2, 6, 5, 5, 5, 5, 5, 5, 4, 5, 6, 6, 6, 5, 5, 5, 2, 5, 6 },
        new byte[] { 3, 5, 7, 7, 7, 7, 3, 4, 4, 7, 7, 4, 6, 3, 7, 6, 4, 6, 6, 6, 6, 6, 6, 6, 6, 3, 4, 6, 6, 6, 6, 7, 6, 6, 6, 6, 6, 6, 6, 6, 5, 6, 6, 6, 7, 6, 7, 6, 7, 6, 6, 7, 6, 7, 7, 7, 7, 7, 5, 7, 5, 5, 7, 4, 6, 6, 6, 6, 6, 6, 6, 6, 3, 5, 6, 3, 7, 6, 6, 6, 6, 6, 6, 5, 6, 7, 7, 7, 6, 6, 6, 3, 6, 7 },
    };

    // g(AdvanceWidth, k) = advance in 1/256-screen units. Rows k=6..40, cols AdvanceWidth 0..6.
    private static readonly byte[][] ProportionalAdvanceG =
    {
        new byte[] { 2, 3, 4, 4, 5, 5, 6 },     // k=6
        new byte[] { 3, 4, 5, 5, 6, 6, 7 },     // k=7
        new byte[] { 2, 3, 4, 5, 6, 7, 8 },     // k=8
        new byte[] { 3, 4, 5, 6, 7, 8, 9 },     // k=9
        new byte[] { 4, 5, 6, 7, 8, 9, 10 },    // k=10
        new byte[] { 3, 4, 6, 7, 8, 10, 11 },   // k=11
        new byte[] { 4, 5, 7, 8, 9, 11, 12 },   // k=12
        new byte[] { 3, 5, 6, 8, 10, 11, 13 },  // k=13
        new byte[] { 4, 6, 7, 9, 11, 12, 14 },  // k=14
        new byte[] { 5, 7, 8, 10, 12, 13, 15 }, // k=15
        new byte[] { 4, 6, 8, 10, 12, 14, 16 }, // k=16
        new byte[] { 5, 7, 9, 11, 13, 15, 17 }, // k=17
        new byte[] { 4, 6, 9, 11, 13, 16, 18 }, // k=18
        new byte[] { 5, 7, 10, 12, 14, 17, 19 },// k=19
        new byte[] { 6, 8, 11, 13, 15, 18, 20 },// k=20
        new byte[] { 5, 8, 10, 13, 16, 18, 21 },// k=21
        new byte[] { 6, 9, 11, 14, 17, 19, 22 },// k=22
        new byte[] { 5, 8, 11, 14, 17, 20, 23 },// k=23
        new byte[] { 6, 9, 12, 15, 18, 21, 24 },// k=24
        new byte[] { 5, 8, 12, 15, 18, 22, 25 },// k=25
        new byte[] { 6, 9, 13, 16, 19, 23, 26 },// k=26
        new byte[] { 7, 10, 14, 17, 20, 24, 27 },// k=27
        new byte[] { 6, 10, 13, 17, 21, 24, 28 },// k=28
        new byte[] { 7, 11, 14, 18, 22, 25, 29 },// k=29
        new byte[] { 6, 10, 14, 18, 22, 26, 30 },// k=30
        new byte[] { 7, 11, 15, 19, 23, 27, 31 },// k=31
        new byte[] { 6, 10, 15, 19, 23, 28, 32 },// k=32
        new byte[] { 7, 11, 16, 20, 24, 29, 33 },// k=33
        new byte[] { 8, 12, 17, 21, 25, 30, 34 },// k=34
        new byte[] { 7, 12, 16, 21, 26, 30, 35 },// k=35
        new byte[] { 8, 13, 17, 22, 27, 31, 36 },// k=36
        new byte[] { 7, 12, 17, 22, 27, 32, 37 },// k=37
        new byte[] { 8, 13, 18, 23, 28, 33, 38 },// k=38
        new byte[] { 7, 12, 18, 23, 28, 34, 39 },// k=39
        new byte[] { 8, 13, 19, 24, 29, 35, 40 },// k=40
    };

    /// <summary>One glyph: metrics + straight-line stroke segments (x0,y0,x1,y1 per segment).</summary>
    public readonly record struct Glyph(int LeftBearing, int AdvanceWidth, int[] Segments);

    private static readonly byte[] Directory = [0,6,0,0,0,0,3,0,8,0,0,0,1,3,8,0,8,0,0,5,16,0,16,0,0,6,32,0,32,0,0,6,36,0,64,0,0,6,36,0,100,0,3,0,4,0,136,0,3,1,12,0,140,0,3,1,12,0,152,0,0,6,12,0,164,0,0,6,8,0,176,0,2,2,8,0,184,0,1,4,4,0,192,0,3,0,4,0,196,0,0,6,4,0,200,0,1,4,44,0,204,0,3,1,8,0,248,0,1,4,28,0,0,1,1,4,44,0,28,1,1,4,16,0,72,1,1,4,32,0,88,1,1,4,36,0,120,1,1,4,16,0,156,1,1,4,68,0,172,1,1,4,36,0,240,1,3,0,8,0,20,2,2,2,12,0,28,2,1,4,8,0,40,2,0,5,8,0,48,2,1,4,8,0,56,2,0,5,24,0,64,2,0,6,44,0,88,2,1,4,24,0,132,2,1,4,40,0,156,2,1,4,28,0,196,2,1,4,24,0,224,2,1,4,16,0,248,2,1,4,12,0,8,3,0,5,36,0,20,3,1,4,12,0,56,3,3,2,20,0,68,3,1,4,16,0,88,3,1,4,12,0,104,3,1,4,8,0,116,3,0,6,16,0,124,3,1,4,12,0,140,3,0,6,40,0,152,3,1,4,24,0,192,3,0,5,36,0,216,3,1,4,32,0,252,3,1,4,44,0,28,4,0,6,8,0,72,4,1,4,20,0,80,4,0,6,16,0,100,4,0,6,28,0,116,4,0,6,8,0,144,4,0,6,20,0,152,4,0,6,12,0,172,4,1,3,12,0,184,4,0,6,4,0,196,4,1,3,12,0,200,4,2,2,8,0,212,4,0,6,4,0,220,4,3,1,4,0,224,4,1,4,28,0,228,4,1,4,24,0,0,5,1,4,20,0,24,5,1,4,24,0,44,5,1,4,32,0,68,5,1,4,20,0,100,5,1,4,32,0,120,5,1,4,20,0,152,5,3,0,8,0,172,5,1,3,20,0,180,5,1,4,12,0,200,5,3,0,4,0,212,5,0,6,36,0,216,5,1,4,20,0,252,5,1,4,32,0,16,6,1,4,24,0,48,6,1,4,24,0,72,6,1,4,12,0,96,6,1,4,28,0,108,6,3,2,8,0,136,6,1,4,20,0,144,6,0,6,16,0,164,6,0,6,36,0,180,6,0,6,8,0,216,6,1,4,28,0,224,6,1,4,12,0,252,6,1,4,32,0,8,7,3,0,4,0,40,7,1,4,32,0,44,7,0,6,20,0,76,7,0,6,0,0,0,0];
    private static readonly byte[] Blob = [3,2,3,2,3,4,3,8,1,8,1,6,4,8,4,6,1,8,1,3,4,3,4,8,5,7,0,7,0,4,5,4,3,8,3,2,5,7,1,7,1,7,0,6,0,6,1,5,1,5,5,5,5,5,6,4,6,4,5,3,5,3,1,3,0,2,6,8,0,7,0,8,0,8,1,8,1,8,1,7,1,7,0,7,5,2,5,3,5,3,6,3,6,3,6,2,6,2,5,2,6,4,4,2,4,2,1,2,1,2,0,3,0,3,0,4,0,4,4,7,4,7,3,8,3,8,2,8,2,8,1,7,1,7,6,2,3,8,3,6,4,8,3,7,3,7,3,3,3,3,4,2,3,8,4,7,4,7,4,3,4,3,3,2,1,2,5,6,5,2,1,6,0,4,6,4,0,5,6,5,3,7,3,3,4,3,4,2,4,2,3,1,1,5,5,5,3,2,3,2,0,2,6,8,3,2,2,2,2,2,1,3,1,3,1,7,1,7,2,8,2,8,3,8,3,2,4,2,4,2,5,3,5,3,5,7,5,7,4,8,4,8,3,8,5,7,1,3,3,7,4,8,4,8,4,2,5,2,1,2,1,2,1,3,1,3,5,6,5,6,5,7,5,7,4,8,4,8,2,8,2,8,1,7,1,7,2,8,2,8,4,8,4,8,5,7,5,7,5,6,5,6,4,5,4,5,3,5,4,5,5,4,5,4,5,3,5,3,4,2,4,2,2,2,2,2,1,3,4,2,4,8,4,8,1,5,1,5,1,4,1,4,5,4,1,3,2,2,2,2,4,2,4,2,5,3,5,3,5,5,5,5,4,6,4,6,1,6,1,6,1,8,1,8,5,8,4,8,2,8,2,8,1,7,1,7,1,3,1,3,2,2,2,2,4,2,4,2,5,3,5,3,5,4,5,4,4,5,4,5,1,5,2,2,2,4,2,4,5,7,5,7,5,8,5,8,1,8,3,2,2,2,2,2,1,3,1,3,1,4,1,4,2,5,2,5,1,6,1,6,1,7,1,7,2,8,2,8,3,8,3,2,4,2,4,2,5,3,5,3,5,4,5,4,4,5,4,5,2,5,4,5,5,6,5,6,5,7,5,7,4,8,4,8,3,8,2,2,4,2,4,2,5,3,5,3,5,7,5,7,4,8,4,8,2,8,2,8,1,7,1,7,1,6,1,6,2,5,2,5,5,5,3,3,3,3,3,5,3,5,4,3,4,2,4,2,3,1,4,5,4,5,5,2,1,5,1,5,5,8,0,6,5,6,5,4,0,4,1,8,5,5,5,5,1,2,0,7,1,8,1,8,4,8,4,8,5,7,5,7,5,6,5,6,3,4,3,2,3,2,5,2,2,2,2,2,0,4,0,4,0,6,0,6,2,8,2,8,5,8,5,8,6,7,6,7,6,4,6,4,4,4,4,4,3,5,3,5,4,6,4,6,6,6,1,2,1,7,1,7,2,8,2,8,4,8,4,8,5,7,5,7,5,2,5,5,1,5,1,2,1,8,1,8,4,8,4,8,5,7,5,7,5,6,5,6,4,5,4,5,1,5,4,5,5,4,5,4,5,3,5,3,4,2,4,2,1,2,5,7,4,8,4,8,2,8,2,8,1,7,1,7,1,3,1,3,2,2,2,2,4,2,4,2,5,3,1,2,4,2,4,2,5,3,5,3,5,7,5,7,4,8,4,8,1,8,2,8,2,2,5,2,1,2,1,2,1,8,1,8,5,8,4,5,1,5,1,2,1,8,1,8,5,8,4,5,1,5,5,7,4,8,4,8,1,8,1,8,0,7,0,7,0,3,0,3,1,2,1,2,4,2,4,2,5,3,5,3,5,4,5,4,3,4,1,2,1,8,5,8,5,2,5,5,1,5,3,2,5,2,5,2,4,2,4,2,4,8,4,8,3,8,3,8,5,8,1,3,2,2,2,2,4,2,4,2,5,3,5,3,5,8,1,2,1,8,5,8,1,4,3,6,5,2,5,2,1,2,1,2,1,8,3,5,0,8,0,8,0,2,3,5,6,8,6,8,6,2,1,2,1,8,1,8,5,2,5,2,5,8,3,2,2,2,2,2,1,3,1,3,1,7,1,7,2,8,2,8,3,8,3,2,4,2,4,2,5,3,5,3,5,7,5,7,4,8,4,8,3,8,1,2,1,8,1,8,4,8,4,8,5,7,5,7,5,6,5,6,4,5,4,5,1,5,0,3,0,7,0,7,1,8,1,8,3,8,3,8,4,7,4,7,4,3,4,3,3,2,3,2,1,2,1,2,0,3,3,4,5,2,1,2,1,8,1,8,4,8,4,8,5,7,5,7,5,6,5,6,4,5,4,5,1,5,3,5,4,4,4,4,5,2,1,3,2,2,2,2,4,2,4,2,5,3,5,3,5,4,5,4,4,5,4,5,2,5,2,5,1,6,1,6,1,7,1,7,2,8,2,8,4,8,4,8,5,7,0,8,6,8,3,8,3,2,1,8,1,3,1,3,2,2,2,2,4,2,4,2,5,3,5,3,5,8,3,2,0,4,0,4,0,8,3,2,6,4,6,4,6,8,0,8,0,2,0,2,2,2,2,2,3,3,3,3,3,5,3,3,4,2,4,2,6,2,6,2,6,8,0,8,6,2,6,8,0,2,3,2,3,4,3,4,0,7,0,7,0,8,3,4,6,7,6,7,6,8,0,8,6,8,6,8,0,2,0,2,6,2,4,2,1,2,1,2,1,8,1,8,4,8,0,8,6,2,1,2,4,2,4,2,4,8,4,8,1,8,2,8,3,9,3,9,4,8,0,1,6,1,3,9,4,8,2,6,4,6,4,6,5,5,5,5,5,2,5,2,2,2,2,2,1,3,1,3,2,4,2,4,5,4,1,8,1,2,1,2,4,2,4,2,5,3,5,3,5,5,5,5,4,6,4,6,1,6,5,2,2,2,2,2,1,3,1,3,1,5,1,5,2,6,2,6,5,6,5,8,5,2,5,2,2,2,2,2,1,3,1,3,1,5,1,5,2,6,2,6,5,6,5,2,2,2,2,2,1,3,1,3,1,5,1,5,2,6,2,6,4,6,4,6,5,5,5,5,5,4,5,4,1,4,2,2,2,7,2,7,3,8,3,8,4,8,4,8,5,7,1,5,4,5,1,0,4,0,4,0,5,1,5,1,5,6,5,6,2,6,2,6,1,5,1,5,1,3,1,3,2,2,2,2,5,2,1,8,1,2,1,5,2,6,2,6,4,6,4,6,5,5,5,5,5,2,3,2,3,6,3,8,3,8,1,1,2,0,2,0,3,0,3,0,4,1,4,1,4,6,4,8,4,8,1,2,1,8,1,4,4,6,3,5,5,2,3,2,3,8,0,2,0,6,0,5,1,6,1,6,2,6,2,6,3,5,3,5,3,3,3,5,4,6,4,6,5,6,5,6,6,5,6,5,6,2,1,2,1,6,1,5,2,6,2,6,4,6,4,6,5,5,5,5,5,2,1,3,1,5,1,5,2,6,2,6,4,6,4,6,5,5,5,5,5,3,5,3,4,2,4,2,2,2,2,2,1,3,1,0,1,6,1,6,4,6,4,6,5,5,5,5,5,3,5,3,4,2,4,2,1,2,5,0,5,6,5,6,2,6,2,6,1,5,1,5,1,3,1,3,2,2,2,2,5,2,1,2,1,6,1,5,4,6,4,6,5,5,1,2,4,2,4,2,5,3,5,3,4,4,4,4,2,4,2,4,1,5,1,5,2,6,2,6,4,6,4,2,4,8,3,6,5,6,1,6,1,3,1,3,2,2,2,2,4,2,4,2,5,3,5,3,5,6,3,2,0,5,0,5,0,6,3,2,6,5,6,5,6,6,0,6,0,3,0,3,1,2,1,2,2,2,2,2,3,3,3,3,3,5,3,3,4,2,4,2,5,2,5,2,6,3,6,3,6,6,0,6,6,2,6,6,0,2,1,0,4,0,4,0,5,1,5,1,5,6,5,3,4,2,4,2,2,2,2,2,1,3,1,3,1,6,5,2,1,2,1,2,5,6,5,6,1,6,5,2,3,2,3,2,2,3,2,3,2,4,2,4,1,5,1,5,2,6,2,6,2,7,2,7,3,8,3,8,5,8,3,2,3,8,1,8,3,8,3,8,4,7,4,7,4,6,4,6,5,5,5,5,4,4,4,4,4,3,4,3,3,2,3,2,1,2,0,8,1,9,1,9,2,9,2,9,4,8,4,8,5,8,5,8,6,9];

    public static readonly Glyph[] Glyphs = Build(Directory, Blob);

    private static Glyph[] Build(byte[] directory, byte[] blob)
    {
        var glyphs = new Glyph[GlyphCount];
        for (int i = 0; i < GlyphCount; i++)
        {
            int o = i * 6;
            int left = directory[o];
            int width = directory[o + 1];
            int len = directory[o + 2] | (directory[o + 3] << 8);
            int ptr = directory[o + 4] | (directory[o + 5] << 8);
            var segs = new int[len];
            for (int j = 0; j < len; j++)
            {
                segs[j] = blob[ptr + j];
            }
            glyphs[i] = new Glyph(left, width, segs);
        }
        return glyphs;
    }

    /// <summary>Returns the glyph for a character, or the space glyph if out of range.</summary>
    public static Glyph ForChar(char c)
    {
        int idx = c - FirstChar;
        return (uint)idx < GlyphCount ? Glyphs[idx] : Glyphs[0];
    }

    // NAPLPS G2 Supplementary character set, the second vector font (same 6-byte record +
    // stroke-blob format, blob-relative pointers). Indexed by G2 code - 0x20, so it is 1:1 aligned
    // with NaplpsState.SupplementaryCharacterSet. Codes 0x41..0x4F are the non-spacing diacritical
    // marks (grave, acute, circumflex, tilde, macron, breve, dot, diaeresis, ring, cedilla,
    // double-acute, ogonek, caron); above-letter marks live at grid y=8..9, below-letter marks
    // (cedilla, ogonek) at y=0..2.
    private static readonly byte[] SupplementaryDirectory = [0,6,0,0,0,0,3,0,8,0,0,0,0,6,32,0,8,0,1,4,24,0,40,0,0,6,32,0,64,0,0,6,20,0,96,0,0,6,16,0,116,0,0,5,48,0,132,0,0,6,48,0,180,0,3,1,16,0,228,0,0,5,32,0,244,0,0,5,16,0,20,1,0,6,12,0,36,1,1,4,12,0,48,1,0,6,12,0,60,1,1,4,12,0,72,1,1,3,32,0,84,1,0,6,12,0,116,1,1,3,20,0,128,1,1,3,24,0,148,1,1,3,8,0,172,1,0,5,16,0,180,1,0,6,32,0,196,1,3,0,4,0,228,1,0,6,12,0,232,1,3,1,16,0,244,1,0,5,32,0,4,2,0,5,16,0,36,2,0,6,20,0,52,2,0,6,24,0,72,2,0,6,40,0,96,2,0,5,24,0,136,2,0,6,8,0,160,2,1,4,4,0,168,2,1,4,4,0,172,2,1,4,8,0,176,2,0,6,20,0,184,2,1,4,4,0,204,2,1,4,12,0,208,2,1,4,4,0,220,2,1,4,8,0,224,2,0,6,4,0,232,2,1,4,16,0,236,2,1,4,16,0,252,2,0,6,4,0,12,3,1,4,8,0,16,3,1,4,16,0,24,3,1,4,8,0,40,3,1,4,4,0,48,3,2,1,8,0,52,3,0,6,52,0,60,3,0,6,52,0,112,3,0,6,24,0,164,3,0,5,40,0,188,3,0,6,4,0,228,3,3,0,4,0,232,3,0,6,4,0,236,3,0,6,4,0,240,3,0,6,44,0,244,3,0,6,48,0,32,4,0,6,28,0,80,4,0,6,48,0,108,4,0,6,48,0,156,4,0,6,40,0,204,4,0,5,44,0,244,4,0,6,28,0,32,5,0,5,28,0,60,5,1,4,32,0,88,5,0,6,16,0,120,5,0,6,8,0,136,5,0,6,20,0,144,5,1,4,12,0,164,5,0,6,12,0,176,5,0,6,36,0,188,5,0,6,44,0,224,5,1,4,36,0,12,6,1,4,28,0,48,6,0,6,12,0,76,6,0,5,24,0,88,6,0,6,24,0,112,6,0,6,20,0,136,6,0,6,56,0,156,6,0,5,28,0,212,6,1,4,36,0,240,6,0,5,24,0,20,7,3,0,4,0,44,7,1,3,24,0,48,7,1,3,12,0,72,7,1,4,12,0,84,7,0,6,36,0,96,7,0,6,64,0,132,7,0,5,36,0,196,7,1,3,20,0,232,7,1,2,12,0,252,7,1,4,24,0,8,8,0,6,0,0,0,0];
    private static readonly byte[] SupplementaryBlob = [3,8,3,8,3,6,3,2,3,2,3,8,6,6,5,7,5,7,1,7,1,7,0,6,0,6,0,4,0,4,1,3,1,3,5,3,5,3,6,4,1,2,5,2,2,2,2,7,2,7,3,8,3,8,4,8,4,8,5,7,3,5,1,5,3,2,3,8,5,7,1,7,1,7,0,6,0,6,1,5,1,5,5,5,5,5,6,4,6,4,5,3,5,3,1,3,0,8,3,6,3,6,6,8,3,6,3,2,1,3,5,3,5,5,1,5,0,3,6,3,6,7,0,7,1,8,1,2,5,2,5,8,0,2,4,2,4,2,5,3,5,3,4,4,4,4,2,4,2,4,1,5,1,5,1,6,1,6,0,7,0,7,1,8,1,8,5,8,1,6,3,6,3,6,4,5,4,5,4,4,2,3,1,4,1,4,1,5,1,5,2,6,2,6,4,6,4,6,5,5,5,5,5,4,5,4,4,3,4,3,2,3,1,3,0,2,0,7,1,6,5,6,6,7,5,3,6,2,3,9,3,7,3,7,4,7,4,7,4,8,4,8,3,8,0,9,0,7,0,7,1,7,1,7,1,8,1,8,0,8,4,9,4,7,4,7,5,7,5,7,5,8,5,8,4,8,2,7,0,5,0,5,2,3,5,3,3,5,3,5,5,7,2,7,0,5,0,5,2,3,0,5,6,5,1,5,3,7,3,7,5,5,3,7,3,3,4,7,6,5,6,5,4,3,6,5,0,5,1,5,3,3,3,3,5,5,3,3,3,7,1,6,1,7,1,7,2,8,2,8,3,8,3,8,4,7,4,7,4,6,4,6,3,5,3,5,2,5,2,5,1,6,0,2,6,2,3,8,3,4,0,6,6,6,1,8,2,9,2,9,3,9,3,9,4,8,4,8,1,5,1,5,4,5,1,9,3,9,3,9,4,8,4,8,3,7,3,7,4,6,4,6,3,5,3,5,1,5,1,6,4,3,4,6,1,3,1,6,1,2,1,2,0,1,1,2,5,2,4,2,4,6,6,2,6,8,6,8,1,8,1,8,0,7,0,7,1,6,1,6,3,6,3,8,3,2,3,2,3,8,3,8,3,6,3,5,3,5,3,3,3,3,3,7,3,7,0,5,6,5,4,7,4,9,4,9,3,9,3,9,3,8,3,8,4,8,1,7,1,9,1,9,0,9,0,9,0,8,0,8,1,8,5,7,5,9,5,9,4,9,4,9,4,8,4,8,5,8,0,7,2,5,2,5,0,3,3,7,5,5,5,5,3,3,0,2,6,8,1,8,1,5,6,2,6,6,6,6,3,3,3,3,6,3,4,5,5,5,5,5,6,4,6,4,4,2,4,2,6,2,0,2,6,8,1,8,1,5,0,2,6,8,6,2,6,6,6,6,3,3,3,3,6,3,0,8,1,8,1,8,2,7,2,7,1,6,1,6,2,5,2,5,1,4,1,4,0,4,5,3,4,2,4,2,1,2,1,2,0,3,0,3,0,4,0,4,2,6,2,8,2,8,0,8,6,8,6,8,4,9,2,9,4,8,2,8,4,9,2,8,3,9,3,9,4,8,0,8,1,9,1,9,2,9,2,9,4,8,4,8,5,8,5,8,6,9,1,9,5,9,1,9,2,8,2,8,4,8,4,8,5,9,3,9,3,9,1,9,1,9,4,9,4,9,0,2,6,8,2,9,3,9,3,9,3,8,3,8,2,8,2,8,2,9,3,2,4,2,4,2,5,1,5,1,4,0,4,0,1,0,0,0,6,0,2,9,1,8,4,9,3,8,3,2,2,2,2,2,1,1,1,1,2,0,2,0,5,0,2,9,3,8,3,8,4,9,1,5,5,5,2,8,3,9,3,9,3,6,0,3,0,7,0,7,1,8,1,8,5,8,5,8,6,7,6,7,6,3,6,3,5,2,5,2,1,2,1,2,0,3,2,3,2,6,2,6,3,6,3,6,4,5,4,5,3,4,3,4,4,3,4,6,3,6,3,6,2,5,2,5,2,4,2,4,3,3,3,3,4,3,1,2,0,3,0,3,0,6,0,6,1,7,1,7,5,7,5,7,6,6,6,6,6,3,6,3,5,2,5,2,1,2,0,8,2,8,1,8,1,5,4,5,4,8,4,8,5,7,5,7,6,8,6,8,6,5,5,6,3,8,3,8,3,3,3,3,2,2,2,2,1,2,1,2,0,3,0,3,1,4,1,4,3,4,3,4,1,2,1,2,1,4,1,3,2,4,0,5,6,5,3,9,3,0,0,0,6,9,0,9,6,0,0,0,6,9,6,9,1,0,1,0,6,7,6,7,2,0,2,0,6,5,6,5,3,0,3,0,6,3,6,3,4,0,4,0,6,1,0,0,6,0,6,0,6,9,6,0,0,9,0,9,5,0,5,0,0,8,0,8,4,0,4,0,0,7,0,7,3,0,3,0,0,6,0,6,2,0,2,0,0,5,0,5,1,0,1,0,0,3,0,0,0,9,0,2,6,8,4,6,6,6,6,6,6,2,6,2,4,2,4,2,4,6,4,4,6,4,1,8,1,5,0,2,6,8,4,6,6,6,6,6,6,2,6,2,4,2,4,2,4,6,4,4,6,4,0,8,1,8,1,8,2,7,2,7,1,6,1,6,2,5,2,5,1,4,1,4,0,4,0,2,6,8,4,6,6,6,6,6,6,2,6,2,4,2,4,2,4,6,4,4,6,4,2,8,0,8,0,8,0,6,0,6,1,6,1,6,2,5,2,5,1,4,1,4,0,4,0,2,6,8,4,6,6,6,6,6,6,2,6,2,4,2,4,2,4,6,4,4,6,4,0,8,2,8,2,8,2,7,2,7,0,5,0,5,0,4,0,2,1,2,1,2,1,3,1,3,0,4,0,4,0,6,0,6,1,7,1,7,4,7,4,7,5,6,5,6,5,4,5,4,4,3,4,3,4,2,4,2,5,2,0,2,0,7,0,7,1,8,1,8,6,8,3,8,3,2,3,2,6,2,0,4,3,4,3,5,5,5,1,2,1,8,1,8,4,8,4,8,5,7,5,7,5,3,5,3,4,2,4,2,1,2,0,5,2,5,1,2,5,2,2,8,4,8,4,8,5,7,5,7,5,4,5,4,2,4,2,4,1,5,1,5,2,6,2,6,5,6,1,2,1,8,5,8,5,2,6,7,0,7,1,5,5,5,0,5,6,5,3,9,3,0,0,8,0,2,3,3,4,2,4,2,5,2,5,2,6,3,6,3,6,8,1,8,1,2,1,2,5,2,4,5,4,5,2,8,2,2,2,2,6,2,0,5,4,8,2,2,1,3,1,3,1,7,1,7,2,8,2,8,4,8,4,8,5,7,5,7,5,3,5,3,4,2,4,2,2,2,0,3,6,7,6,2,3,2,3,2,3,8,3,8,6,8,3,5,5,5,3,3,2,2,2,2,1,2,1,2,0,3,0,3,0,7,0,7,1,8,1,8,2,8,2,8,3,7,1,2,5,2,2,4,1,5,1,5,1,7,1,7,2,8,2,8,4,8,4,8,5,7,5,7,5,5,5,5,4,4,4,4,2,4,1,2,3,2,2,2,2,8,1,8,3,8,2,6,4,6,4,6,5,5,5,5,4,4,4,4,2,4,0,8,6,8,3,8,3,2,1,5,5,5,0,3,0,8,0,7,1,8,1,8,4,8,4,8,5,7,5,7,5,4,5,4,4,2,0,8,0,7,2,2,2,6,2,5,3,6,3,6,5,6,5,6,6,5,6,5,6,2,0,2,4,8,2,4,5,4,5,4,6,5,3,4,5,2,5,2,6,2,1,6,2,6,2,6,3,5,3,5,3,2,3,2,1,2,1,2,0,3,0,3,1,4,1,4,3,4,3,5,4,6,4,6,5,6,5,6,6,5,6,5,6,4,6,4,3,4,3,3,4,2,4,2,6,2,4,8,4,2,4,2,1,2,1,2,0,3,0,3,0,4,0,4,1,5,1,5,4,5,3,7,5,7,1,8,5,5,5,5,5,3,5,3,4,2,4,2,2,2,2,2,1,3,1,3,1,4,1,4,2,5,2,5,5,5,1,6,3,8,1,8,1,2,1,4,2,5,2,5,4,5,4,5,5,4,5,4,5,2,0,7,2,7,3,2,3,6,1,2,1,6,1,8,1,8,4,8,4,8,4,6,4,1,4,1,3,0,3,0,1,0,1,8,1,3,1,3,2,2,4,5,4,5,3,8,3,3,3,3,4,2,1,4,5,7,1,3,1,5,1,5,2,6,2,6,4,6,4,6,5,5,5,5,5,3,5,3,4,2,4,2,2,2,2,2,1,3,0,2,6,6,0,3,0,5,0,5,1,6,1,6,2,6,2,6,3,5,3,5,3,3,3,3,2,2,2,2,1,2,1,2,0,3,3,5,4,6,4,6,5,6,5,6,6,5,6,5,6,4,6,4,3,4,3,4,3,3,3,3,4,2,4,2,6,2,0,2,0,7,0,7,1,8,1,8,2,8,2,8,3,7,3,7,2,6,2,6,4,5,4,5,5,4,5,4,5,3,5,3,4,2,1,8,1,2,1,6,3,6,3,6,4,5,4,5,3,4,3,4,1,4,2,8,2,2,1,6,3,6,1,3,3,4,1,2,1,6,1,5,2,6,2,6,4,6,4,6,5,5,5,5,5,1,5,1,4,0];
    public static readonly Glyph[] SupplementaryGlyphs = Build(SupplementaryDirectory, SupplementaryBlob);

    /// <summary>
    /// Returns the supplementary-set glyph for a G2 code (0x20..0x7F), or the space glyph if
    /// out of range. Used for both spacing supplementary characters and the non-spacing accent
    /// marks composed onto the following base glyph.
    /// </summary>
    public static Glyph ForSupplementary(int g2Code)
    {
        int idx = g2Code - FirstChar;
        return (uint)idx < GlyphCount ? SupplementaryGlyphs[idx] : SupplementaryGlyphs[0];
    }
}
