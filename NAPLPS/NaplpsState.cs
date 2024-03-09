// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Drawing;
using System.Text.Json;

namespace NAPLPS;

public class NaplpsState
{
    public static JsonSerializerOptions GlobalJsonSerializerOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /* State Recording Utilities */

    public string ToJson() => JsonSerializer.Serialize(this, GlobalJsonSerializerOptions);

    public static NaplpsState FromJson(string json) => JsonSerializer.Deserialize<NaplpsState>(json, GlobalJsonSerializerOptions) ?? new();

    /* Parsing States */

    /// <summary>Future Spec [2 = x,y,0 3 = x,y,z]</summary>
    /// <figure>11</figure>
    public byte Dimensionality { get; set; } = 2;

    public byte MultiByteValue { get; set; } = 3;

    public byte SingleByteValue { get; set; } = 1;

    /* Drawing States */

    public Point LogicalPel { get; set; } = new(1, 1);

    public PointF Pen { get; set; } = new();

    /* Color States */

    public byte ColorMode { get; set; }

    public Dictionary<byte, NaplpsColor> ColorMap { get; set; } = new(ColorMapDefaults);

    public byte ColorMapForegroundSelected { get; set; }

    public byte ColorMapBackgroundSelected { get; set; }

    public bool IsTransparent { get; set; }

    public NaplpsColor Foreground { get; set; } = new();

    public NaplpsColor Background { get; set; } = new();

    public override string ToString()
    {
        return $"D:{Dimensionality} M:{MultiByteValue} S:{SingleByteValue}-P:(<{Pen.X},{Pen.Y}>({LogicalPel.X},{LogicalPel.Y}))-C:{ColorMode} CMF:{ColorMapForegroundSelected} CMB:{ColorMapBackgroundSelected}, CF:{Foreground} CB:{Background}";
    }

    /* Defaults */
    public static readonly Dictionary<byte, NaplpsColor> ColorMapDefaults = new()
    {
        {0x0, NaplpsColor.From3BitGRB(0, 0, 0)},
        {0x1, NaplpsColor.From3BitGRB(1, 1, 1)},
        {0x2, NaplpsColor.From3BitGRB(2, 2, 2)},
        {0x3, NaplpsColor.From3BitGRB(3, 3, 3)},
        {0x4, NaplpsColor.From3BitGRB(4, 4, 4)},
        {0x5, NaplpsColor.From3BitGRB(5, 5, 5)},
        {0x6, NaplpsColor.From3BitGRB(6, 6, 6)},
        {0x7, NaplpsColor.From3BitGRB(7, 7, 7)},
        {0x8, NaplpsColor.From3BitGRB(0, 0, 7)},
        {0x9, NaplpsColor.From3BitGRB(0, 5, 7)},
        {0xA, NaplpsColor.From3BitGRB(0, 7, 4)},
        {0xB, NaplpsColor.From3BitGRB(2, 7, 0)},
        {0xC, NaplpsColor.From3BitGRB(7, 7, 0)},
        {0xD, NaplpsColor.From3BitGRB(7, 2, 0)},
        {0xE, NaplpsColor.From3BitGRB(7, 0, 4)},
        {0xF, NaplpsColor.From3BitGRB(5, 0, 7)},
    };
}
