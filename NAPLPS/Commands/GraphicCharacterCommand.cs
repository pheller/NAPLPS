// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// </summary>
public class GraphicCharacterCommand(NaplpsState state, NaplpsOperands operands, char character) : EscCommand(state, operands)
{
    public char Character { get; } = character;
}