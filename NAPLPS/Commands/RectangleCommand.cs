// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class RectangleCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 Dimensions { get; }

    public RectangleCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Dimensions = ProcessVertices(operands).FirstOrDefault();

        SetPen(State.Pen);

        MovePen(new Vector3(Dimensions.X, 0, 0));
    }
}