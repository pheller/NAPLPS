// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class NaplpsCommand(NaplpsState? state, byte opcode, NaplpsOperands? operands)
{
    public static NaplpsOperandType OperandType = NaplpsOperandType.None;

    public static int OperandCount => 0;

    public byte OpCode { get; } = opcode;

    public NaplpsOperands Operands { get; } = operands ?? [];

    public bool IsValid { get; internal set; } = true;

    public NaplpsState State { get; } = state ?? new NaplpsState();

    public override string ToString()
    {
        return GetType().Name;
    }
}