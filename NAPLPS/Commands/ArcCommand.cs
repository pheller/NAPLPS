// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class ArcCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; internal set; }

    public Vector3 IntermediatePointDisplacement { get; internal set; }

    public Vector3 EndPointDisplacement { get; internal set; }

    public ArcCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Vertices = ProcessVerticies(operands);

        if ((OpCode == ARC_OUTLINED || OpCode == ARC_FILLED) && operands.Count == State.MultiByteValue * 2)
        {
            IntermediatePointDisplacement = Vertices[0];
            EndPointDisplacement = Vertices[1];
        }
        else if (operands.Count == State.MultiByteValue * 3)
        {
            StartPoint = Vertices[0];
            IntermediatePointDisplacement = Vertices[1];
            EndPointDisplacement = Vertices[2];
        }
        else
        {
            IsValid = false;
        }
    }

}
