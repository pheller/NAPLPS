// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class TextureCommand(NaplpsState state, List<byte> operands) : NaplpsCommand(state, TEXTURE, operands)
{
    public TexturePatterns TexturePattern { get; } = (TexturePatterns)ConvertBitsToByte([operands[0].GetBit(6), operands[0].GetBit(5), operands[0].GetBit(4)]);

    public LineTextures LineTexture { get; } = (LineTextures)ConvertBitsToByte([operands[0].GetBit(2), operands[0].GetBit(1)]);

    public bool ShouldHighlight { get; } = operands[0].GetBit(3);

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