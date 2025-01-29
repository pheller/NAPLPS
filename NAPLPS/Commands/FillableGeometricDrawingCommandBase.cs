// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public abstract class FillableGeometricDrawingCommandBase(NaplpsState state, byte opcode, NaplpsOperands operands) : GeometricDrawingCommandBase(state, opcode, operands)
{
    public bool ShouldFill { get; internal set; }
}