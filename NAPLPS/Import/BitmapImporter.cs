// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NAPLPS.Import;

/// <summary>
/// Convert a raster bitmap (PNG/JPG/etc) into a NAPLPS byte stream. The image is scaled
/// to a target cell-grid (default 40x30 = 1/40 per cell at aspect ratio 0.75), quantized
/// to the 16-color NAPLPS palette via nearest-neighbor lookup, then emitted as a series
/// of RectangleSetFilled commands — one per contiguous run of same-color cells.
///
/// Simple, visually approximate, doesn't use IncrementalPoint or DRCS — those are more
/// efficient but require bit-level packing work; save that for v2.
/// </summary>
public static class BitmapImporter
{
    /// <summary>
    /// Load an image file and convert to a Telidraw `.td` source string. Output dimensions
    /// are mapped to NAPLPS normalized coords (1 unit wide × 0.75 tall).
    /// </summary>
    public static string ToTelidraw(string imagePath, int cellColumns = 40, int cellRows = 30)
    {
        using var image = Image.Load<Rgba32>(imagePath);
        return ConvertToTelidraw(image, cellColumns, cellRows);
    }

    /// <summary>
    /// Convert an in-memory ImageSharp Image (resize + quantize + emit cell rectangles).
    /// Public for callers that have the image loaded through other means (clipboard, etc.).
    /// </summary>
    public static string ConvertToTelidraw(Image<Rgba32> image, int cellColumns, int cellRows)
    {
        // Resize to target cell grid. Nearest-neighbor for block-mosaic feel; swap to
        // Bicubic if a smoother look is wanted — but then the quantizer has to work harder.
        image.Mutate(ctx => ctx.Resize(cellColumns, cellRows, KnownResamplers.NearestNeighbor));

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// Imported from bitmap");
        sb.AppendLine("#coord fractions");
        sb.AppendLine();

        float cellW = 1.0f / cellColumns;
        float cellH = 0.75f / cellRows;

        // Find contiguous horizontal runs of the same color — emit one rect-set per run.
        // Cheaper than per-pixel and produces cleaner Telidraw output.
        for (int row = 0; row < cellRows; row++)
        {
            int runStart = 0;
            byte runColor = NearestPaletteIndex(image[0, row]);

            for (int col = 1; col <= cellColumns; col++)
            {
                byte c = col < cellColumns ? NearestPaletteIndex(image[col, row]) : (byte)255;

                if (c != runColor || col == cellColumns)
                {
                    if (runColor != 0)  // skip black runs (background)
                    {
                        float x = runStart * cellW;
                        float y = 0.75f - (row + 1) * cellH;  // flip Y (image is y-down, NAPLPS y-up)
                        float w = (col - runStart) * cellW;
                        sb.AppendLine($"color {runColor}");
                        sb.AppendLine($"rect-set {Fmt(x)} {Fmt(y)} {Fmt(w)} {Fmt(cellH)}");
                    }
                    runStart = col;
                    runColor = c;
                }
            }
        }

        return sb.ToString();
    }

    private static string Fmt(float v) => v.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Find the nearest NAPLPS default-palette color to the given pixel. Brute-force
    /// Euclidean distance in RGB space — fine for 16 entries, not tuned for perceptual
    /// accuracy. For better results a user can load a custom palette via the palette editor
    /// and re-import.
    /// </summary>
    private static byte NearestPaletteIndex(Rgba32 px)
    {
        byte bestIdx = 0;
        int bestDist = int.MaxValue;

        foreach (var kvp in NaplpsState.ColorMapDefaults)
        {
            var col = kvp.Value;
            int dr = col.Red - px.R;
            int dg = col.Green - px.G;
            int db = col.Blue - px.B;
            int dist = dr * dr + dg * dg + db * db;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIdx = kvp.Key;
            }
        }

        return bestIdx;
    }
}
