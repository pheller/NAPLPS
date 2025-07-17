// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class RectangleCommand : FillableGeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType =  NaplpsOperandType.MultiValue;

    public Vector3 Dimensions { get; }

    public RectangleCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var verts = ProcessVertices(Operands);

        Dimensions = verts.FirstOrDefault();

        SetPen(State.Pen);

        MovePen(new Vector3(Dimensions.X, 0, 0));
    }
}