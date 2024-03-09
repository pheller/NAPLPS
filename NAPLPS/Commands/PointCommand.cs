// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class PointCommand : GeometricDrawingCommandBase
{
    public Vector3 Point { get; internal set; }

    public PointCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state,opcode, operands)
    {
        Point = ProcessVerticies(operands).FirstOrDefault();
    }
}