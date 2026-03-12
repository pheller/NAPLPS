// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

using static NAPLPS.NaplpsUtils;

namespace NAPLPSTests;

[TestClass]
public class EncoderTests
{
    private const float Tolerance = 1f / 512f; // 9-bit precision for multiByteValue=3

    #region ConvertFractionToBits Round-Trip — Basic Values

    [TestMethod]
    public void ConvertFractionToBits_Zero_RoundTrips()
    {
        var bits = ConvertFractionToBits(0.0f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(0.0f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_PositiveHalf_RoundTrips()
    {
        var bits = ConvertFractionToBits(0.5f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(0.5f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_PositiveQuarter_RoundTrips()
    {
        var bits = ConvertFractionToBits(0.25f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(0.25f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_Positive375_RoundTrips()
    {
        var bits = ConvertFractionToBits(0.375f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(0.375f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_NegativeHalf_RoundTrips()
    {
        // -0.5 encoded as: sign=true, fraction = -0.5 + 1 = 0.5
        var bits = ConvertFractionToBits(-0.5f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(-0.5f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_NegativeQuarter_RoundTrips()
    {
        var bits = ConvertFractionToBits(-0.25f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(-0.25f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_SmallPositive_RoundTrips()
    {
        var bits = ConvertFractionToBits(0.125f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(0.125f, result, Tolerance);
    }

    #endregion

    #region ConvertFractionToBits Round-Trip — Edge Cases

    [TestMethod]
    public void ConvertFractionToBits_NearOne_RoundTrips()
    {
        // Largest positive value representable: 0.5 + 0.25 + 0.125 + ... ≈ 0.998
        float value = 0.5f + 0.25f + 0.125f + 0.0625f;
        var bits = ConvertFractionToBits(value, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(value, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_NearNegativeOne_RoundTrips()
    {
        // Close to -1.0: sign=true, fraction near 0
        // -1 + 0.0625 = -0.9375
        float value = -0.9375f;
        var bits = ConvertFractionToBits(value, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(value, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_NegativeEighth_RoundTrips()
    {
        var bits = ConvertFractionToBits(-0.125f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(-0.125f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_Negative375_RoundTrips()
    {
        var bits = ConvertFractionToBits(-0.375f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(-0.375f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_Negative75_RoundTrips()
    {
        var bits = ConvertFractionToBits(-0.75f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(-0.75f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_SmallestPositive_RoundTrips()
    {
        // Smallest positive fraction with 9 bits: 1/256
        float value = 1f / 256f;
        var bits = ConvertFractionToBits(value, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(value, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_Positive75_RoundTrips()
    {
        var bits = ConvertFractionToBits(0.75f, 9);
        var result = ConvertBitsToFraction(bits);

        Assert.AreEqual(0.75f, result, Tolerance);
    }

    [TestMethod]
    public void ConvertFractionToBits_PositiveSignBitIsFalse()
    {
        var bits = ConvertFractionToBits(0.5f, 9);

        Assert.IsFalse(bits[0], "Sign bit should be false for positive values");
    }

    [TestMethod]
    public void ConvertFractionToBits_NegativeSignBitIsTrue()
    {
        var bits = ConvertFractionToBits(-0.5f, 9);

        Assert.IsTrue(bits[0], "Sign bit should be true for negative values");
    }

    [TestMethod]
    public void ConvertFractionToBits_ZeroSignBitIsFalse()
    {
        var bits = ConvertFractionToBits(0.0f, 9);

        Assert.IsFalse(bits[0], "Sign bit should be false for zero");
    }

    [TestMethod]
    public void ConvertFractionToBits_ProducesCorrectBitCount()
    {
        var bits6 = ConvertFractionToBits(0.5f, 6);
        var bits9 = ConvertFractionToBits(0.5f, 9);
        var bits12 = ConvertFractionToBits(0.5f, 12);

        Assert.AreEqual(6, bits6.Count);
        Assert.AreEqual(9, bits9.Count);
        Assert.AreEqual(12, bits12.Count);
    }

    [TestMethod]
    public void ConvertFractionToBits_FewerBits_LosesPrecision()
    {
        // With only 4 bits (1 sign + 3 fraction), precision is 1/8
        float value = 0.3f; // Not exactly representable in 3 fraction bits
        var bits = ConvertFractionToBits(value, 4);
        var result = ConvertBitsToFraction(bits);

        // Should be quantized to 0.25 (nearest representable value)
        Assert.AreEqual(0.25f, result, 1f / 8f);
    }

    [TestMethod]
    public void ConvertFractionToBits_SweepPositives_AllRoundTrip()
    {
        // Test a range of positive values
        float[] values = [0.0f, 0.0625f, 0.125f, 0.1875f, 0.25f, 0.3125f, 0.375f, 0.4375f, 0.5f, 0.5625f, 0.625f, 0.6875f, 0.75f, 0.8125f, 0.875f];

        foreach (var value in values)
        {
            var bits = ConvertFractionToBits(value, 9);
            var result = ConvertBitsToFraction(bits);

            Assert.AreEqual(value, result, Tolerance, $"Failed for value {value}");
        }
    }

    [TestMethod]
    public void ConvertFractionToBits_SweepNegatives_AllRoundTrip()
    {
        // Test a range of negative values
        float[] values = [-0.0625f, -0.125f, -0.1875f, -0.25f, -0.3125f, -0.375f, -0.4375f, -0.5f, -0.5625f, -0.625f, -0.6875f, -0.75f, -0.8125f, -0.875f];

        foreach (var value in values)
        {
            var bits = ConvertFractionToBits(value, 9);
            var result = ConvertBitsToFraction(bits);

            Assert.AreEqual(value, result, Tolerance, $"Failed for value {value}");
        }
    }

    #endregion

    #region EncodeVertex2D Round-Trip — Basic

    [TestMethod]
    public void EncodeVertex2D_Origin_RoundTrips()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.0f, 0.0f);

        // Decode using a PointSetAbsolute command
        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(0.0f, command.Points[0].X, Tolerance);
        Assert.AreEqual(0.0f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_Center_RoundTrips()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.375f);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(0.5f, command.Points[0].X, Tolerance);
        Assert.AreEqual(0.375f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_375_25_RoundTrips()
    {
        // Known value from Byte Magazine test case
        var operands = NaplpsEncoder.EncodeVertex2D(0.375f, 0.25f);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(0.375f, command.Points[0].X, Tolerance);
        Assert.AreEqual(0.25f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_AllOperandsAreNumericalData()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f);

        foreach (byte b in operands)
        {
            // All bytes must be in the 8-bit numerical data range (0xC0-0xFF)
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range (expected 0xC0-0xFF)");
        }
    }

    #endregion

    #region EncodeVertex2D Round-Trip — Negative Coordinates

    [TestMethod]
    public void EncodeVertex2D_NegativeX_RoundTrips()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(-0.25f, 0.5f);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(-0.25f, command.Points[0].X, Tolerance);
        Assert.AreEqual(0.5f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_NegativeY_RoundTrips()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, -0.25f);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(0.5f, command.Points[0].X, Tolerance);
        Assert.AreEqual(-0.25f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_BothNegative_RoundTrips()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(-0.375f, -0.5f);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(-0.375f, command.Points[0].X, Tolerance);
        Assert.AreEqual(-0.5f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_NegativeXPositiveY_RoundTrips()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(-0.5f, 0.375f);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(-0.5f, command.Points[0].X, Tolerance);
        Assert.AreEqual(0.375f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_NegativeCoordinates_AllNumericalData()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(-0.5f, -0.25f);

        foreach (byte b in operands)
        {
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range for negative coords");
        }
    }

    #endregion

    #region EncodeVertex2D Round-Trip — Boundary Values

    [TestMethod]
    public void EncodeVertex2D_NearMaxPositive_RoundTrips()
    {
        float nearMax = 0.5f + 0.25f + 0.125f; // 0.875
        var operands = NaplpsEncoder.EncodeVertex2D(nearMax, nearMax);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(nearMax, command.Points[0].X, Tolerance);
        Assert.AreEqual(nearMax, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_NearMaxNegative_RoundTrips()
    {
        float nearMin = -0.875f; // sign=true, fraction=0.125
        var operands = NaplpsEncoder.EncodeVertex2D(nearMin, nearMin);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(nearMin, command.Points[0].X, Tolerance);
        Assert.AreEqual(nearMin, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertex2D_ProducesThreeOperandBytes()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f);

        Assert.AreEqual(3, operands.Count, "Default multiByteValue=3 should produce 3 operand bytes");
    }

    [TestMethod]
    public void EncodeVertex2D_MultiByteValue2_ProducesTwoOperandBytes()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f, 2);

        Assert.AreEqual(2, operands.Count);
    }

    [TestMethod]
    public void EncodeVertex2D_MultiByteValue1_ProducesOneOperandByte()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f, 1);

        Assert.AreEqual(1, operands.Count);
    }

    [TestMethod]
    public void EncodeVertex2D_MultiByteValue4_ProducesFourOperandBytes()
    {
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f, 4);

        Assert.AreEqual(4, operands.Count);
    }

    [TestMethod]
    public void EncodeVertex2D_SymmetricCoordinates_ProduceSymmetricBits()
    {
        // When X == Y, each operand byte should have symmetric X and Y bits
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f);

        foreach (byte b in operands)
        {
            int xBits = (b >> 3) & 0x07; // bits 5,4,3 (0-indexed)
            int yBits = b & 0x07;         // bits 2,1,0

            Assert.AreEqual(xBits, yBits, $"Byte 0x{b:X2}: X bits ({xBits}) should equal Y bits ({yBits}) for symmetric input");
        }
    }

    #endregion

    #region EncodeVertices2D

    [TestMethod]
    public void EncodeVertices2D_SingleVertex_RoundTrips()
    {
        var vertices = new[] { new Vector3(0.25f, 0.5f, 0) };
        var operands = NaplpsEncoder.EncodeVertices2D(vertices);

        var state = new NaplpsState();
        var command = new PointSetAbsoluteCommand(state, 0xA4, operands);

        Assert.AreEqual(0.25f, command.Points[0].X, Tolerance);
        Assert.AreEqual(0.5f, command.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void EncodeVertices2D_TwoVertices_ProducesSixBytes()
    {
        var vertices = new[]
        {
            new Vector3(0.25f, 0.5f, 0),
            new Vector3(0.75f, 0.375f, 0)
        };
        var operands = NaplpsEncoder.EncodeVertices2D(vertices);

        // 2 vertices × 3 bytes each = 6 bytes
        Assert.AreEqual(6, operands.Count);
    }

    [TestMethod]
    public void EncodeVertices2D_AllBytesAreNumericalData()
    {
        var vertices = new[]
        {
            new Vector3(0.1f, 0.2f, 0),
            new Vector3(0.3f, 0.4f, 0),
            new Vector3(0.5f, 0.6f, 0)
        };
        var operands = NaplpsEncoder.EncodeVertices2D(vertices);

        foreach (byte b in operands)
        {
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range");
        }
    }

    [TestMethod]
    public void EncodeVertices2D_EmptyArray_ProducesEmptyOperands()
    {
        var vertices = Array.Empty<Vector3>();
        var operands = NaplpsEncoder.EncodeVertices2D(vertices);

        Assert.AreEqual(0, operands.Count);
    }

    #endregion

    #region SelectColor Encoding

    [TestMethod]
    public void EncodeSelectColorForeground_AllIndices_AreNumericalData()
    {
        for (byte i = 0; i < 16; i++)
        {
            var operands = NaplpsEncoder.EncodeSelectColorForeground(i);

            Assert.AreEqual(1, operands.Count, $"Index {i}: should produce exactly 1 operand");
            Assert.IsTrue((operands[0] & 0xC0) == 0xC0, $"Index {i}: byte 0x{operands[0]:X2} not in numerical data range");
        }
    }

    [TestMethod]
    public void EncodeSelectColorForegroundBackground_AllIndices_AreNumericalData()
    {
        for (byte fg = 0; fg < 16; fg++)
        {
            for (byte bg = 0; bg < 16; bg++)
            {
                var operands = NaplpsEncoder.EncodeSelectColorForegroundBackground(fg, bg);

                Assert.AreEqual(2, operands.Count, $"FG={fg},BG={bg}: should produce exactly 2 operands");
                Assert.IsTrue((operands[0] & 0xC0) == 0xC0, $"FG={fg}: byte 0x{operands[0]:X2} not in numerical data range");
                Assert.IsTrue((operands[1] & 0xC0) == 0xC0, $"BG={bg}: byte 0x{operands[1]:X2} not in numerical data range");
            }
        }
    }

    [TestMethod]
    public void EncodeSelectColorForeground_DifferentIndices_ProduceDifferentBytes()
    {
        var ops0 = NaplpsEncoder.EncodeSelectColorForeground(0);
        var ops8 = NaplpsEncoder.EncodeSelectColorForeground(8);
        var ops15 = NaplpsEncoder.EncodeSelectColorForeground(15);

        Assert.AreNotEqual(ops0[0], ops8[0], "Index 0 and 8 should produce different bytes");
        Assert.AreNotEqual(ops0[0], ops15[0], "Index 0 and 15 should produce different bytes");
        Assert.AreNotEqual(ops8[0], ops15[0], "Index 8 and 15 should produce different bytes");
    }

    #endregion

    #region Command Builder — Opcodes

    [TestMethod]
    public void BuildPointSetAbsolute_ProducesCorrectOpcode()
    {
        var (opcode, _) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.5f, 0.5f);

        Assert.AreEqual(NaplpsCommandBuilder.OpPointSetAbsolute, opcode);
    }

    [TestMethod]
    public void BuildLineAbsolute_ProducesCorrectOpcode()
    {
        var (opcode, _) = NaplpsCommandBuilder.BuildLineAbsolute(0.5f, 0.5f);

        Assert.AreEqual(NaplpsCommandBuilder.OpLineAbsolute, opcode);
    }

    [TestMethod]
    public void BuildRectangleFilled_ProducesCorrectOpcode()
    {
        var (opcode, _) = NaplpsCommandBuilder.BuildRectangleFilled(0.1f, 0.1f);

        Assert.AreEqual(NaplpsCommandBuilder.OpRectangleFilled, opcode);
    }

    [TestMethod]
    public void BuildRectangleOutlined_ProducesCorrectOpcode()
    {
        var (opcode, _) = NaplpsCommandBuilder.BuildRectangleOutlined(0.1f, 0.1f);

        Assert.AreEqual(NaplpsCommandBuilder.OpRectangleOutlined, opcode);
    }

    [TestMethod]
    public void BuildSelectColor_Foreground_ProducesCorrectOpcode()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildSelectColor(5);

        Assert.AreEqual(NaplpsCommandBuilder.OpSelectColor, opcode);
        Assert.AreEqual(1, operands.Count);
    }

    [TestMethod]
    public void BuildSelectColor_ForegroundBackground_ProducesCorrectOpcode()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildSelectColor(5, 2);

        Assert.AreEqual(NaplpsCommandBuilder.OpSelectColor, opcode);
        Assert.AreEqual(2, operands.Count);
    }

    #endregion

    #region Command Builder — Operand Counts

    [TestMethod]
    public void BuildPointSetAbsolute_ProducesThreeOperands()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.5f, 0.5f);

        Assert.AreEqual(3, operands.Count);
    }

    [TestMethod]
    public void BuildLineAbsolute_ProducesThreeOperands()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildLineAbsolute(0.5f, 0.5f);

        Assert.AreEqual(3, operands.Count);
    }

    [TestMethod]
    public void BuildRectangleFilled_ProducesThreeOperands()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildRectangleFilled(0.1f, 0.1f);

        Assert.AreEqual(3, operands.Count);
    }

    [TestMethod]
    public void BuildRectangleOutlined_ProducesThreeOperands()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildRectangleOutlined(0.1f, 0.1f);

        Assert.AreEqual(3, operands.Count);
    }

    [TestMethod]
    public void BuildLineSetAbsolute_TwoPoints_ProducesSixOperands()
    {
        var points = new[]
        {
            new Vector3(0.25f, 0.25f, 0),
            new Vector3(0.75f, 0.5f, 0)
        };
        var (_, operands) = NaplpsCommandBuilder.BuildLineSetAbsolute(points);

        Assert.AreEqual(6, operands.Count);
    }

    [TestMethod]
    public void BuildLineSetAbsolute_ThreePoints_ProducesNineOperands()
    {
        var points = new[]
        {
            new Vector3(0.1f, 0.1f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(0.9f, 0.1f, 0)
        };
        var (_, operands) = NaplpsCommandBuilder.BuildLineSetAbsolute(points);

        Assert.AreEqual(9, operands.Count);
    }

    [TestMethod]
    public void BuildLineSetAbsolute_ProducesCorrectOpcode()
    {
        var points = new[] { new Vector3(0.5f, 0.5f, 0) };
        var (opcode, _) = NaplpsCommandBuilder.BuildLineSetAbsolute(points);

        Assert.AreEqual(NaplpsCommandBuilder.OpLineSetAbsolute, opcode);
    }

    #endregion

    #region Command Builder — All Operands Are Numerical Data

    [TestMethod]
    public void BuildPointSetAbsolute_AllOperandsAreNumericalData()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.3f, 0.7f);

        foreach (byte b in operands)
        {
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range");
        }
    }

    [TestMethod]
    public void BuildLineAbsolute_AllOperandsAreNumericalData()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildLineAbsolute(0.3f, 0.7f);

        foreach (byte b in operands)
        {
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range");
        }
    }

    [TestMethod]
    public void BuildRectangleFilled_AllOperandsAreNumericalData()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildRectangleFilled(0.3f, 0.2f);

        foreach (byte b in operands)
        {
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range");
        }
    }

    [TestMethod]
    public void BuildRectangleOutlined_AllOperandsAreNumericalData()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildRectangleOutlined(0.3f, 0.2f);

        foreach (byte b in operands)
        {
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range");
        }
    }

    [TestMethod]
    public void BuildSelectColor_AllOperandsAreNumericalData()
    {
        var (_, operands) = NaplpsCommandBuilder.BuildSelectColor(10, 5);

        foreach (byte b in operands)
        {
            Assert.IsTrue((b & 0xC0) == 0xC0, $"Byte 0x{b:X2} not in numerical data range");
        }
    }

    #endregion

    #region Serialize/Reparse Round-Trip — Points

    [TestMethod]
    public void BuildCommand_Serialize_Reparse_PreservesVertex()
    {
        var format = NaplpsFormat.New();
        var (opcode, operands) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.375f, 0.25f);
        format.AddCommand(opcode, operands);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var pointCmd = FindCommand<PointSetAbsoluteCommand>(reparsed);

        Assert.IsNotNull(pointCmd, "PointSetAbsoluteCommand not found after reparse");
        Assert.AreEqual(0.375f, pointCmd.Points[0].X, Tolerance);
        Assert.AreEqual(0.25f, pointCmd.Points[0].Y, Tolerance);
    }

    [TestMethod]
    public void PointSetAbsolute_MultipleVertices_Reparse()
    {
        // Multiple separate point commands in sequence — use exactly representable values
        var format = NaplpsFormat.New();

        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.125f, 0.25f));
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.75f, 0.625f));

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var pointCmds = FindAllCommands<PointSetAbsoluteCommand>(reparsed);
        Assert.AreEqual(2, pointCmds.Count, "Should have 2 PointSetAbsolute commands");

        Assert.AreEqual(0.125f, pointCmds[0].Points[0].X, Tolerance);
        Assert.AreEqual(0.25f, pointCmds[0].Points[0].Y, Tolerance);

        Assert.AreEqual(0.75f, pointCmds[1].Points[0].X, Tolerance);
        Assert.AreEqual(0.625f, pointCmds[1].Points[0].Y, Tolerance);
    }

    #endregion

    #region Serialize/Reparse Round-Trip — Lines

    [TestMethod]
    public void BuildLine_Serialize_Reparse_Roundtrips()
    {
        var format = NaplpsFormat.New();

        var (moveOp, moveOps) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.25f, 0.25f);
        format.AddCommand(moveOp, moveOps);

        var (lineOp, lineOps) = NaplpsCommandBuilder.BuildLineAbsolute(0.75f, 0.5f);
        format.AddCommand(lineOp, lineOps);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var lineCmd = FindCommand<LineAbsoluteCommand>(reparsed);

        Assert.IsNotNull(lineCmd, "LineAbsoluteCommand not found after reparse");
        Assert.IsTrue(lineCmd.Points.Count >= 2, $"Expected at least 2 points, got {lineCmd.Points.Count}");
        Assert.AreEqual(0.75f, lineCmd.Points[1].X, Tolerance);
        Assert.AreEqual(0.5f, lineCmd.Points[1].Y, Tolerance);
    }

    [TestMethod]
    public void LineSetAbsolute_TwoPoints_Serialize_Reparse_Roundtrips()
    {
        var format = NaplpsFormat.New();

        var points = new[]
        {
            new Vector3(0.25f, 0.25f, 0),
            new Vector3(0.75f, 0.5f, 0)
        };
        var (opcode, operands) = NaplpsCommandBuilder.BuildLineSetAbsolute(points);
        format.AddCommand(opcode, operands);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var lineCmd = FindCommand<LineSetAbsoluteCommand>(reparsed);

        Assert.IsNotNull(lineCmd, "LineSetAbsoluteCommand not found after reparse");
        Assert.IsTrue(lineCmd.Points.Count >= 2, $"Expected at least 2 points, got {lineCmd.Points.Count}");

        Assert.AreEqual(0.25f, lineCmd.Points[0].X, Tolerance);
        Assert.AreEqual(0.25f, lineCmd.Points[0].Y, Tolerance);
        Assert.AreEqual(0.75f, lineCmd.Points[1].X, Tolerance);
        Assert.AreEqual(0.5f, lineCmd.Points[1].Y, Tolerance);
    }

    [TestMethod]
    public void LineSetAbsolute_ThreePoints_Serialize_Reparse_Roundtrips()
    {
        var format = NaplpsFormat.New();

        var points = new[]
        {
            new Vector3(0.125f, 0.125f, 0),
            new Vector3(0.5f, 0.625f, 0),
            new Vector3(0.875f, 0.125f, 0)
        };
        var (opcode, operands) = NaplpsCommandBuilder.BuildLineSetAbsolute(points);
        format.AddCommand(opcode, operands);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var lineCmd = FindCommand<LineSetAbsoluteCommand>(reparsed);

        Assert.IsNotNull(lineCmd, "LineSetAbsoluteCommand not found after reparse");
        Assert.IsTrue(lineCmd.Points.Count >= 3, $"Expected at least 3 points, got {lineCmd.Points.Count}");

        Assert.AreEqual(0.125f, lineCmd.Points[0].X, Tolerance);
        Assert.AreEqual(0.125f, lineCmd.Points[0].Y, Tolerance);
        Assert.AreEqual(0.5f, lineCmd.Points[1].X, Tolerance);
        Assert.AreEqual(0.625f, lineCmd.Points[1].Y, Tolerance);
        Assert.AreEqual(0.875f, lineCmd.Points[2].X, Tolerance);
        Assert.AreEqual(0.125f, lineCmd.Points[2].Y, Tolerance);
    }

    #endregion

    #region Serialize/Reparse Round-Trip — Rectangles

    [TestMethod]
    public void RectangleFilled_Serialize_Reparse_RoundTrips()
    {
        var format = NaplpsFormat.New();

        // Move pen to start position
        var (moveOp, moveOps) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.25f, 0.25f);
        format.AddCommand(moveOp, moveOps);

        // Draw a filled rectangle
        var (rectOp, rectOps) = NaplpsCommandBuilder.BuildRectangleFilled(0.5f, 0.375f);
        format.AddCommand(rectOp, rectOps);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var rectCmd = FindCommand<RectangleFilledCommand>(reparsed);

        Assert.IsNotNull(rectCmd, "RectangleFilledCommand not found after reparse");
        Assert.AreEqual(0.5f, rectCmd.Dimensions.X, Tolerance);
        Assert.AreEqual(0.375f, rectCmd.Dimensions.Y, Tolerance);
    }

    [TestMethod]
    public void RectangleOutlined_Serialize_Reparse_RoundTrips()
    {
        var format = NaplpsFormat.New();

        var (moveOp, moveOps) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.125f, 0.125f);
        format.AddCommand(moveOp, moveOps);

        var (rectOp, rectOps) = NaplpsCommandBuilder.BuildRectangleOutlined(0.25f, 0.5f);
        format.AddCommand(rectOp, rectOps);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var rectCmd = FindCommand<RectangleOutlinedCommand>(reparsed);

        Assert.IsNotNull(rectCmd, "RectangleOutlinedCommand not found after reparse");
        Assert.AreEqual(0.25f, rectCmd.Dimensions.X, Tolerance);
        Assert.AreEqual(0.5f, rectCmd.Dimensions.Y, Tolerance);
    }

    [TestMethod]
    public void RectangleFilled_SmallDimensions_Serialize_Reparse_RoundTrips()
    {
        var format = NaplpsFormat.New();

        var (moveOp, moveOps) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.0f, 0.0f);
        format.AddCommand(moveOp, moveOps);

        // Small rectangle (1/16 x 1/16)
        var (rectOp, rectOps) = NaplpsCommandBuilder.BuildRectangleFilled(0.0625f, 0.0625f);
        format.AddCommand(rectOp, rectOps);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var rectCmd = FindCommand<RectangleFilledCommand>(reparsed);

        Assert.IsNotNull(rectCmd, "RectangleFilledCommand not found after reparse");
        Assert.AreEqual(0.0625f, rectCmd.Dimensions.X, Tolerance);
        Assert.AreEqual(0.0625f, rectCmd.Dimensions.Y, Tolerance);
    }

    #endregion

    #region Serialize/Reparse Round-Trip — SelectColor

    [TestMethod]
    public void SelectColor_Foreground_RoundTrips()
    {
        var format = NaplpsFormat.New();
        var (opcode, operands) = NaplpsCommandBuilder.BuildSelectColor(8);
        format.AddCommand(opcode, operands);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var colorCmd = FindCommand<SelectColorCommand>(reparsed);

        Assert.IsNotNull(colorCmd, "SelectColorCommand not found after reparse");
        Assert.AreEqual(1, colorCmd.State.ColorMode);
        Assert.AreEqual(8, colorCmd.State.ColorMapForeground);
    }

    [TestMethod]
    public void SelectColor_AllForegroundIndices_RoundTrip()
    {
        for (byte index = 0; index < 16; index++)
        {
            var format = NaplpsFormat.New();
            var (opcode, operands) = NaplpsCommandBuilder.BuildSelectColor(index);
            format.AddCommand(opcode, operands);

            var bytes = format.ToBytes();
            var reparsed = NaplpsFormat.FromBytes(bytes);

            var colorCmd = FindCommand<SelectColorCommand>(reparsed);

            Assert.IsNotNull(colorCmd, $"SelectColorCommand not found for index {index}");
            Assert.AreEqual(1, colorCmd.State.ColorMode, $"Color mode wrong for index {index}");
            Assert.AreEqual(index, colorCmd.State.ColorMapForeground, $"Foreground index wrong for index {index}");
        }
    }

    [TestMethod]
    public void SelectColor_ForegroundBackground_RoundTrips()
    {
        var format = NaplpsFormat.New();
        var (opcode, operands) = NaplpsCommandBuilder.BuildSelectColor(10, 3);
        format.AddCommand(opcode, operands);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var colorCmd = FindCommand<SelectColorCommand>(reparsed);

        Assert.IsNotNull(colorCmd, "SelectColorCommand not found after reparse");
        Assert.AreEqual(2, colorCmd.State.ColorMode, "Expected color mode 2 for fg+bg");
        Assert.AreEqual(10, colorCmd.State.ColorMapForeground);
        Assert.AreEqual(3, colorCmd.State.ColorMapBackground);
    }

    [TestMethod]
    public void SelectColor_ForegroundBackground_VariousCombinations_RoundTrip()
    {
        byte[][] combos = [[0, 15], [7, 8], [1, 14], [15, 0]];

        foreach (var combo in combos)
        {
            byte fg = combo[0], bg = combo[1];
            var format = NaplpsFormat.New();
            var (opcode, operands) = NaplpsCommandBuilder.BuildSelectColor(fg, bg);
            format.AddCommand(opcode, operands);

            var bytes = format.ToBytes();
            var reparsed = NaplpsFormat.FromBytes(bytes);

            var colorCmd = FindCommand<SelectColorCommand>(reparsed);

            Assert.IsNotNull(colorCmd, $"SelectColorCommand not found for fg={fg}, bg={bg}");
            Assert.AreEqual(2, colorCmd.State.ColorMode, $"fg={fg}, bg={bg}: color mode wrong");
            Assert.AreEqual(fg, colorCmd.State.ColorMapForeground, $"fg={fg}, bg={bg}: fg wrong");
            Assert.AreEqual(bg, colorCmd.State.ColorMapBackground, $"fg={fg}, bg={bg}: bg wrong");
        }
    }

    #endregion

    #region Serialize/Reparse — Multi-Command Sequences

    [TestMethod]
    public void MultiCommand_ColorThenLine_Serialize_Reparse()
    {
        var format = NaplpsFormat.New();

        // Set color
        var (colorOp, colorOps) = NaplpsCommandBuilder.BuildSelectColor(12);
        format.AddCommand(colorOp, colorOps);

        // Move pen
        var (moveOp, moveOps) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.125f, 0.125f);
        format.AddCommand(moveOp, moveOps);

        // Draw line
        var (lineOp, lineOps) = NaplpsCommandBuilder.BuildLineAbsolute(0.875f, 0.625f);
        format.AddCommand(lineOp, lineOps);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var colorCmd = FindCommand<SelectColorCommand>(reparsed);
        var pointCmd = FindCommand<PointSetAbsoluteCommand>(reparsed);
        var lineCmd = FindCommand<LineAbsoluteCommand>(reparsed);

        Assert.IsNotNull(colorCmd, "SelectColorCommand missing");
        Assert.IsNotNull(pointCmd, "PointSetAbsoluteCommand missing");
        Assert.IsNotNull(lineCmd, "LineAbsoluteCommand missing");

        Assert.AreEqual(12, colorCmd.State.ColorMapForeground);
        Assert.AreEqual(0.125f, pointCmd.Points[0].X, Tolerance);
        Assert.AreEqual(0.875f, lineCmd.Points[1].X, Tolerance);
    }

    [TestMethod]
    public void MultiCommand_ColorThenRectangle_Serialize_Reparse()
    {
        var format = NaplpsFormat.New();

        var (colorOp, colorOps) = NaplpsCommandBuilder.BuildSelectColor(5);
        format.AddCommand(colorOp, colorOps);

        var (moveOp, moveOps) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.25f, 0.25f);
        format.AddCommand(moveOp, moveOps);

        var (rectOp, rectOps) = NaplpsCommandBuilder.BuildRectangleFilled(0.5f, 0.25f);
        format.AddCommand(rectOp, rectOps);

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var colorCmd = FindCommand<SelectColorCommand>(reparsed);
        var rectCmd = FindCommand<RectangleFilledCommand>(reparsed);

        Assert.IsNotNull(colorCmd, "SelectColorCommand missing");
        Assert.IsNotNull(rectCmd, "RectangleFilledCommand missing");

        Assert.AreEqual(5, colorCmd.State.ColorMapForeground);
        Assert.AreEqual(0.5f, rectCmd.Dimensions.X, Tolerance);
        Assert.AreEqual(0.25f, rectCmd.Dimensions.Y, Tolerance);
    }

    [TestMethod]
    public void MultiCommand_TwoLines_Serialize_Reparse()
    {
        var format = NaplpsFormat.New();

        // First line
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.0f, 0.0f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));

        // Second line
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.0f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.0f, 0.5f));

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        var lineCmds = FindAllCommands<LineAbsoluteCommand>(reparsed);
        Assert.AreEqual(2, lineCmds.Count, "Should have 2 LineAbsolute commands");
    }

    [TestMethod]
    public void MultiCommand_MixedShapes_Serialize_Reparse()
    {
        var format = NaplpsFormat.New();

        // Color
        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, NaplpsEncoder.EncodeSelectColorForeground(9));

        // Rectangle
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.1f, 0.1f));
        format.AddCommand(NaplpsCommandBuilder.OpRectangleFilled, NaplpsEncoder.EncodeVertex2D(0.3f, 0.2f));

        // Line
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.75f, 0.625f));

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        Assert.IsNotNull(FindCommand<SelectColorCommand>(reparsed), "SelectColor missing");
        Assert.IsNotNull(FindCommand<RectangleFilledCommand>(reparsed), "RectangleFilled missing");
        Assert.IsNotNull(FindCommand<LineAbsoluteCommand>(reparsed), "LineAbsolute missing");

        var pointCmds = FindAllCommands<PointSetAbsoluteCommand>(reparsed);
        Assert.AreEqual(2, pointCmds.Count, "Should have 2 PointSetAbsolute commands");
    }

    #endregion

    #region Format AddCommand / RemoveCommand / InsertCommand

    [TestMethod]
    public void AddCommand_IncreasesCommandCount()
    {
        var format = NaplpsFormat.New();
        var initialCount = format.Commands.Count;

        var (opcode, operands) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.5f, 0.5f);
        format.AddCommand(opcode, operands);

        Assert.AreEqual(initialCount + 1, format.Commands.Count);
    }

    [TestMethod]
    public void RemoveCommand_DecreasesCommandCount()
    {
        var format = NaplpsFormat.New();
        var (opcode, operands) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.5f, 0.5f);
        format.AddCommand(opcode, operands);

        var countAfterAdd = format.Commands.Count;
        format.RemoveCommand(countAfterAdd - 1);

        Assert.AreEqual(countAfterAdd - 1, format.Commands.Count);
    }

    [TestMethod]
    public void InsertCommand_InsertsAtCorrectPosition()
    {
        var format = NaplpsFormat.New();

        var (opcode1, operands1) = NaplpsCommandBuilder.BuildPointSetAbsolute(0.1f, 0.1f);
        format.AddCommand(opcode1, operands1);

        var countBefore = format.Commands.Count;

        var (opcode2, operands2) = NaplpsCommandBuilder.BuildLineAbsolute(0.9f, 0.9f);
        format.InsertCommand(countBefore - 1, opcode2, operands2);

        Assert.AreEqual(countBefore + 1, format.Commands.Count);
        Assert.IsInstanceOfType(format.Commands[countBefore - 1].Command, typeof(LineAbsoluteCommand));
    }

    [TestMethod]
    public void AddCommand_MultipleCommands_IncreasesCount()
    {
        var format = NaplpsFormat.New();
        var initialCount = format.Commands.Count;

        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.1f, 0.1f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));
        format.AddCommand(NaplpsCommandBuilder.OpRectangleFilled, NaplpsEncoder.EncodeVertex2D(0.2f, 0.2f));

        Assert.AreEqual(initialCount + 3, format.Commands.Count);
    }

    [TestMethod]
    public void RemoveCommand_InvalidIndex_NoOp()
    {
        var format = NaplpsFormat.New();
        var count = format.Commands.Count;

        // Out of range — should silently do nothing
        format.RemoveCommand(-1);
        Assert.AreEqual(count, format.Commands.Count);

        format.RemoveCommand(count + 100);
        Assert.AreEqual(count, format.Commands.Count);
    }

    [TestMethod]
    public void RemoveCommand_FirstCommand_ShiftsOthers()
    {
        var format = NaplpsFormat.New();

        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.1f, 0.1f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));

        var lastCommand = format.Commands[^1].Command;
        var countBefore = format.Commands.Count;

        // Remove the point command (third item: Cancel=0, NSR=1, Point=2)
        format.RemoveCommand(2);

        Assert.AreEqual(countBefore - 1, format.Commands.Count);
        // Last command should still be the line
        Assert.AreSame(lastCommand, format.Commands[^1].Command);
    }

    [TestMethod]
    public void InsertCommand_AtBeginning_ShiftsExisting()
    {
        var format = NaplpsFormat.New();
        var initialCount = format.Commands.Count;

        format.InsertCommand(0, NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));

        Assert.AreEqual(initialCount + 1, format.Commands.Count);
        Assert.IsInstanceOfType(format.Commands[0].Command, typeof(PointSetAbsoluteCommand));
    }

    [TestMethod]
    public void InsertCommand_AtEnd_SameAsAdd()
    {
        var format = NaplpsFormat.New();

        format.InsertCommand(format.Commands.Count, NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));

        Assert.IsInstanceOfType(format.Commands[^1].Command, typeof(LineAbsoluteCommand));
    }

    [TestMethod]
    public void AddCommand_CreatesCorrectCommandType()
    {
        var format = NaplpsFormat.New();

        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));
        Assert.IsInstanceOfType(format.Commands[^1].Command, typeof(PointSetAbsoluteCommand));

        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));
        Assert.IsInstanceOfType(format.Commands[^1].Command, typeof(LineAbsoluteCommand));

        format.AddCommand(NaplpsCommandBuilder.OpRectangleFilled, NaplpsEncoder.EncodeVertex2D(0.2f, 0.2f));
        Assert.IsInstanceOfType(format.Commands[^1].Command, typeof(RectangleFilledCommand));

        format.AddCommand(NaplpsCommandBuilder.OpRectangleOutlined, NaplpsEncoder.EncodeVertex2D(0.2f, 0.2f));
        Assert.IsInstanceOfType(format.Commands[^1].Command, typeof(RectangleOutlinedCommand));

        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, NaplpsEncoder.EncodeSelectColorForeground(5));
        Assert.IsInstanceOfType(format.Commands[^1].Command, typeof(SelectColorCommand));
    }

    #endregion

    #region NaplpsFormat.New() Initial State

    [TestMethod]
    public void FormatNew_HasTwoInitialCommands()
    {
        // New() adds Cancel + NonSelectiveReset
        var format = NaplpsFormat.New();

        Assert.AreEqual(2, format.Commands.Count);
    }

    [TestMethod]
    public void FormatNew_FirstCommandIsControlCommand()
    {
        var format = NaplpsFormat.New();

        Assert.IsInstanceOfType(format.Commands[0].Command, typeof(ControlCommand));
    }

    [TestMethod]
    public void FormatNew_SecondCommandIsControlCommand()
    {
        var format = NaplpsFormat.New();

        Assert.IsInstanceOfType(format.Commands[1].Command, typeof(ControlCommand));
    }

    [TestMethod]
    public void FormatNew_StateIsInitialized()
    {
        var format = NaplpsFormat.New();

        Assert.IsNotNull(format.State);
        Assert.IsNotNull(format.State.InUseTable);
    }

    #endregion

    #region ToBytes / FromBytes Identity

    [TestMethod]
    public void ToBytes_EmptyNewFormat_ProducesNonEmptyBytes()
    {
        var format = NaplpsFormat.New();
        var bytes = format.ToBytes();

        // Cancel (0x18) + NSR (0x1F) = at least 2 bytes
        Assert.IsTrue(bytes.Length >= 2, $"Expected at least 2 bytes, got {bytes.Length}");
    }

    [TestMethod]
    public void ToBytes_FromBytes_PreservesCommandCount()
    {
        var format = NaplpsFormat.New();
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.75f, 0.25f));

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        // Exact count may differ due to parser behavior, but PDI commands should be preserved
        var originalPdiCount = format.Commands.Count(s => s.Command is GeometricDrawingCommandBase);
        var reparsedPdiCount = reparsed.Commands.Count(s => s.Command is GeometricDrawingCommandBase);

        Assert.AreEqual(originalPdiCount, reparsedPdiCount, "PDI command count should be preserved through reparse");
    }

    [TestMethod]
    public void ToBytes_FromBytes_TwoCycles_ProducesSameBytes()
    {
        var format = NaplpsFormat.New();
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.25f, 0.375f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.75f, 0.5f));

        var bytes1 = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes1);
        var bytes2 = reparsed.ToBytes();

        CollectionAssert.AreEqual(bytes1, bytes2, "Two serialize/reparse cycles should produce identical bytes");
    }

    [TestMethod]
    public void ToBytes_FromBytes_WithSelectColor_TwoCycles_ProducesSameBytes()
    {
        var format = NaplpsFormat.New();
        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, NaplpsEncoder.EncodeSelectColorForeground(7));
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));
        format.AddCommand(NaplpsCommandBuilder.OpRectangleFilled, NaplpsEncoder.EncodeVertex2D(0.25f, 0.125f));

        var bytes1 = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes1);
        var bytes2 = reparsed.ToBytes();

        CollectionAssert.AreEqual(bytes1, bytes2, "Two cycles with SelectColor should produce identical bytes");
    }

    [TestMethod]
    public void ToBytes_IncludesOpcodeAndOperands()
    {
        var format = NaplpsFormat.New();
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));

        var bytes = format.ToBytes();

        // Should contain the opcode 0xA4 somewhere after the initial control commands
        Assert.IsTrue(bytes.Contains(NaplpsCommandBuilder.OpPointSetAbsolute),
            "Serialized bytes should contain the PointSetAbsolute opcode 0xA4");
    }

    #endregion

    #region Command Builder — Coordinate Preservation Through AddCommand

    [TestMethod]
    public void AddCommand_PointSetAbsolute_SetsCorrectPenPosition()
    {
        var format = NaplpsFormat.New();

        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.625f, 0.375f));

        // After AddCommand, the state's pen should reflect the point
        Assert.AreEqual(0.625f, format.State.Pen.X, Tolerance);
        Assert.AreEqual(0.375f, format.State.Pen.Y, Tolerance);
    }

    [TestMethod]
    public void AddCommand_SelectColor_SetsColorMode()
    {
        var format = NaplpsFormat.New();

        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, NaplpsEncoder.EncodeSelectColorForeground(10));

        Assert.AreEqual(1, format.State.ColorMode);
        Assert.AreEqual(10, format.State.ColorMapForeground);
    }

    [TestMethod]
    public void AddCommand_SelectColorFgBg_SetsColorMode2()
    {
        var format = NaplpsFormat.New();

        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, NaplpsEncoder.EncodeSelectColorForegroundBackground(7, 3));

        Assert.AreEqual(2, format.State.ColorMode);
        Assert.AreEqual(7, format.State.ColorMapForeground);
        Assert.AreEqual(3, format.State.ColorMapBackground);
    }

    [TestMethod]
    public void AddCommand_RectangleFilled_MovesPen()
    {
        var format = NaplpsFormat.New();

        // Move pen to 0.25, 0.25
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.25f, 0.25f));

        // Draw rectangle 0.5 wide, 0.375 tall — pen moves right by width
        format.AddCommand(NaplpsCommandBuilder.OpRectangleFilled, NaplpsEncoder.EncodeVertex2D(0.5f, 0.375f));

        // Rectangle advances pen by width in X
        Assert.AreEqual(0.75f, format.State.Pen.X, Tolerance);
    }

    #endregion

    #region Sequence State Snapshots

    [TestMethod]
    public void AddCommand_SequenceState_IsPreExecutionClone()
    {
        var format = NaplpsFormat.New();

        // Before adding the point, pen is at origin
        var penBefore = format.State.Pen;

        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));

        var lastSequence = format.Commands[^1];

        // The sequence state should be a pre-execution clone (pen at origin)
        Assert.AreEqual(penBefore.X, lastSequence.State.Pen.X, Tolerance,
            "Sequence state should have pen position from BEFORE the command executed");
    }

    [TestMethod]
    public void AddCommand_CommandState_IsPostExecution()
    {
        var format = NaplpsFormat.New();

        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));

        var command = format.Commands[^1].Command;

        // The command's State is the live state (shared with format) which has been modified
        Assert.AreEqual(0.5f, command.State.Pen.X, Tolerance,
            "Command state should have pen position AFTER the command executed");
    }

    #endregion

    #region Edge Cases — Format Operations

    [TestMethod]
    public void AddCommand_WithNullOperands_UsesEmptyOperands()
    {
        var format = NaplpsFormat.New();
        var countBefore = format.Commands.Count;

        // SelectColor with null operands → color mode 0
        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, null);

        Assert.AreEqual(countBefore + 1, format.Commands.Count);
        Assert.AreEqual(0, format.State.ColorMode, "SelectColor with no operands should set color mode 0");
    }

    [TestMethod]
    public void FromBytes_EmptyArray_ProducesEmptyCommands()
    {
        var reparsed = NaplpsFormat.FromBytes([]);

        Assert.AreEqual(0, reparsed.Commands.Count);
    }

    [TestMethod]
    public void ToBytes_FromBytes_ComplexSequence_PreservesAllTypes()
    {
        var format = NaplpsFormat.New();

        // Build a complex sequence
        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, NaplpsEncoder.EncodeSelectColorForeground(14));
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.1f, 0.1f));
        format.AddCommand(NaplpsCommandBuilder.OpRectangleOutlined, NaplpsEncoder.EncodeVertex2D(0.3f, 0.2f));
        format.AddCommand(NaplpsCommandBuilder.OpSelectColor, NaplpsEncoder.EncodeSelectColorForeground(10));
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f));
        format.AddCommand(NaplpsCommandBuilder.OpLineAbsolute, NaplpsEncoder.EncodeVertex2D(0.9f, 0.1f));
        format.AddCommand(NaplpsCommandBuilder.OpPointSetAbsolute, NaplpsEncoder.EncodeVertex2D(0.2f, 0.6f));
        format.AddCommand(NaplpsCommandBuilder.OpRectangleFilled, NaplpsEncoder.EncodeVertex2D(0.15f, 0.1f));

        var bytes = format.ToBytes();
        var reparsed = NaplpsFormat.FromBytes(bytes);

        Assert.AreEqual(2, FindAllCommands<SelectColorCommand>(reparsed).Count, "Should have 2 SelectColor commands");
        Assert.AreEqual(3, FindAllCommands<PointSetAbsoluteCommand>(reparsed).Count, "Should have 3 PointSetAbsolute commands");
        Assert.AreEqual(1, FindAllCommands<RectangleOutlinedCommand>(reparsed).Count, "Should have 1 RectangleOutlined command");
        Assert.AreEqual(1, FindAllCommands<RectangleFilledCommand>(reparsed).Count, "Should have 1 RectangleFilled command");
        Assert.AreEqual(1, FindAllCommands<LineAbsoluteCommand>(reparsed).Count, "Should have 1 LineAbsolute command");
    }

    #endregion

    #region Builder Opcode Constants

    [TestMethod]
    public void OpcodeConstants_AreInPDIRange()
    {
        // All PDI opcodes should be in the 0xA0-0xBF range (GeneralPDISet)
        Assert.IsTrue(NaplpsCommandBuilder.OpPointSetAbsolute >= 0xA0 && NaplpsCommandBuilder.OpPointSetAbsolute <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpPointSetRelative >= 0xA0 && NaplpsCommandBuilder.OpPointSetRelative <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpPointAbsolute >= 0xA0 && NaplpsCommandBuilder.OpPointAbsolute <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpPointRelative >= 0xA0 && NaplpsCommandBuilder.OpPointRelative <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpLineAbsolute >= 0xA0 && NaplpsCommandBuilder.OpLineAbsolute <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpLineRelative >= 0xA0 && NaplpsCommandBuilder.OpLineRelative <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpLineSetAbsolute >= 0xA0 && NaplpsCommandBuilder.OpLineSetAbsolute <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpLineSetRelative >= 0xA0 && NaplpsCommandBuilder.OpLineSetRelative <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpRectangleOutlined >= 0xA0 && NaplpsCommandBuilder.OpRectangleOutlined <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpRectangleFilled >= 0xA0 && NaplpsCommandBuilder.OpRectangleFilled <= 0xBF);
        Assert.IsTrue(NaplpsCommandBuilder.OpSelectColor >= 0xA0 && NaplpsCommandBuilder.OpSelectColor <= 0xBF);
    }

    [TestMethod]
    public void OpcodeConstants_MatchExpectedValues()
    {
        Assert.AreEqual(0xA4, NaplpsCommandBuilder.OpPointSetAbsolute);
        Assert.AreEqual(0xA5, NaplpsCommandBuilder.OpPointSetRelative);
        Assert.AreEqual(0xA6, NaplpsCommandBuilder.OpPointAbsolute);
        Assert.AreEqual(0xA7, NaplpsCommandBuilder.OpPointRelative);
        Assert.AreEqual(0xA8, NaplpsCommandBuilder.OpLineAbsolute);
        Assert.AreEqual(0xA9, NaplpsCommandBuilder.OpLineRelative);
        Assert.AreEqual(0xAA, NaplpsCommandBuilder.OpLineSetAbsolute);
        Assert.AreEqual(0xAB, NaplpsCommandBuilder.OpLineSetRelative);
        Assert.AreEqual(0xB0, NaplpsCommandBuilder.OpRectangleOutlined);
        Assert.AreEqual(0xB1, NaplpsCommandBuilder.OpRectangleFilled);
        Assert.AreEqual(0xBE, NaplpsCommandBuilder.OpSelectColor);
    }

    #endregion

    #region Helpers

    private static T? FindCommand<T>(NaplpsFormat format) where T : NaplpsCommand
    {
        foreach (var seq in format.Commands)
        {
            if (seq.Command is T cmd)
            {
                return cmd;
            }
        }

        return null;
    }

    private static List<T> FindAllCommands<T>(NaplpsFormat format) where T : NaplpsCommand
    {
        var results = new List<T>();

        foreach (var seq in format.Commands)
        {
            if (seq.Command is T cmd)
            {
                results.Add(cmd);
            }
        }

        return results;
    }

    #endregion
}
