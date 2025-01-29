namespace NAPLPS.Commands;

public class IncrementalLineCommand : NaplpsCommand
{
    public IncrementalLineCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
