// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using TexturePatterns = NAPLPS.NaplpsTexture.TexturePatterns;
using LineTextures = NAPLPS.NaplpsTexture.LineTextures;

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
        LineTexture = (LineTextures)(operands.Count != 0 ? ConvertBitsToByte([operands[0, 1], operands[0, 2]]) : 0);
        MaskSize = ProcessVertices(operands[1..]).FirstOrDefault();

        state.Texture = new NaplpsTexture(TexturePattern, ShouldHighlight, LineTexture, MaskSize);
    }
}