namespace NAPLPS.Commands;

public class IncrementalPointCommand : NaplpsCommand
{
    public IncrementalPointCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
