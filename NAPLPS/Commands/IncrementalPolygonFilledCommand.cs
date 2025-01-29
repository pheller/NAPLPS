namespace NAPLPS.Commands;

public class IncrementalPolygonFilledCommand : NaplpsCommand
{
    public IncrementalPolygonFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
