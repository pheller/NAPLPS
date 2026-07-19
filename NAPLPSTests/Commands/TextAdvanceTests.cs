// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Drawing;

namespace NAPLPSTests.Commands;

/// <summary>
/// Pen-advance tests for rotated text paths. Proportional spacing must advance by the per-glyph
/// proportional metric along the path; TAX2's rotated "Form" (90 degrees CCW, path Up,
/// proportional) pitches at ~7 device rows per glyph, not the 14+ of a char-cell-height advance.
/// </summary>
[TestClass]
public class TextAdvanceTests
{
    [TestMethod]
    public void VerticalProportionalText_AdvancesByProportionalMetric()
    {
        var bytes = System.IO.File.ReadAllBytes("examples/Anthony Wetzel/TAX2");
        var fmt = NaplpsFormat.FromBytes(bytes, NaplpsSystemType.Prodigy);
        using var ctx = new DrawContext(fmt, new SixLabors.ImageSharp.Size(640, 480));
        ctx.Render();

        // The rotated "Form" column at the screen's top-left: collect dark-ink row clusters above
        // the header box line. Four glyphs, successive pitch ~7 rows (proportional); the fixed
        // char-height advance bug spread them ~14 rows apart and pushed ink off the band.
        var clusters = new List<(int Top, int Bottom)>();
        (int Top, int Bottom)? cur = null;
        for (var y = 5; y < 60; y++)
        {
            var ink = 0;
            for (var x = 0; x < 40; x++)
            {
                var p = ctx.Image[x, y];
                if (p.R + p.G + p.B < 200) { ink++; }
            }

            if (ink > 0) { cur = cur is null ? (y, y) : (cur.Value.Top, y); }
            else if (cur is not null) { clusters.Add(cur.Value); cur = null; }
        }
        if (cur is not null) { clusters.Add(cur.Value); }

        Assert.AreEqual(4, clusters.Count, $"expected 4 rotated glyphs, got {clusters.Count}");
        for (var i = 0; i + 1 < clusters.Count; i++)
        {
            var pitch = clusters[i + 1].Top - clusters[i].Top;
            Assert.IsTrue(pitch >= 5 && pitch <= 9,
                $"glyph pitch {pitch} rows between clusters {i} and {i + 1}; proportional is ~7, the fixed-advance bug gives ~14");
        }
    }
}
