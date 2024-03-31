// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The start point is the first block of coordinate data, specified in absolute coordinates.
/// The intermediate point is the second block of coordinate data, specified as a relative 
/// displacement from the start point, and the end point is the third block of coordinate data,
/// specified as a relative displacement from the intermediate point. The start and end points
/// are joined by a chord and the resulting figure is filled in the current color(s) with 
/// the current texture pattern.
/// </summary>
public class ArcSetFilledCommand : ArcCommand
{
    public ArcSetFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}