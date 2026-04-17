// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Import;
using SixLabors.ImageSharp.Processing;

namespace NAPLPSTests.Import;

[TestClass]
public class ImporterTests
{
    [TestMethod]
    public void SvgImporter_SimpleLinePath_ProducesTelidraw()
    {
        var svg = @"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'>
            <path d='M 10 10 L 90 10 L 90 90 L 10 90 Z'/>
        </svg>";

        var td = SvgImporter.ToTelidraw(svg);

        Assert.IsTrue(td.Contains("move "), $"Expected a move statement, got:\n{td}");
        Assert.IsTrue(td.Contains("line "), $"Expected line statements, got:\n{td}");
        Assert.IsTrue(td.Contains("#coord fractions"), "Output should include coord mode directive");
    }

    [TestMethod]
    public void SvgImporter_MissingViewBox_FallsBackToDefaults()
    {
        var svg = @"<svg xmlns='http://www.w3.org/2000/svg'>
            <path d='M 0 0 L 50 50'/>
        </svg>";

        var td = SvgImporter.ToTelidraw(svg);

        Assert.IsTrue(td.Contains("line "), "Should still emit line commands with default viewBox");
    }

    [TestMethod]
    public void SvgImporter_UnsupportedBezier_ApproximatesAsLine()
    {
        // Cubic bezier should render as a line-to-endpoint approximation.
        var svg = @"<svg viewBox='0 0 100 100'><path d='M 10 10 C 20 20 40 40 90 90'/></svg>";
        var td = SvgImporter.ToTelidraw(svg);
        Assert.IsTrue(td.Contains("line "), "Bezier commands should fall back to line");
        Assert.IsTrue(td.Contains("approximated"), "Output should note the approximation");
    }

    [TestMethod]
    public void SvgImporter_FlipsYAxis()
    {
        // SVG y=0 is at top; NAPLPS y=0.75 is at top. An SVG point at (50, 0) should
        // map to NAPLPS (0.5, 0.75).
        var svg = @"<svg viewBox='0 0 100 100'><path d='M 50 0'/></svg>";
        var td = SvgImporter.ToTelidraw(svg);
        Assert.IsTrue(td.Contains("0.5") && td.Contains("0.75"),
                      $"Y-axis flip broken, got:\n{td}");
    }

    [TestMethod]
    public void BitmapImporter_SolidBlackImage_ProducesEmptyDocument()
    {
        // A fully-black image maps to palette index 0 which we skip (background).
        using var img = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(10, 10);
        // Default pixel values are (0,0,0,0) — treated as palette 0.

        var td = BitmapImporter.ConvertToTelidraw(img, 10, 10);
        Assert.IsFalse(td.Contains("rect-set "), "Black image should produce no rect-sets (background skipped)");
    }

    [TestMethod]
    public void BitmapImporter_WhiteImage_ProducesWhiteRects()
    {
        // Construct a 4x4 all-white image by setting pixels directly (avoids pulling in
        // the ImageSharp.Drawing package for the Fill extension).
        using var img = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(4, 4);
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                img[x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 255, 255);
            }
        }
        var td = BitmapImporter.ConvertToTelidraw(img, 4, 3);
        Assert.IsTrue(td.Contains("rect-set "), $"White image should produce rect-sets, got:\n{td}");
    }
}
