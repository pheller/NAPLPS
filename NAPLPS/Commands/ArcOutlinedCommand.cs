// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The start point is the current drawing point, the
/// intermediate point is the first block of coordinate data, specified as a relative
/// displacement from the start point, and the end point is the second block of
/// coordinate data, specified as a relative displacement from the intermediate
/// point. The arc is not filled.
/// </summary>
public class ArcOutlinedCommand : ArcCommand
{
    public ArcOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, ARC_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}