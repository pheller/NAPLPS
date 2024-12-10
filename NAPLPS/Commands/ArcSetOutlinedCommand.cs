// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The start point is the first block of coordinate data, specified in absolute coordinates.
/// The intermediate point is the second block of coordinate data, specified as a relative 
/// displacement from the start point, and the end point is the third block of coordinate data,
/// specified as a relative displacement from the intermediate point. The arc is not filled.
/// </summary>
public class ArcSetOutlinedCommand : ArcCommand
{
    public ArcSetOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_SET_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}