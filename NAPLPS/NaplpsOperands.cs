// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS;

public class NaplpsOperands : List<byte>
{
    /// <summary>Get a range of NaplpsOperands</summary>
    /// <param name="range">The range requested</param>
    /// <returns>A new NaplpOperands, shallow copied, of the range requested</returns>
    public NaplpsOperands this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(Count);
            var newOperands = new NaplpsOperands();
            newOperands.AddRange(GetRange(offset, length));
            return newOperands;
        }
    }

    /// <summary>Access a bit of a NaplpsOperands byte</summary>
    /// <param name="theByte">The index of the NaplpsOperand</param>
    /// <param name="theBit">The 1, 8 index based bit (matches official documentation)</param>
    /// <returns>A bit (0, 1)</returns>
    public bool this[int theByte, int theBit]
    {
        get => GetBit(base[theByte], theBit);
    }

    static bool GetBit(byte b, int bitNumber)
    {
        if (bitNumber < 1 || bitNumber > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(bitNumber), "Bit number must be between 1 and 8.");
        }

        return (b & 1 << bitNumber - 1) != 0;
    }
}
