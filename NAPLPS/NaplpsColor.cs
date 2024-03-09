namespace NAPLPS;

public class NaplpsColor
{
    private byte green;
    private byte red;
    private byte blue;

    public static NaplpsColor Empty { get; } = new(0, 0, 0);

    public byte Green
    {
        get => green;
        set => green = ValidateColorValue(value, nameof(Green));
    }

    public byte Red
    {
        get => red;
        set => red = ValidateColorValue(value, nameof(Red));
    }

    public byte Blue
    {
        get => blue;
        set => blue = ValidateColorValue(value, nameof(Blue));
    }

    public NaplpsColor()
    {
    }

    public NaplpsColor(byte green, byte red, byte blue)
    {
        Green = green;
        Red = red;
        Blue = blue;
    }

    public static NaplpsColor From3BitGRB(int red3Bit, int green3Bit, int blue3Bit)
    {
        // Ensure the input values are within the 3-bit range
        green3Bit = Math.Clamp(green3Bit, 0, 7);
        red3Bit = Math.Clamp(red3Bit, 0, 7);
        blue3Bit = Math.Clamp(blue3Bit, 0, 7);

        // Convert 3-bit values to 6-bit
        int green = green3Bit * 63 / 7;
        int red = red3Bit * 63 / 7;
        int blue = blue3Bit * 63 / 7;

        return new NaplpsColor((byte)green, (byte)red, (byte)blue);
    }

    private static byte ValidateColorValue(byte value, string colorName)
    {
        if (value > 63)
        {
            throw new ArgumentException($"{colorName} value must be between 0 and 63. Given value: <{value}>");
        }

        return value;
    }

    public override string ToString()
    {
        return $"<{Green},{Red},{Blue}>";
    }
}
