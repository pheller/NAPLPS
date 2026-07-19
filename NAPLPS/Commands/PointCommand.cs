// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class PointCommand : FillableGeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValue;

    public bool IsVisible { get; internal set; }

    // POINT (A6/A7) displays the logical pel at each vertex; POINT SET (A4/A5) only moves the
    // drawing point (invisible). Default visible; the SET variants pass false. Without this every
    // visible point was dropped - e.g. the bantam-doubleday-dell title, whose serif strokes are
    // POINT-stamped pels.
    public PointCommand(bool isRelative, NaplpsState state, byte opcode, NaplpsOperands operands)
        : this(isRelative, true, state, opcode, operands)
    {
    }

    public PointCommand(bool isRelative, bool isVisible, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        IsVisible = isVisible;

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

    }
}
