// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(160, "Control", "C0 or C1 control code. See the Command property for the specific function.", Category = CommandCategory.Control)]
public class ControlCommand : NaplpsCommand
{
    public const byte EscapeC0Set = 0x21;
    public const byte EscapeC1Set = 0x22;

    public NaplpsControlCommands Command { get; }

    public ControlCommand(NaplpsControlCommands command, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        Command = command;

        if (Command == Escape)
        {
            if (operands.Count < 2)
            {
                IsValid = false;
            }
            else if ((NaplpsControlCommands)operands[0] == Repeat && operands.Count == 2)
            {
                IsValid = false;
            }
        }
    }

    public override string ToString()
    {
        return $"{nameof(ControlCommand)}({Command})";
    }
}
