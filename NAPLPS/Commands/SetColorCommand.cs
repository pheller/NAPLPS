// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

/// <summary>
/// </summary>
public class SetColorCommand : NaplpsCommand
{
    public ushort ColorMode { get; }

    public bool IsTransparent { get; }

    public ushort Green { get; private set; }

    public ushort Red { get; private set; }

    public ushort Blue { get; private set; }

    public SetColorCommand(List<byte> operands) : base(SET_COLOR, operands)
    {
        if (ColorMode == 0 && operands.Count == 0)
        {
            IsTransparent = true;
        }
        else if (ColorMode == 0)
        {
            ParseColorComponents(operands);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void ParseColorComponents(List<byte> operands)
    {
        Green = 0;
        Red = 0;
        Blue = 0;

        foreach (var b in operands)
        {
            // Extract bits for each color component from the first triplet
            Green = (ushort)(Green << 1 | b >> 5 & 0x1);
            Red = (ushort)(Red << 1 | b >> 4 & 0x1);
            Blue = (ushort)(Blue << 1 | b >> 3 & 0x1);

            // Extract bits for each color component from the second triplet
            Green = (ushort)(Green << 1 | b >> 2 & 0x1);
            Red = (ushort)(Red << 1 | b >> 1 & 0x1);
            Blue = (ushort)(Blue << 1 | b & 0x1);
        }
    }
}