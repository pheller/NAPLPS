// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class LineCommand : GeometricDrawingCommandBase
{
    public Vector3 Point { get; internal set; }

    public LineCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Point = ProcessVerticies(operands).FirstOrDefault();

        Points.Add(Point);

        if (opcode == LINE_SET_ABS || opcode == LINE_SET_REL)
        {
            State.SetPen(Point);
        }
        else
        {
            State.MovePen(Point);
        }
    }
}