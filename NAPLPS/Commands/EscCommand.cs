// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// This character (0x1B) is used for code extension (see 4.3.2 and 4.3.3).
/// </summary>
public class EscCommand : NaplpsCommand
{
    public EscCommand(NaplpsState state, NaplpsOperands operands) : base(state, ESC, operands)
    {
        if (Operands.Count == 2 && Operands[1] == (byte)NaplpsGSet.PDISet)
        {
            switch ((NaplpsEscapeCommands)Operands[0])
            {
                case NaplpsEscapeCommands.G1:
                case NaplpsEscapeCommands.G1D:
                {
                    State.G1 = NaplpsGSet.PDISet;
                }
                break;

                case NaplpsEscapeCommands.G2:
                case NaplpsEscapeCommands.G2D:
                {
                    State.G2 = NaplpsGSet.PDISet;
                }
                break;

                case NaplpsEscapeCommands.G3:
                case NaplpsEscapeCommands.G3D:
                {
                    State.G3 = NaplpsGSet.PDISet;
                }
                break;
            }
        }
    }
}