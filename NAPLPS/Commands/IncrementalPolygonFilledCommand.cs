namespace NAPLPS.Commands;

public class IncrementalPolygonFilledCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType =  NaplpsOperandType.MultiValueAndString;

    public IncrementalPolygonFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
    }
}
