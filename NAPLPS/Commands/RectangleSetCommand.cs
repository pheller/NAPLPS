// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class RectangleSetCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; }

    public Vector3 Dimensions { get; }

    public RectangleSetCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var verts = ProcessVerticies(operands);

        StartPoint = verts[0];

        State.SetPen(StartPoint);

        Dimensions = verts[1];

        State.MovePen(Dimensions); // TODO: Verify
    }
}