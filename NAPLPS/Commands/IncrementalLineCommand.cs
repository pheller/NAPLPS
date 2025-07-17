namespace NAPLPS.Commands;

public class IncrementalLineCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType =  NaplpsOperandType.MultiValueAndString;

    public IncrementalLineCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
