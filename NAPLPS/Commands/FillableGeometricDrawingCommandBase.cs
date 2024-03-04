// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

public abstract class FillableGeometricDrawingCommandBase(NaplpsState state, NaplpsCommands opcode, List<byte> operands) : GeometricDrawingCommandBase(state, opcode, operands)
{
    public bool ShouldFill { get; internal set; }

    public bool ShouldOutline { get; internal set; }
}