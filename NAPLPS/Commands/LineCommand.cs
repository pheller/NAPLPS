// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS.Commands;

public abstract class LineCommand : GeometricDrawingCommandBase
{
    public LineCommand(NaplpsState state, NaplpsCommands opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        var verticies = ProcessVertices(operands);

        if (verticies.Count == 0 || operands.Count == 0)
        {
            Debugger.Break();

            return;
        }

        if (opcode == LINE_SET_ABS || opcode == LINE_SET_REL)
        {
            SetPen(verticies.First());

            foreach (var vert in verticies.Skip(1))
            {
                if (opcode == LINE_SET_ABS)
                {
                    SetPen(vert);
                }
                else 
                {
                    MovePen(vert);
                }
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