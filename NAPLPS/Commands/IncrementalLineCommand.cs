// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// INCREMENTAL LINE (0x3A) - Defines a scribble which is an efficient representation
/// for certain types of polylines such as a signature.
/// </summary>
public class IncrementalLineCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValueAndString;

    /// <summary>
    /// The starting offset (dx, dy) from current pen position.
    /// </summary>
    public Vector3 StartOffset { get; }

    /// <summary>
    /// List of motion segments parsed from the string data.
    /// </summary>
    public List<MotionSegment> Segments { get; } = new();

    /// <summary>
    /// Whether the command is valid.
    /// </summary>
    public new bool IsValid { get; }

    public IncrementalLineCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        if (operands.Count == 0)
        {
            IsValid = false;
            return;
        }

        IsValid = true;

        // First operand is a multi-value operand for dx, dy offset
        var vertices = ProcessVertices(operands);
        if (vertices.Count > 0)
        {
            StartOffset = vertices[0];
        }

        // Parse the string portion for motion codes
        // Motion codes are 2-bit instructions followed by optional dx/dy values
        ParseMotionCodes(operands, vertices.Count > 0 ? GetOperandByteCount(vertices[0]) : 0);
    }

    private int GetOperandByteCount(Vector3 vertex)
    {
        // Estimate how many bytes were used for the first vertex
        // This depends on domain settings, simplified here
        return State.MultiByteValue * 2; // dx and dy
    }

    private void ParseMotionCodes(NaplpsOperands operands, int startOffset)
    {
        // Build bit buffer from remaining operands
        var bitBuffer = new List<bool>();

        // Skip the multi-value operand bytes
        int skipBytes = Math.Min(startOffset, operands.Count);
        for (int i = skipBytes; i < operands.Count; i++)
        {
            byte b = operands[i];
            // Each data byte has 6 usable bits
            for (int bit = 5; bit >= 0; bit--)
            {
                bitBuffer.Add((b & (1 << bit)) != 0);
            }
        }

        // Parse motion codes
        // 00 = end OR toggle draw flag (if followed by more data)
        // 01 = move dy only
        // 10 = move dx only
        // 11 = move dx and dy
        int bitIndex = 0;
        bool drawFlag = true; // Draw flag starts ON

        while (bitIndex + 2 <= bitBuffer.Count)
        {
            int code = (bitBuffer[bitIndex] ? 2 : 0) + (bitBuffer[bitIndex + 1] ? 1 : 0);
            bitIndex += 2;

            if (code == 0)
            {
                // Check if this is end or toggle
                if (bitIndex + 2 <= bitBuffer.Count)
                {
                    // Toggle draw flag
                    drawFlag = !drawFlag;
                }
                else
                {
                    // End of scribble
                    break;
                }
            }
            else
            {
                var segment = new MotionSegment
                {
                    Draw = drawFlag,
                    HasDx = code == 2 || code == 3,
                    HasDy = code == 1 || code == 3
                };

                // Read dx if present (simplified: using 6 bits as signed value)
                if (segment.HasDx && bitIndex + 6 <= bitBuffer.Count)
                {
                    segment.Dx = ReadSignedBits(bitBuffer, ref bitIndex, 6);
                }

                // Read dy if present
                if (segment.HasDy && bitIndex + 6 <= bitBuffer.Count)
                {
                    segment.Dy = ReadSignedBits(bitBuffer, ref bitIndex, 6);
                }

                Segments.Add(segment);
            }
        }
    }

    private static int ReadSignedBits(List<bool> buffer, ref int index, int bits)
    {
        if (index + bits > buffer.Count) return 0;

        int value = 0;
        bool negative = buffer[index]; // First bit is sign

        for (int i = 0; i < bits && index < buffer.Count; i++)
        {
            value = (value << 1) | (buffer[index++] ? 1 : 0);
        }

        // Convert from sign-magnitude or two's complement as needed
        if (negative)
        {
            // Simple sign-magnitude interpretation
            value = -(value & ((1 << (bits - 1)) - 1));
        }

        return value;
    }

    public struct MotionSegment
    {
        public bool Draw;   // Whether to draw a line (or just move)
        public bool HasDx;  // Whether dx is specified
        public bool HasDy;  // Whether dy is specified
        public int Dx;      // Delta X (in logical pel units)
        public int Dy;      // Delta Y (in logical pel units)
    }
}
