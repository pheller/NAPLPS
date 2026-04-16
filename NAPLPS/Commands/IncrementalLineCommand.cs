// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// INCREMENTAL LINE (0x3A) - Defines a scribble which is an efficient representation
/// for certain types of polylines such as a signature.
/// </summary>
[AddCommand(240, "Incremental Line", "Efficient polyline via 2-bit motion codes (signatures, scribbles).", Category = CommandCategory.Incremental, DslKeyword = "scribble")]
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

        // First operand is a multi-value operand for dx, dy step size
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

        // The dx and dy step sizes are fixed from the first multi-value operand
        float dx = StartOffset.X;
        float dy = StartOffset.Y;

        // Sign tracking: starts positive, can be reversed by meta opcodes
        int signDx = 1;
        int signDy = 1;

        // Draw flag starts ON
        bool drawFlag = true;

        int bitIndex = 0;

        while (bitIndex + 2 <= bitBuffer.Count)
        {
            int code = (bitBuffer[bitIndex] ? 2 : 0) + (bitBuffer[bitIndex + 1] ? 1 : 0);
            bitIndex += 2;

            switch (code)
            {
                case 0b00:
                {
                    // Meta opcode: read another 2-bit sub-opcode
                    if (bitIndex + 2 > bitBuffer.Count)
                    {
                        // Not enough bits for sub-opcode, stop parsing
                        return;
                    }

                    int subCode = (bitBuffer[bitIndex] ? 2 : 0) + (bitBuffer[bitIndex + 1] ? 1 : 0);
                    bitIndex += 2;

                    switch (subCode)
                    {
                        case 0b00:
                        {
                            drawFlag = !drawFlag;
                        }
                        break;

                        case 0b01:
                        {
                            signDx = -signDx;
                        }
                        break;

                        case 0b10:
                        {
                            signDy = -signDy;
                        }
                        break;

                        case 0b11:
                        {
                            signDx = -signDx;
                            signDy = -signDy;
                        }
                        break;
                    }
                }
                break;

                case 0b01:
                {
                    // Move dx in x direction only
                    Segments.Add(new MotionSegment
                    {
                        Draw = drawFlag,
                        Dx = signDx * dx,
                        Dy = 0
                    });
                }
                break;

                case 0b10:
                {
                    // Move dy in y direction only
                    Segments.Add(new MotionSegment
                    {
                        Draw = drawFlag,
                        Dx = 0,
                        Dy = signDy * dy
                    });
                }
                break;

                case 0b11:
                {
                    // Move dx and dy simultaneously
                    Segments.Add(new MotionSegment
                    {
                        Draw = drawFlag,
                        Dx = signDx * dx,
                        Dy = signDy * dy
                    });
                }
                break;
            }
        }
    }

    public struct MotionSegment
    {
        public bool Draw { get; set; }   // Whether to draw a line (or just move)
        public float Dx { get; set; }    // Delta X displacement for this segment
        public float Dy { get; set; }    // Delta Y displacement for this segment
    }
}
