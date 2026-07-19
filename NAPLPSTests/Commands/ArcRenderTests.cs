// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Drawing;

namespace NAPLPSTests.Commands;

/// <summary>
/// Render tests for arc/circle outlines. Authentic (hard-pel) circles must honor the current
/// line texture with the same dashed-pel plot the arc path uses, instead of always plotting solid.
/// </summary>
[TestClass]
public class ArcRenderTests
{
    // POINT SET ABS to (0.3, 0.5), TEXTURE with the given line texture, then an outlined arc
    // whose displacements cancel (mid +0.25, end -0.25) so start == end: a circle of diameter
    // 0.25. The displacements must be exactly representable in the 9-bit coordinate encoding or
    // quantization breaks the start == end circle classification.
    private static byte[] CircleStream(byte lineTexture)
    {
        var bytes = new List<byte> { 0xA4 };
        bytes.AddRange(NaplpsEncoder.EncodeVertex2D(0.3f, 0.5f));
        bytes.Add(0xA3);
        bytes.Add(NaplpsEncoder.EncodeTextureFixedByte(lineTexture, false, 0));
        bytes.Add(0xAC);
        bytes.AddRange(NaplpsEncoder.EncodeVertex2D(0.25f, 0f));
        bytes.AddRange(NaplpsEncoder.EncodeVertex2D(-0.25f, 0f));
        return bytes.ToArray();
    }

    private static int RenderInkCount(byte[] bytes)
    {
        // Prodigy system type turns on the authentic hard-pel geometry path.
        var fmt = NaplpsFormat.FromBytes(bytes, NaplpsSystemType.Prodigy);
        using var ctx = new DrawContext(fmt, new SixLabors.ImageSharp.Size(640, 480));
        ctx.Render();

        var ink = 0;
        for (var y = 0; y < ctx.Image.Height; y++)
        {
            for (var x = 0; x < ctx.Image.Width; x++)
            {
                var p = ctx.Image[x, y];
                if (p.R > 32 || p.G > 32 || p.B > 32)
                {
                    ink++;
                }
            }
        }

        return ink;
    }

    [TestMethod]
    public void AuthenticCircle_DottedTexture_DrawsGaps()
    {
        var solid = RenderInkCount(CircleStream((byte)NaplpsTexture.LineTextures.Solid));
        var dotted = RenderInkCount(CircleStream((byte)NaplpsTexture.LineTextures.Dotted));

        Assert.IsTrue(solid > 100, $"solid ring should be a real circle (ink={solid})");
        Assert.IsTrue(dotted > 0, $"dotted ring should still draw (ink={dotted})");

        // Dotted is 1 pel on / 1 pel off, so roughly half the solid ring's pixels. A circle that
        // ignores the texture renders identical to solid and fails the upper bound.
        Assert.IsTrue(dotted < solid * 3 / 4, $"dotted ring should have gaps (solid={solid} dotted={dotted})");
        Assert.IsTrue(dotted > solid / 4, $"dotted ring should keep its dots (solid={solid} dotted={dotted})");
    }
}
