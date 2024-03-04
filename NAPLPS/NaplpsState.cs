// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System;
using System.Drawing;

namespace NAPLPS;

public class NaplpsState
{
    public NaplpsState ShallowCopy() => (NaplpsState)this.MemberwiseClone();

    /* Parsing States */

    /// <summary>Future Spec [2 = x,y,0 3 = x,y,z]</summary>
    /// <figure>11</figure>
    public byte Dimensionality { get; set; } = 2;

    public byte MultiByteValue { get; set; } = 3;

    public byte SingleByteValue { get; set; } = 1;

    /* Drawing States */

    public Point LogicalPel { get; set; } = new(1, 1);

    public byte ColorMode { get; set; }

    public Dictionary<byte, Color> ColorMap { get; set; } = new(ColorMapDefaults);

    public byte ForegroundSelectedColor { get; set; }

    public byte BackgroundSelectedColor { get; set; }

    public bool IsTransparent { get; set; }


    public byte DrawForgroundGreen { get; set; }

    public byte DrawForgroundRed { get; set; }

    public byte DrawForgroundBlue { get; set; }


    public byte DrawBackgroundGreen { get; set; }

    public byte DrawBackgroundRed { get; set; }

    public byte DrawBackgroundBlue { get; set; }

    /* Defaults */
    public static readonly Dictionary<byte, Color> ColorMapDefaults = new()
    {
        {0x0, Color.Empty.From3BitGRB(0, 0, 0)},
        {0x1, Color.Empty.From3BitGRB(1, 1, 1)},
        {0x2, Color.Empty.From3BitGRB(2, 2, 2)},
        {0x3, Color.Empty.From3BitGRB(3, 3, 3)},
        {0x4, Color.Empty.From3BitGRB(4, 4, 4)},
        {0x5, Color.Empty.From3BitGRB(5, 5, 5)},
        {0x6, Color.Empty.From3BitGRB(6, 6, 6)},
        {0x7, Color.Empty.From3BitGRB(7, 7, 7)},
        {0x8, Color.Empty.From3BitGRB(0, 0, 7)},
        {0x9, Color.Empty.From3BitGRB(0, 5, 7)},
        {0xA, Color.Empty.From3BitGRB(0, 7, 4)},
        {0xB, Color.Empty.From3BitGRB(2, 7, 0)},
        {0xC, Color.Empty.From3BitGRB(7, 7, 0)},
        {0xD, Color.Empty.From3BitGRB(7, 2, 0)},
        {0xE, Color.Empty.From3BitGRB(7, 0, 4)},
        {0xF, Color.Empty.From3BitGRB(5, 0, 7)},
    };
}
