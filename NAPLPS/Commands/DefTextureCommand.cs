// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// This command is used to define one of the four programmable texture masks described in 5.3.2.4.
/// 
/// Bits b7 through bi of the character following this control shall be one of the
/// following bit combinations: (4/1), (4/2), (4/3), (4/4), that causes mask A, B, C,
/// or D, respectively, to be defined. Any existing texture pattern associated with
/// the specified mask is deleted. The mask is cleared by terminating the
/// command at this point. If presentation layer code follows, it describes the
/// texture mask in the same manner as DRCS characters, except that the texture
/// mask size is used rather than the character field size. The DEF TEXTURE
/// command is terminated when an END, DEF MACRO, DEFP MACRO, DEFT
/// MACRO, DEF DRCS, or another DEF TEXTURE command is received. If bits
/// b7 to bi of the character following the DEF TEXTURE control are not in the
/// range 4/ I to 4/4, the entire command (ie, the CI control and the out of range
/// character) is in error and is executed as a null operation. At the end of the
/// DEF TEXTURE command, the receiving device reverts to the normal
/// procedure of mapping the unit screen to the physical display screen, with the
/// drawing point reset to (0,0).
/// 
/// Note: The INCREMENTAL POINT command may scale the actual active field
/// before execution (see 5.3.3.6.3), causing the actual area defined to be smaller
/// than requested.
/// </summary>
public class DefTextureCommand : EscCommand
{
    public ushort MaskId { get; }

    public DefTextureCommand(NaplpsState state, NaplpsOperands operands) : base(state, operands)
    {
        if (Operands.Count != 2 && (NaplpsEscapeCommands)Operands[0] != NaplpsEscapeCommands.DEF_TEXTURE)
        {
            throw new ArgumentOutOfRangeException(nameof(operands));
        }

        if (Operands[1] == 0x41)
        {
            MaskId = 0;
        }
        else if (Operands[1] == 0x42)
        {
            MaskId = 1;
        }
        else if (Operands[1] == 0x43)
        {
            MaskId = 2;
        }
        else if (Operands[1] == 0x44)
        {
            MaskId = 3;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(operands));
        }
    }
}