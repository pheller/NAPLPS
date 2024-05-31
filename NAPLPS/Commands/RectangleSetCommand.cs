// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS.Commands;

public abstract class RectangleSetCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; }

    public Vector3 Dimensions { get; }

    public RectangleSetCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (Operands.Count == 0)
        {
            IsValid = false;

            return;
        }

        Vertices = ProcessVertices(Operands);

        StartPoint = Vertices[0];

        if (StartPoint == Vector3.Zero)
        {
            // if (Debugger.IsAttached) Debugger.Break();
        }

        SetPen(StartPoint);

        if (Vertices.Count >= 2)
        {
            Dimensions = Vertices[1];

            MovePen(new Vector3(Dimensions.X, 0, 0));
        }
        else
        {
            IsValid = false;
        }
    }
}