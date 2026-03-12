// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// INCREMENTAL POLY FILLED (0x3B) - Almost identical to INCREMENTAL LINE except:
/// 1. The scribble is filled with the current colors and fill pattern
/// 2. The draw flag is always on
/// 3. If an opcode to turn off the draw flag is encountered, the drawing point
///    is returned to its last known location
/// </summary>
public class IncrementalPolygonFilledCommand : GeometricDrawingCommandBase
{
    public static new readonly NaplpsOperandType OperandType = NaplpsOperandType.MultiValueAndString;

    /// <summary>
    /// The starting offset (dx, dy) from current pen position.
    /// </summary>
    public Vector3 StartOffset { get; }

    /// <summary>
    /// List of motion segments parsed from the string data.
    /// All segments have Draw = true for filled polygon.
    /// </summary>
    public List<MotionSegment> Segments { get; } = new();

    /// <summary>
    /// Whether the command is valid.
    /// </summary>
    public new bool IsValid { get; }

    public IncrementalPolygonFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
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
        ParseMotionCodes(operands, vertices.Count > 0 ? GetOperandByteCount(vertices[0]) : 0);
    }

    private int GetOperandByteCount(Vector3 vertex)
    {
        return State.MultiByteValue * 2; // dx and dy
    }

    private void ParseMotionCodes(NaplpsOperands operands, int startOffset)
    {
        var bitBuffer = new List<bool>();

        int skipBytes = Math.Min(startOffset, operands.Count);
        for (int i = skipBytes; i < operands.Count; i++)
        {
            byte b = operands[i];
            for (int bit = 5; bit >= 0; bit--)
            {
                bitBuffer.Add((b & (1 << bit)) != 0);
            }
        }

        int bitIndex = 0;
        Vector3 lastKnownPosition = Vector3.Zero;
        bool returnToLastKnown = false;

        while (bitIndex + 2 <= bitBuffer.Count)
        {
            int code = (bitBuffer[bitIndex] ? 2 : 0) + (bitBuffer[bitIndex + 1] ? 1 : 0);
            bitIndex += 2;

            if (code == 0)
            {
                if (bitIndex + 2 <= bitBuffer.Count)
                {
                    // In filled polygon, "toggle draw flag off" means return to last known position
                    returnToLastKnown = true;
                }
                else
                {
                    break;
                }
            }
            else
            {
                var segment = new MotionSegment
                {
                    Draw = true, // Always draw for filled polygon
                    HasDx = code == 2 || code == 3,
                    HasDy = code == 1 || code == 3,
                    ReturnToLastKnown = returnToLastKnown
                };

                returnToLastKnown = false;

                if (segment.HasDx && bitIndex + 6 <= bitBuffer.Count)
                {
                    segment.Dx = ReadSignedBits(bitBuffer, ref bitIndex, 6);
                }

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
        bool negative = buffer[index];

        for (int i = 0; i < bits && index < buffer.Count; i++)
        {
            value = (value << 1) | (buffer[index++] ? 1 : 0);
        }

        if (negative)
        {
            value = -(value & ((1 << (bits - 1)) - 1));
        }

        return value;
    }

    public struct MotionSegment
    {
        public bool Draw;
        public bool HasDx;
        public bool HasDy;
        public int Dx;
        public int Dy;
        public bool ReturnToLastKnown; // For filled polygon special behavior
    }
}
