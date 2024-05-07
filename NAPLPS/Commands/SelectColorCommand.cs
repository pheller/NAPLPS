// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NAPLPS.Commands;

/// <summary>
/// The SELECT COLOR opcode is used to establish the color mode as
/// well as select the drawing color for modes 1 and 2, and the background color
/// for mode 2.
/// </summary>
public class SelectColorCommand : NaplpsCommand
{
    /// <summary>
    /// The SELECT COLOR opcode can take zero, one, or two single-value operands.
    /// Additional numeric data bytes are reserved for future standardization and
    /// shall be ignored.
    /// </summary>
    public SelectColorCommand(NaplpsState state, NaplpsOperands operands) : base(state, SELECT_COLOR, operands)
    {
        var actualSingleValues = Operands.Count / State.SingleByteValue;

        /// If the SELECT COLOR opcode is followed by no operands,
        /// color mode 0 is indicated. The terminal will remain in color mode 0 until
        /// either another SELECT COLOR command with operands is received or the
        /// color mode is changed with a RESET command. While in color mode 0, the
        /// SET COLOR command is used to set the drawing color, as described in the
        /// previous section, and the SELECT COLOR command is not  used. A background
        /// color is not specified in color mode 0, rather, alphanumerics
        /// and pictorial drawings merely overwrite the existing contents of the
        /// physical display screen only where the drawing color is applied.
        State.ColorMode = (byte)Math.Min(2, actualSingleValues);

        /// If the SELECT COLOR opcode is followed by a single operand,
        /// color mode 1 is indicated. (This has no effect on the color map.) The terminal
        /// will remain in color mode 1 until either another SELECT COLOR command
        /// with 0 or 2 operands is received or the color mode is changed with a RESET or
        /// NSR command. While in color mode 1, the single operand following the
        /// SELECT COLOR opcode is used to set the drawing color that is applied to
        /// subsequently received alphanumeric text and pictorial information. Note,
        /// again, that the drawing color in this case is an ordinal number that represents
        /// an address in the color map in which the actual color value was previously, or
        /// will later be, loaded with a SET COLOR command. A background color is not
        /// specified in color mode 1, rather, alphanumerics and pictorial drawings merely
        /// overwrite the existing contents of the physical display area only where the
        /// drawing color is applied.
        if (actualSingleValues == 1)
        {
            if (State.SingleByteValue == 1)
            {
                State.ColorMapForeground = ConvertBitsToByte([Operands[0, 3], Operands[0, 4], Operands[0, 5], Operands[0, 6]]);
            }
            else if (State.SingleByteValue == 2)
            {
                State.ColorMapForeground = ConvertBitsToByte([Operands[0, 3], Operands[0, 4], Operands[0, 5], Operands[0, 6]]);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// If the SELECT COLOR opcode is followed by two operands, color
        /// mode 2 is indicated. Again, the terminal will remain in color mode 2 until
        /// either another SELECT COLOR command with 0 or 1 operand is received or
        /// the color mode is changed with a RESET or NSR command. While in color
        /// mode 2, the first operand following the SELECT COLOR opcode is used to set
        /// the drawing color and the second operand is used to set the background color.
        /// Characters received while in color mode 2 will be drawn in the drawing color
        /// over the background color, which occupies the remainder of the character
        /// field. That part of the intercharacter spacing which is not part of the
        /// character field is not affected by the background color. For the special case
        /// in which the two operands are identical, ie, the drawing color is specified to be
        /// the same as the background color, the drawing color is, instead, left at its
        /// current value and only the background color is changed to the value specified.
        /// The background color also applies to the highlight as well as the alternating
        /// color in the line and area texture patterns
        else if (actualSingleValues == 2)
        {
            if (State.SingleByteValue == 1)
            {
                State.ColorMapForeground = ConvertBitsToByte([Operands[0, 3], Operands[0, 4], Operands[0, 5], Operands[0, 6]]);
                State.ColorMapBackground = ConvertBitsToByte([Operands[1, 3], Operands[1, 4], Operands[1, 5], Operands[1, 6]]);
            }
            else if (State.SingleByteValue == 2)
            {
                State.ColorMapForeground = ConvertBitsToByte([Operands[0, 3], Operands[0, 4], Operands[0, 5], Operands[0, 6]]);
                State.ColorMapBackground = ConvertBitsToByte([Operands[2, 3], Operands[2, 4], Operands[2, 5], Operands[2, 6]]);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}