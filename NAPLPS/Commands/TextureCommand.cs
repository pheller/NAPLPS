// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using LineTextures = NAPLPS.NaplpsTexture.LineTextures;
using TexturePatterns = NAPLPS.NaplpsTexture.TexturePatterns;

namespace NAPLPS.Commands;

[AddCommand(220, "Texture", "Set line texture, fill pattern, highlight, and mask size for subsequent geometry.", Category = CommandCategory.Attribute, DslKeyword = "texture")]
public class TextureCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.FixedAndMultiValue;

    public TexturePatterns TexturePattern { get; }

    public bool ShouldHighlight { get; }

    public LineTextures LineTexture { get; }

    public Vector3 MaskSize { get; }

    public TextureCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        TexturePattern = (TexturePatterns)(operands.Count != 0 ? ConvertBitsToByte([operands[0, 4], operands[0, 5], operands[0, 6]]) : 0);
        ShouldHighlight = operands.Count != 0 && operands[0, 3];
        LineTexture = (LineTextures)(operands.Count != 0 ? ConvertBitsToByte([operands[0, 1], operands[0, 2]]) : 0);
        MaskSize = operands.Count > 1 ? ProcessVertices(operands[1..]).FirstOrDefault() : Vector3.Zero;

        state.Texture = new NaplpsTexture(TexturePattern, ShouldHighlight, LineTexture, MaskSize);
    }
}