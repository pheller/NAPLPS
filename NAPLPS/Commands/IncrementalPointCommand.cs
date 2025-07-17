namespace NAPLPS.Commands;

public class IncrementalPointCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType =  NaplpsOperandType.FixedAndString;

    public IncrementalPointCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
