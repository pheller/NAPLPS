// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class RectangleSetOutlinedCommand : RectangleSetCommand
{
    public RectangleSetOutlinedCommand(List<byte> operands) : base(RECTANGLE_SET_OUTLINED, operands)
    {
        ShouldOutline = true;
    }
}