// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class TextureCommand : GeometricDrawingCommandBase
{
    public TexturePatterns TexturePattern { get; }

    public bool ShouldHighlight { get; }

    public LineTextures LineTexture { get; }

    public Vector3 MaskSize { get; }

    public TextureCommand(NaplpsState state, NaplpsOperands operands) : base(state, TEXTURE, operands)
    {
        TexturePattern = (TexturePatterns)(operands.Count != 0 ? ConvertBitsToByte([operands[0, 6], operands[0, 5], operands[0, 4]]) : 0);

        ShouldHighlight = operands.Count != 0 && operands[0, 3];

        LineTexture = (LineTextures)(operands.Count != 0 ? ConvertBitsToByte([operands[0, 2], operands[0, 1]]) : 0);

        MaskSize = ProcessVertices(operands[1..]).FirstOrDefault();
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