// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Drawing;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

/// <summary>
/// The DOMAIN command is used to control the precision of
/// single-value and multi-value operands, the dimensionality of
/// coordinate specifications, and the size of the logical pel.
///
/// Once set, these parameters do not change until acted upon by either the
/// RESET command, another DOMAIN command, or the NSR control code
/// </summary>
public class DomainCommand : GeometricDrawingCommandBase
{
    public DomainCommand(NaplpsState state, List<byte> operands) : base(state, DOMAIN, operands)
    {
        if (operands.Count == 0)
        {
            return;
        }

        State.SingleByteValue = ConvertBitsToByte([Operands[0].GetBit(1), Operands[0].GetBit(2)]);
        State.SingleByteValue++;

        State.MultiByteValue = ConvertBitsToByte([Operands[0].GetBit(3), Operands[0].GetBit(4), Operands[0].GetBit(5)]);
        State.MultiByteValue++;

        State.Dimensionality = (byte)(Operands[0].GetBit(6) ? 3 : 2);

        if (operands.Count > 1)
        {
            Vertices = ProcessVerticies(operands.Skip(1).ToList(), true);

            State.LogicalPel = Vertices != null ? new Point((int)Vertices[0].X, (int)Vertices[0].Y) : Point.Empty;
        }
    }
}