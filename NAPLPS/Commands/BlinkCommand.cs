namespace NAPLPS.Commands;

internal class BlinkCommand : NaplpsCommand
{
    public BlinkCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
