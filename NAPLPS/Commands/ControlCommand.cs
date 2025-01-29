namespace NAPLPS.Commands;

internal class ControlCommand : NaplpsCommand
{
    public NaplpsControlCommands Command { get; }

    public ControlCommand(NaplpsControlCommands command, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Command = command;
    }

    public override string ToString()
    {
        return $"{nameof(ControlCommand)}({Command})";
    }
}
