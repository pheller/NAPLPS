// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class RectangleSetCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; }

    public Vector3 Dimensions { get; }

    public RectangleSetCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var verts = ProcessVertices(Operands);

        StartPoint = verts[0];

        SetPen(StartPoint);

        if (verts.Count >= 2)
        {
            Dimensions = verts[1];

            MovePen(new Vector3(Dimensions.X, 0, 0));
        }
        else
        {
            IsValid = false;
        }
    }
}