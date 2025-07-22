// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public struct NaplpsColor
{
    public static NaplpsColor Black { get; } = new(0, 0, 0);

    public static NaplpsColor White { get; } = new(byte.MaxValue, byte.MaxValue, byte.MaxValue);

    public byte Green { get; set; }

    public byte Red { get; set; }

    public byte Blue { get; set; }

    public NaplpsColor()
    {
    }

    public NaplpsColor(byte green, byte red, byte blue)
    {
        Green = green;
        Red = red;
        Blue = blue;
    }

    public readonly Color ToColor() => Color.FromArgb(Red, Green, Blue);

    public static NaplpsColor From3BitGRB(int green3Bit, int red3Bit, int blue3Bit)
    {
        // Ensure the input values are within the 3-bit range
        green3Bit = Math.Clamp(green3Bit, 0, 7);
        red3Bit = Math.Clamp(red3Bit, 0, 7);
        blue3Bit = Math.Clamp(blue3Bit, 0, 7);

        // Convert 3-bit values to 8-bit
        int green = green3Bit * 255 / 7;
        int red = red3Bit * 255 / 7;
        int blue = blue3Bit * 255 / 7;

        return new NaplpsColor((byte)green, (byte)red, (byte)blue);
    }

    public override string ToString()
    {
        return $"<{Green},{Red},{Blue}>";
    }
}
