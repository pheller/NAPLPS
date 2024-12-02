// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS.Commands;

public abstract class LineCommand : GeometricDrawingCommandBase
{
    public List<Vector3> RawVertices { get; internal set; }
    public LineCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var verticies = ProcessVertices(operands);

        RawVertices = verticies;

        if (verticies.Count == 0 || operands.Count == 0)
        {
            Debugger.Break();

            return;
        }

        if (opcode == LINE_SET_REL)
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
        else if (opcode == LINE_SET_ABS)
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
                if (opcode == LINE_ABS)
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