// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// INCREMENTAL POINT (0x39) - Displays a color bitmap within the active field.
/// Each pixel is drawn using one logical pel with the color specified in the bitmap.
/// </summary>
public class IncrementalPointCommand : NaplpsCommand
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.FixedAndString;

    /// <summary>
    /// Bits per pixel (1-48). If 0 or > 48, the command is discarded.
    /// </summary>
    public int BitsPerPixel { get; }

    /// <summary>
    /// The pixel data as a list of colors/palette indices.
    /// </summary>
    public List<PixelData> Pixels { get; } = new();

    /// <summary>
    /// Whether the command is valid (BitsPerPixel in range 1-48).
    /// </summary>
    public new bool IsValid { get; }

    public IncrementalPointCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (operands.Count == 0)
        {
            IsValid = false;
            return;
        }

        // First byte is the fixed format parameter describing bits per pixel
        BitsPerPixel = operands[0] & 0x3F; // Lower 6 bits

        if (BitsPerPixel == 0 || BitsPerPixel > 48)
        {
            IsValid = false;
            return;
        }

        IsValid = true;

        // Parse the pixel data from the remaining operands
        // The data is a bitstring where each pixel uses BitsPerPixel bits
        if (operands.Count > 1)
        {
            ParsePixelData(operands);
        }
    }

    private void ParsePixelData(NaplpsOperands operands)
    {
        // Build a bit buffer from the operands (starting after the BitsPerPixel byte)
        var bitBuffer = new List<bool>();

        for (int i = 1; i < operands.Count; i++)
        {
            byte b = operands[i];
            // Each data byte has 6 usable bits (bits 0-5)
            for (int bit = 5; bit >= 0; bit--)
            {
                bitBuffer.Add((b & (1 << bit)) != 0);
            }
        }

        // Parse pixels from the bit buffer
        int bitIndex = 0;
        while (bitIndex + BitsPerPixel <= bitBuffer.Count)
        {
            // Check for repositioning codes (first 2 bits)
            if (bitIndex + 2 <= bitBuffer.Count)
            {
                int code = (bitBuffer[bitIndex] ? 2 : 0) + (bitBuffer[bitIndex + 1] ? 1 : 0);

                if (code == 0)
                {
                    // 00 = end of bitmap
                    break;
                }
                else if (code == 1 || code == 2 || code == 3)
                {
                    // Repositioning: 01=dy, 10=dx, 11=dx+dy
                    // For now, we'll handle these as special markers
                    var pixel = new PixelData
                    {
                        IsRepositioning = true,
                        RepositionCode = code
                    };

                    // Read the offset values based on domain single-byte width
                    // This is simplified - full implementation would parse dx/dy values
                    bitIndex += 2;

                    if (code == 1 || code == 3) // dy
                    {
                        // Skip dy bits
                        bitIndex += 6;
                    }
                    if (code == 2 || code == 3) // dx
                    {
                        // Skip dx bits
                        bitIndex += 6;
                    }

                    Pixels.Add(pixel);
                    continue;
                }
            }

            // Extract pixel color value
            int colorValue = 0;
            for (int b = 0; b < BitsPerPixel && bitIndex < bitBuffer.Count; b++)
            {
                colorValue = (colorValue << 1) | (bitBuffer[bitIndex++] ? 1 : 0);
            }

            Pixels.Add(new PixelData
            {
                IsRepositioning = false,
                ColorValue = colorValue
            });
        }
    }

    public struct PixelData
    {
        public bool IsRepositioning;
        public int RepositionCode; // 1=dy, 2=dx, 3=dx+dy
        public int ColorValue;
        public int DeltaX;
        public int DeltaY;
    }
}
