// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Drawing;

namespace NAPLPS;

public class NaplpsState
{
    /* Parsing States */

    /// <summary>Future Spec [2 = x,y,0 3 = x,y,z]</summary>
    /// <figure>11</figure>
    public ushort Dimensionality { get; set; } = 2;

    public ushort MultiByteValue { get; set; } = 3;

    public ushort SingleByteValue { get; set; } = 1;

    /* Drawing States */
    public Point LogicalPel { get; set; } = Point.Empty;
}
