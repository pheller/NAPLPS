// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;

namespace NAPLPS.Commands;

public abstract class FillableGeometricDrawingCommandBase : GeometricDrawingCommandBase
{
    public bool ShouldFill { get; internal set; }

    public bool ShouldOutline { get; internal set; }

    public FillableGeometricDrawingCommandBase(byte opcode, List<byte> operands) : base(opcode, operands) { }
}