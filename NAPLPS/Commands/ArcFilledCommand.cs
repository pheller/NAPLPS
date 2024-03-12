// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The start point is the current drawing point, the
/// intermediate point is the first block of coordinate data, specified as a relative
/// displacement from the start point, and the end point is the second block of
/// coordinate data, specified as a relative displacement from the intermediate
/// point.The start and end points are joined by a chord and the resulting figure
/// is filled in the current color(s) with the current texture pattern.
/// </summary>
public class ArcFilledCommand : ArcCommand
{
    public ArcFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_FILLED, operands)
    {
        ShouldFill = true;
    }
}