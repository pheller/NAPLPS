// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public static class Extensions
{
    // Naplps Nuance Extensions

    public static bool IsOpcode(this byte b)
    {
        return (b & 1 << 6) == 0;
    }

    public static bool IsEOF(this BinaryReader stream)
    {
        return stream.BaseStream.IsEOF();
    }

    public static bool IsEOF(this Stream stream)
    {
        return stream.CanSeek && stream.Position >= stream.Length;
    }

    // Color Extensions

    public static Color From3BitGRB(this Color _, int red3Bit, int green3Bit, int blue3Bit)
    {
        // Ensure the input values are within the 3-bit range
        green3Bit = Math.Clamp(green3Bit, 0, 7);
        red3Bit = Math.Clamp(red3Bit, 0, 7);
        blue3Bit = Math.Clamp(blue3Bit, 0, 7);

        // Convert 3-bit values to 8-bit
        int green = green3Bit * 255 / 7;
        int red = red3Bit * 255 / 7;
        int blue = blue3Bit * 255 / 7;

        // Create and return the new Color
        return Color.FromArgb(255, red, green, blue); // Alpha is set to 255 (fully opaque)
    }

    public static (int green3Bit, int red3Bit, int blue3Bit) To3BitGRB(this Color color)
    {
        // Convert 8-bit values to 3-bit
        int green3Bit = (int)Math.Round(color.G * 7.0 / 255);
        int red3Bit = (int)Math.Round(color.R * 7.0 / 255);
        int blue3Bit = (int)Math.Round(color.B * 7.0 / 255);

        return (green3Bit, red3Bit, blue3Bit);
    }

    public static Color From6BitRGB(this Color _, int red6Bit, int green6Bit, int blue6Bit)
    {
        // Ensure the input values are within the 6-bit range
        red6Bit = Math.Clamp(red6Bit, 0, 63);
        green6Bit = Math.Clamp(green6Bit, 0, 63);
        blue6Bit = Math.Clamp(blue6Bit, 0, 63);

        // Convert 6-bit values to 8-bit
        int red = red6Bit * 255 / 63;
        int green = green6Bit * 255 / 63;
        int blue = blue6Bit * 255 / 63;

        // Create and return the new Color
        return Color.FromArgb(255, red, green, blue); // Alpha is set to 255 (fully opaque)
    }

    public static (int red6Bit, int green6Bit, int blue6Bit) To6BitRGB(this Color color)
    {
        // Convert 8-bit values to 6-bit
        int red6Bit = (int)Math.Round(color.R * 63.0 / 255);
        int green6Bit = (int)Math.Round(color.G * 63.0 / 255);
        int blue6Bit = (int)Math.Round(color.B * 63.0 / 255);

        return (red6Bit, green6Bit, blue6Bit);
    }

}
