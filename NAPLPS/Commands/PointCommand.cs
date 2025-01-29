// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class PointCommand : FillableGeometricDrawingCommandBase
{
    public bool IsVisible { get; internal set; }

    public PointCommand(bool isRelative, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var vertices = ProcessVertices(operands);

        foreach (var point in vertices)
        {
            if (!isRelative)
            {
                SetPen(point);
            }
            else
            {
                SetPen(State.Pen + point);
            }
        }

        // IsVisible = opcode == POINT_ABS || opcode == POINT_REL;
    }
}
