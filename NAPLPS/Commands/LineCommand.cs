// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class LineCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValue;

    public LineCommand(bool isSet, bool isRelative, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var verticies = ProcessVertices(operands);

        if (verticies.Count == 0 || operands.Count == 0)
        {
            State.RecordError(NaplpsErrorSeverity.Warning, NaplpsErrorType.InvalidCommand, "Line command received with no vertices or operands", opcode);

            return;
        }

        if (isSet && isRelative)
        {
            for (int i = 0; i < verticies.Count; ++i)
            {
                var vert = verticies[i];

                if (i % 2 == 0)
                {
                    SetPen(vert);
                }
                else
                {
                    MovePen(vert);
                }
            }
        }
        else if (isSet && !isRelative)
        {
            foreach (var vert in verticies)
            {
                SetPen(vert);
            }
        }
        else
        {
            SetPen(State.Pen);

            foreach (var vert in verticies)
            {
                if (!isRelative)
                {
                    SetPen(vert);
                }
                else
                {
                    MovePen(vert);
                }
            }
        }
    }
}