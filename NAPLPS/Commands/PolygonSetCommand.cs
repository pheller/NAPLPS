// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public abstract class PolygonSetCommand : FillableGeometricDrawingCommandBase
{
    public Vector3 StartPoint { get; }

    public PolygonSetCommand(NaplpsState state, NaplpsCommands opcode, List<byte> operands) : base(state, opcode, operands)
    {
        StartPoint = ProcessVerticies(operands.Take(State.MultiByteValue).ToList()).FirstOrDefault();

        Vertices = ProcessVerticies(operands.Skip(State.MultiByteValue).ToList());
    }
}