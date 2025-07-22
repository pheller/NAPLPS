// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public struct NaplpsTexture
{
    public TexturePatterns TexturePattern { get; set; }

    public bool ShouldHighlight { get; set; }

    public LineTextures LineTexture { get; set; }

    public Vector3 MaskSize { get; set; }

    public NaplpsTexture(
        TexturePatterns texturePattern,
        bool shouldHighlight,
        LineTextures lineTexture,
        Vector3 maskSize
    )
    {
        TexturePattern = texturePattern;
        ShouldHighlight = shouldHighlight;
        LineTexture = lineTexture;
        MaskSize = maskSize;
    }

    public enum LineTextures : byte
    {
        /// <summary>This is the default value</summary>
        Solid,
        Dotted,
        Dashed,
        DottedDashed
    }

    public enum TexturePatterns : byte
    {
        /// <summary>This is the default value</summary>
        Solid,
        VerticalHatching,
        HorizontalHatching,
        CrossHatching,
        MaskA,
        MaskB,
        MaskC,
        MaskD
    }
}

