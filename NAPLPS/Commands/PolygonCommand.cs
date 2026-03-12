// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class PolygonCommand : FillableGeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValue;

    public Vector3 StartPoint { get; private set; }

    public PolygonCommand(bool isSet, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (operands.Count == 0)
        {
            IsValid = false;
            return;
        }

        if (!isSet)
        {
            StartPoint = State.Pen;
            SetPen(State.Pen);

            Vertices = ProcessVertices(operands);

            foreach (var vert in Vertices)
            {
                MovePen(vert);
            }
        }
        else
        {
            StartPoint = ProcessVertices(operands[..State.MultiByteValue]).FirstOrDefault();

            SetPen(StartPoint);

            Vertices = ProcessVertices(operands[State.MultiByteValue..]);

            foreach (var vert in Vertices)
            {
                MovePen(vert);
            }

            SetPen(StartPoint);
        }
    }
}
