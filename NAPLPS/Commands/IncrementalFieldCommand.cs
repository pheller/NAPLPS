// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class IncrementalFieldCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType =  NaplpsOperandType.MultiValue;

    public Vector3 Origin { get; }

    public Vector3 Dimensions { get; }

    public IncrementalFieldCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var vertices = ProcessVertices(Operands);

        if (operands.Count == 0)
        {
            return;
        }

        if (vertices.Count == 1)
        {
            Origin = State.Pen;
            Dimensions = vertices[0];
        }
        else
        {
            Origin = vertices[0];
            Dimensions = vertices[1];
        }

        state.Field = new NaplpsField(Origin, Dimensions);
    }
}
