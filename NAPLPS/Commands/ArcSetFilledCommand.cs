// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The start point is the first block of coordinate data, specified in absolute coordinates.
/// The intermediate point is the second block of coordinate data, specified as a relative 
/// displacement from the start point, and the end point is the third block of coordinate data,
/// specified as a relative displacement from the intermediate point. The start and end points
/// are joined by a chord and the resulting figure is filled in the current color(s) with 
/// the current texture pattern.
/// </summary>
[AddCommand(200, "Arc Set Filled", "Draw a filled (chorded) arc with absolute start, plus relative mid and end.", Category = CommandCategory.Geometric, DslKeyword = "arcSetFilled")]
public class ArcSetFilledCommand : ArcCommand
{
    public ArcSetFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(true, state, opcode, operands)
    {
        ShouldFill = true;
    }
}