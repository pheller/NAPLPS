// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

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
    public DomainCommand(NaplpsState state, NaplpsOperands operands) : base(state, DOMAIN, operands)
    {
        if (Operands.Count == 0)
        {
            return;
        }

        State.SingleByteValue = ConvertBitsToByte([Operands[0, 1], Operands[0, 2]]);
        State.SingleByteValue++;

        State.MultiByteValue = ConvertBitsToByte([Operands[0, 3], Operands[0, 4], Operands[0, 5]]);
        State.MultiByteValue++;

        State.Dimensionality = (byte)(Operands[0, 6] ? 3 : 2);

        if (operands.Count > 1)
        {
            Vertices = ProcessVertices(Operands[1..], true);

            State.LogicalPel = Vertices != null ? new Point((int)Vertices[0].X, (int)Vertices[0].Y) : Point.Empty;
        }
    }
}