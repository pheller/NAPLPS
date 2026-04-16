// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The start point is the current drawing point, the
/// intermediate point is the first block of coordinate data, specified as a relative
/// displacement from the start point, and the end point is the second block of
/// coordinate data, specified as a relative displacement from the intermediate
/// point. The arc is not filled.
/// </summary>
[AddCommand(200, "Arc Outlined", "Draw an unfilled arc through start, mid and end points (relative).", Category = CommandCategory.Geometric, DslKeyword = "arcOutlined")]
public class ArcOutlinedCommand : ArcCommand
{
    public ArcOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, state, opcode, operands)
    {
        ShouldFill = false;
    }
}