// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class IncrementalFieldCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValue;

    public Vector3 Origin { get; }

    public Vector3 Dimensions { get; }

    public IncrementalFieldCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var vertices = ProcessVertices(Operands);

        if (operands.Count == 0)
        {
            // If no data bytes follow the FIELD opcode, the active field is set to the full unit
            // screen and the origin point is (0, 0).
            state.Field = new NaplpsField();

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

        // Position pen at the top of the field.
        // NAPLPS text convention: FIELD followed by CR+LF positions the cursor at the
        // first text row. The APD (line feed) will move pen down by CharSize.Y into the
        // field's top row, so we start at Origin.Y + Dimensions.Y (field top edge).
        var pen = Origin;
        pen.Y = Origin.Y + Dimensions.Y;
        state.Pen = pen;
    }
}
