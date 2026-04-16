// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(120, "Delete", "DEL (0x7F/0xFF) — positional delete glyph.", Category = CommandCategory.Character, DslKeyword = "del")]
public class DeleteCommand : NaplpsCommand
{
    public DeleteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
