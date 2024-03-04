// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class RectangleSetOutlinedCommand : RectangleSetCommand
{
    public RectangleSetOutlinedCommand(NaplpsState state, List<byte> operands) : base(state, RECTANGLE_SET_OUTLINED, operands)
    {
        ShouldOutline = true;
    }
}