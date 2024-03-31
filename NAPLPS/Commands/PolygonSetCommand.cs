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
            StartPoint = ProcessVerticies(operands[..State.MultiByteValue]).FirstOrDefault();

            Points.Add(StartPoint);
            State.SetPen(StartPoint);

            Vertices = ProcessVerticies(operands[State.MultiByteValue..]);

            foreach (var vert in Vertices)
            {
                Points.Add(Points.LastOrDefault() + vert);
                State.MovePen(vert);
            }
        }
    }
}