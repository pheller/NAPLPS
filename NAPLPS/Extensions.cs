// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public static class Extensions
{
    public static bool GetBit(this byte b, int bitNumber)
    {
        if (bitNumber < 1 || bitNumber > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(bitNumber), "Bit number must be between 1 and 8.");
        }

        return (b & 1 << bitNumber - 1) != 0;
    }

    public static bool IsEOF(this BinaryReader stream)
    {
        return stream.BaseStream.IsEOF();
    }

    public static bool IsEOF(this Stream stream)
    {
        return stream.CanSeek && stream.Position >= stream.Length;
    }
}
