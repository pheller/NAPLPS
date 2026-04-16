// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Line Relative", "Draw a line from the pen by a relative offset.", Category = CommandCategory.Geometric, DslKeyword = "lineRel")]
public class LineRelativeCommand : LineCommand
{
    public LineRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, true, state, opcode, operands)
    {

    }
}