namespace NAPLPS.Commands;

public class DeleteCommand : NaplpsCommand
{
    public DeleteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
