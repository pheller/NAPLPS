// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Reflection;

namespace NAPLPS;

public static class NaplpsUtils
{
    /// <summary>
    /// Display ratio: the visible portion of the unit screen's Y axis.
    /// ANSI X3.110 spec: "lower 75 percent" (0.75), Byte Magazine 1983: "usually closer to 0.78".
    /// Default 0.80 matches historical PP3/Prodigy rendering. Configurable for different displays.
    /// </summary>
    public static float DisplayRatio { get; set; } = 0.80f;



    public static double CalculateDistance(Point p1, Point p2)
    {
        int dx = p2.X - p1.X;  // Difference in X coordinates
        int dy = p2.Y - p1.Y;  // Difference in Y coordinates

        return Math.Sqrt(dx * dx + dy * dy);  // Pythagorean theorem
    }

    public static (int, int, int, int) ConvertRectToScreen(Size size, double x, double y, double width, double height)
    {
        var x2 = x + width;
        var y2 = y + height;

        var normalizedPoint1 = ConvertNormalizedToPoint(size, x, y);
        var normalizedPoint2 = ConvertNormalizedToPoint(size, x2, y2);

        return (
            Math.Min(normalizedPoint1.X, normalizedPoint2.X),
            Math.Min(normalizedPoint1.Y, normalizedPoint2.Y),
            Math.Max(normalizedPoint1.X, normalizedPoint2.X),
            Math.Max(normalizedPoint1.Y, normalizedPoint2.Y)
        );
    }

    public static (int, int) ConvertNormalizedToScreenScale(Size size, double normalizedX, double normalizedY)
    {
        var shrunkY = normalizedY / DisplayRatio;

        int actualX = (int)(normalizedX * size.Width);
        int actualY = (int)(shrunkY * size.Height);

        return (actualX, actualY);
    }

    public static Point ConvertNormalizedToPoint(Size size, double normalizedX, double normalizedY)
    {
        var (actualX, actualY) = ConvertNormalizedToScreenScale(size, normalizedX, normalizedY);
        (actualX, actualY) = ConvertCoordinates(size.Width, size.Height, actualX, actualY);

        return new Point(actualX, actualY);
    }

    public static (int, int) ConvertCoordinates(int _, int height, int x, int y)
    {
        // Convert x from top-right origin to bottom-left origin by subtracting from width
        int convertedX = x;

        // Convert y from top-right origin to bottom-left origin by subtracting from height
        // int convertedY = height - y;
        int convertedY = height - y;

        return (convertedX, convertedY);
    }

    /// <summary>
    /// Float version of coordinate conversion for higher precision (used by arcs, etc.)
    /// Converts NAPLPS normalized coords to screen coords with Y-flip.
    /// </summary>
    public static (float, float) ConvertNormalizedToScreenF(Size size, float normalizedX, float normalizedY)
    {
        float screenX = normalizedX * size.Width;
        float screenY = size.Height - (normalizedY / DisplayRatio * size.Height);
        return (screenX, screenY);
    }

    /// <summary>
    /// Reverse conversion: screen coords back to NAPLPS normalized coords.
    /// </summary>
    public static (float, float) ConvertScreenToNormalizedF(Size size, float screenX, float screenY)
    {
        float normX = screenX / size.Width;
        float normY = (size.Height - screenY) / size.Height * DisplayRatio;
        return (normX, normY);
    }

    public static (int, int, int, int) ConvertNormalizedDimensionsToPixels(Size size, double normalizedX, double normalizedY, double normalizedWidth, double normalizedHeight)
    {
        // Adjust for negative dimensions
        if (normalizedWidth < 0)
        {
            normalizedX += normalizedWidth;
            normalizedWidth = Math.Abs(normalizedWidth);
        }

        if (normalizedHeight < 0)
        {
            normalizedY += normalizedHeight;
            normalizedHeight = Math.Abs(normalizedHeight);
        }

        // Clamp the normalized coordinates to their valid ranges
        normalizedX = Math.Clamp(normalizedX, 0, 1);
        normalizedY = Math.Clamp(normalizedY, 0, DisplayRatio);

        // Adjust normalizedY by shrinking it to the 0-1 range
        var shrunkY = normalizedY / DisplayRatio;

        // Convert normalized starting point to pixels
        int startX = (int)(normalizedX * size.Width);
        int startY = (int)(shrunkY * size.Height);

        // Convert normalized dimensions to pixels
        int widthPixels = (int)(normalizedWidth * size.Width);
        int heightPixels = (int)(normalizedHeight * size.Height);

        // Convert coordinates to the desired origin system
        (startX, startY) = ConvertCoordinates(size.Width, size.Height, startX, startY);

        return (startX, startY, widthPixels, heightPixels);
    }

    public static byte ConvertBitsToByte(List<bool> booleans)
    {
        byte result = 0;
        int maxBits = 8;

        for (int i = 0; i < Math.Min(booleans.Count, maxBits); i++)
        {
            if (booleans[i])
            {
                result |= (byte)(1 << i);
            }
        }

        return result;
    }

    public static float ConvertBitsToFraction(List<bool> boolList)
    {
        if (boolList.Count == 0)
        {
            return 0f;
        }

        float fraction = 0f;
        float baseValue = 0.5f; // Starting from the MSB just right of the decimal point

        var numericalData = boolList[1..];

        foreach (bool bit in numericalData)
        {
            fraction += bit ? baseValue : 0f;
            baseValue /= 2f;
        }

        // Is negative
        if (boolList[0]) // If the number is negative
        {
            fraction = -1 + fraction; // Adjust for two's complement notation
        }

        return fraction;
    }

    /// <summary>
    /// Reverse of ConvertBitsToFraction. Given a float in [-1, 1), produce the bit list
    /// that ConvertBitsToFraction would decode back to that float.
    /// bit[0] = sign (true if negative), bits[1..N] = binary fraction of the absolute value.
    /// Negative values use -1 + fraction representation (matching the decoder).
    /// </summary>
    public static List<bool> ConvertFractionToBits(float value, int totalBits)
    {
        var bits = new List<bool>(totalBits);

        // NAPLPS coordinate encoding: bit 1 = sign (1=negative, 0=positive).
        // For positive: remaining bits encode the fraction starting at 0.5.
        // For negative: decoder does result = -1 + fraction, so encode fraction = value + 1.
        // The sign bit is part of the total bit count — all totalBits are used.
        bool isNegative = value < 0;
        bits.Add(isNegative);

        float fraction = isNegative ? value + 1.0f : value;
        float baseValue = 0.5f;

        for (int i = 1; i < totalBits; i++)
        {
            if (fraction >= baseValue)
            {
                bits.Add(true);
                fraction -= baseValue;
            }
            else
            {
                bits.Add(false);
            }

            baseValue /= 2f;
        }

        return bits;
    }

    public static int ConvertBitsToInt(List<bool> binaryArray)
    {
        if (binaryArray == null || binaryArray.Count == 0)
        {
            throw new ArgumentException("Array must not be empty or null.");
        }

        int point = 0;

        for (int i = 0; i < binaryArray.Count; i++)
        {
            if (binaryArray[i])
            {
                point |= 1 << binaryArray.Count - 1 - i;
            }
        }

        // Handling two's complement for negative numbers
        if (binaryArray[0]) // Check if the number is negative (MSB is true/1)
        {
            // Invert and add 1 to handle two's complement
            point = -(int)((~(uint)point & (1U << binaryArray.Count) - 1) + 1);
        }

        return point;
    }

    /// <summary>
    /// Enumerate the human-readable names of every <c>[AddCommand]</c>-attributed command
    /// class. Iterates the AOT-safe static type list in <see cref="CommandRegistryKnownTypes"/>
    /// instead of <c>Assembly.GetTypes()</c> so the trimmer preserves command classes.
    /// </summary>
    public static List<string> GetAddCommands()
    {
        var output = new List<string>();

        foreach (var type in CommandRegistryKnownTypes.All)
        {
            var attribute = type.GetCustomAttributes(typeof(AddCommandAttribute), false)?.FirstOrDefault();
            if (attribute == null) { continue; }
            output.Add(((AddCommandAttribute)attribute).Name);
        }

        return output;
    }
}
