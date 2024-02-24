// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Diagnostics;

namespace NAPLPS.Commands;

public class TextureCommand : NaplpsCommand
{
    public TexturePatterns TexturePattern { get; }

    public LineTextures LineTexture { get; }

    public bool ShouldHighlight { get; }

    public TextureCommand(byte opcode, List<byte> operands) : base(opcode, operands)
    {
        TexturePattern = (TexturePatterns)ConvertBitsToByte(operands[0].GetBit(6), operands[0].GetBit(5), operands[0].GetBit(4));

        ShouldHighlight = operands[0].GetBit(3);

        LineTexture = (LineTextures)ConvertBitsToByte(operands[0].GetBit(2), operands[0].GetBit(1));
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