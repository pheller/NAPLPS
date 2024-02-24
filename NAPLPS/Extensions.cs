// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public static class Extensions
{
    public static bool IsEOF(this BinaryReader stream)
    {
        return stream.BaseStream.IsEOF();
    }

    public static bool IsEOF(this Stream stream)
    {
        return stream.CanSeek && stream.Position >= stream.Length;
    }
}
