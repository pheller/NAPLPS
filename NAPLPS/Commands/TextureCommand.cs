// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class TextureCommand(NaplpsState state, NaplpsOperands operands) : NaplpsCommand(state, TEXTURE, operands)
{
    public TexturePatterns TexturePattern { get; } = (TexturePatterns)(operands.Count != 0 ? ConvertBitsToByte([operands[0, 6], operands[0, 5], operands[0, 4]]) : 0);

    public LineTextures LineTexture { get; } = (LineTextures)(operands.Count != 0 ? ConvertBitsToByte([operands[0, 2], operands[0, 1]]) : 0);

    public bool ShouldHighlight { get; } = operands.Count != 0 && operands[0, 3];

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