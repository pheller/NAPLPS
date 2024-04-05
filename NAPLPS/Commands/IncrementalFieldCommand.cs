// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAPLPS.Commands;

public class IncrementalFieldCommand : GeometricDrawingCommandBase
{
    public Vector3 Origin { get; }

    public Vector3 Dimensions { get; }

    public IncrementalFieldCommand(NaplpsState state, NaplpsOperands operands) : base(state, INCREMENTAL_FIELD, operands)
    {
        var vertices = ProcessVertices(Operands);

        if (operands.Count == 0)
        {
            return;
        }

        Origin = vertices[0];

        Dimensions = vertices[1];
    }
}
