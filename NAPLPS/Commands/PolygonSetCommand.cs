// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class PolygonSetCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; }

    public PolygonSetCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (operands.Count == 0)
        {
            IsValid = false;
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
        }
    }
}