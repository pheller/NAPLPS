// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

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

    public readonly Color ToColor()
    {
        if (Drawing.Drawable.Options.ColorGunWidth is int bits and > 0 and < 8)
        {
            return Color.FromArgb(QuantizeGun(Red, bits), QuantizeGun(Green, bits), QuantizeGun(Blue, bits));
        }

        return Color.FromArgb(Red, Green, Blue);
    }

    /// <summary>
    /// Reduces a gun value to its top <paramref name="bits"/> bits and expands back to
    /// 8 bits by bit replication (e.g. width 2 yields levels 0/85/170/255). This is how
    /// period display drivers mapped NAPLPS color values onto a limited-width DAC.
    /// </summary>
    public static byte QuantizeGun(byte value, int bits)
    {
        var q = value >> 8 - bits;
        var result = 0;
        var filled = 0;

        while (filled < 8)
        {
            result = result << bits | q;
            filled += bits;
        }

        return (byte)(result >> filled - 8);
    }

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

    /// <summary>
    /// Linearly interpolates between two NaplpsColors.
    /// t=0 returns 'a', t=1 returns 'b'.
    /// </summary>
    public static NaplpsColor Lerp(NaplpsColor a, NaplpsColor b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new NaplpsColor(
            (byte)(a.Green + (b.Green - a.Green) * t),
            (byte)(a.Red + (b.Red - a.Red) * t),
            (byte)(a.Blue + (b.Blue - a.Blue) * t)
        );
    }

    public override string ToString()
    {
        return $"<{Green},{Red},{Blue}>";
    }
}
