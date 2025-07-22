// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class NaplpsCommand(NaplpsState? state, byte opcode, NaplpsOperands? operands)
{
    public byte OpCode { get; } = opcode;

    public static NaplpsOperandType OperandType = NaplpsOperandType.None;

    public static int OperandCount => 0;

    public NaplpsOperands Operands { get; } = operands ?? [];

    public bool IsValid { get; internal set; } = true;

    public NaplpsState State { get; } = state ?? new NaplpsState();

    private static NaplpsCommand BreakAndReturn(NaplpsState state, byte opcode, NaplpsOperands operands)
    {
        var newUnknownCommand = new NaplpsCommand(state, opcode, operands);

        System.Diagnostics.Debugger.Break();

        return newUnknownCommand;
    }

    public override string ToString()
    {
        return GetType().Name;
    }
}