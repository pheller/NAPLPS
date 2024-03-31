// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class PolygonCommand : FillableGeometricDrawingCommandBase
{
    public PolygonCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Vertices = ProcessVerticies(operands);

        foreach (var vert in Vertices)
        {
            State.MovePen(vert);
        }
    }
}