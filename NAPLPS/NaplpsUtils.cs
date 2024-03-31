namespace NAPLPS;

public static class NaplpsUtils
{
    public static Point ConvertNormalizedToPoint(Size size, double normalizedX, double normalizedY)
    {
        if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 0.75)
        {
            normalizedX = Math.Clamp(normalizedX, 0, 1);
            normalizedY = Math.Clamp(normalizedY, 0, 0.75);
        }

        var shrunkY = normalizedY / 0.75;

        int actualX = (int)(normalizedX * size.Width);
        int actualY = (int)(shrunkY * size.Height);

        (actualX, actualY) = ConvertCoordinates(size.Width, size.Height, actualX, actualY);

        return new Point(actualX, actualY);
    }

    public static (int, int) ConvertCoordinates(int _, int height, int x, int y)
    {
        // Convert x from top-right origin to bottom-left origin by subtracting from width
        int convertedX = x;

        // Convert y from top-right origin to bottom-left origin by subtracting from height
        int convertedY = height - y;

        return (convertedX, convertedY);
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
}
