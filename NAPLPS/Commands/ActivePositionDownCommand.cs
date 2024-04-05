namespace NAPLPS.Commands;

public class ActivePositionDownCommand : NaplpsCommand
{
    public ActivePositionDownCommand(NaplpsState state, NaplpsOperands operands) : base(state, AP_DOWN, operands)
    {
        // State.MovePen(State.TextInterrowSpacing)
    }
}
