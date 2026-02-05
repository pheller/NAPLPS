// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

/// <summary>
/// Represents a blink animation process that cycles a palette entry between colors.
/// </summary>
public class BlinkProcess
{
    /// <summary>Palette entry being animated</summary>
    public byte PaletteEntry { get; set; }

    /// <summary>Color to blink from (the "on" color)</summary>
    public NaplpsColor BlinkFromColor { get; set; }

    /// <summary>Color to blink to (the "off" color, typically black)</summary>
    public NaplpsColor BlinkToColor { get; set; }

    /// <summary>Duration of "on" phase in units (implementation dependent)</summary>
    public int OnInterval { get; set; }

    /// <summary>Duration of "off" phase in units</summary>
    public int OffInterval { get; set; }

    /// <summary>Initial delay before blinking starts</summary>
    public int StartDelay { get; set; }

    /// <summary>Number of cycles (0 = infinite)</summary>
    public int CycleCount { get; set; }

    /// <summary>Phase: 0 to 7 for gradual transitions</summary>
    public int Phase { get; set; }

    /// <summary>Current state of the blink (true = showing BlinkFromColor)</summary>
    public bool IsOn { get; set; } = true;

    /// <summary>Time elapsed in current phase</summary>
    public int ElapsedTime { get; set; }
}
