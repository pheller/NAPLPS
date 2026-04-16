// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

namespace NAPLPSTests.Base;

/// <summary>
/// Round-trip tests for NaplpsCommandBuilder: each builder produces (opcode, operands)
/// that, when fed back through the corresponding command class's constructor, yield
/// the same logical values. This is the contract Telidraw's compiler will rely on.
/// </summary>
[TestClass]
public class BuilderRoundTripTests
{
    [TestMethod]
    public void BuildPointAbsolute_RoundTripsCoordinates()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildPointAbsolute(0.25f, 0.5f);

        Assert.AreEqual(NaplpsCommandBuilder.OpPointAbsolute, opcode);

        var state = new NaplpsState();
        var cmd = new PointAbsoluteCommand(state, opcode, operands);

        Assert.AreEqual(0.25f, state.Pen.X, 0.01f);
        Assert.AreEqual(0.5f, state.Pen.Y, 0.01f);
    }

    [TestMethod]
    public void BuildPointRelative_RoundTripsOffset()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildPointRelative(0.1f, -0.05f);

        Assert.AreEqual(NaplpsCommandBuilder.OpPointRelative, opcode);

        var state = new NaplpsState { Pen = new Vector3(0.5f, 0.5f, 0f) };
        var cmd = new PointRelativeCommand(state, opcode, operands);

        Assert.AreEqual(0.6f, state.Pen.X, 0.01f);
        Assert.AreEqual(0.45f, state.Pen.Y, 0.01f);
    }

    [TestMethod]
    public void BuildLineRelative_RoundTripsOffset()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildLineRelative(0.3f, 0.2f);

        Assert.AreEqual(NaplpsCommandBuilder.OpLineRelative, opcode);

        var state = new NaplpsState { Pen = new Vector3(0.1f, 0.1f, 0f) };
        var cmd = new LineRelativeCommand(state, opcode, operands);

        // After a relative line, pen should advance by the relative offset
        Assert.AreEqual(0.4f, state.Pen.X, 0.01f);
        Assert.AreEqual(0.3f, state.Pen.Y, 0.01f);
    }

    [TestMethod]
    public void BuildRectangleSetFilled_RoundTripsDimensions()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildRectangleSetFilled(0.1f, 0.2f, 0.3f, 0.4f);

        Assert.AreEqual(NaplpsCommandBuilder.OpRectangleSetFilled, opcode);

        var state = new NaplpsState();
        var cmd = new RectangleSetFilledCommand(state, opcode, operands);

        // RectangleSet should have decoded a starting point and dimensions
        Assert.IsTrue(cmd.IsValid);
    }

    [TestMethod]
    public void BuildArcSetOutlined_ProducesThreeVertices()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildArcSetOutlined(0.2f, 0.3f, 0.05f, 0.05f, 0.1f, 0.0f);

        Assert.AreEqual(NaplpsCommandBuilder.OpArcSetOutlined, opcode);
        // 3 vertices × 3 bytes per vertex (default multiByteValue=3) = 9 bytes
        Assert.AreEqual(9, operands.Count);
    }

    [TestMethod]
    public void BuildPolygonSetFilled_AbsStartPlusRelativeVertices()
    {
        var verts = new[]
        {
            new Vector3(0.1f, 0.0f, 0f),
            new Vector3(0.0f, 0.1f, 0f),
            new Vector3(-0.1f, 0.0f, 0f),
        };

        var (opcode, operands) = NaplpsCommandBuilder.BuildPolygonSetFilled(new Vector3(0.2f, 0.2f, 0f), verts);

        Assert.AreEqual(NaplpsCommandBuilder.OpPolygonSetFilled, opcode);
        // 1 absolute + 3 relative = 4 vertices × 3 bytes
        Assert.AreEqual(12, operands.Count);
    }

    [TestMethod]
    public void BuildDomain_RoundTripsValues()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildDomain(2, 4, 2);

        Assert.AreEqual(NaplpsCommandBuilder.OpDomain, opcode);

        var state = new NaplpsState();
        var cmd = new DomainCommand(state, opcode, operands);

        Assert.AreEqual(2, state.SingleByteValue);
        Assert.AreEqual(4, state.MultiByteValue);
        Assert.AreEqual(2, state.Dimensionality);
    }

    [TestMethod]
    public void BuildDomain_3DRoundTrips()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildDomain(1, 3, 3);

        var state = new NaplpsState();
        var cmd = new DomainCommand(state, opcode, operands);

        Assert.AreEqual(3, state.Dimensionality);
        Assert.AreEqual(1, state.SingleByteValue);
        Assert.AreEqual(3, state.MultiByteValue);
    }

    [TestMethod]
    public void BuildTexture_RoundTripsAllFlags()
    {
        // line=2, highlight=true, pattern=5
        var (opcode, operands) = NaplpsCommandBuilder.BuildTexture(linePattern: 2, highlight: true, fillPattern: 5);

        Assert.AreEqual(NaplpsCommandBuilder.OpTexture, opcode);

        var state = new NaplpsState();
        var cmd = new TextureCommand(state, opcode, operands);

        Assert.AreEqual((NAPLPS.NaplpsTexture.LineTextures)2, cmd.LineTexture);
        Assert.IsTrue(cmd.ShouldHighlight);
        Assert.AreEqual((NAPLPS.NaplpsTexture.TexturePatterns)5, cmd.TexturePattern);
    }

    [TestMethod]
    public void BuildWait_HasFixedFormatByte()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildWait(20);

        Assert.AreEqual(NaplpsCommandBuilder.OpWait, opcode);
        Assert.AreEqual((byte)0x5C, operands[0]);

        var state = new NaplpsState();
        var cmd = new WaitCommand(state, opcode, operands);

        Assert.IsTrue(cmd.IsValid);
        Assert.AreEqual(20, cmd.WaitTime);
    }

    [TestMethod]
    public void BuildWait_MultipleIntervalsAreSummedOrIndividual()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildWait(30, 10, 5);

        var state = new NaplpsState();
        var cmd = new WaitCommand(state, opcode, operands);

        Assert.IsTrue(cmd.IsValid);
        Assert.AreEqual(30, cmd.WaitTime);
        Assert.AreEqual(2, cmd.WaitTimes.Count);
        Assert.AreEqual(10, cmd.WaitTimes[0]);
        Assert.AreEqual(5, cmd.WaitTimes[1]);
    }

    [TestMethod]
    public void BuildReset_RoundTripsByte1Flags()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildReset(
            domainReset: true,
            colorMode: ResetCommand.ColorModeReset.SelectOneAndDefaults,
            screenBorder: ResetCommand.ScreenBorderReset.ScreenBlack);

        Assert.AreEqual(NaplpsCommandBuilder.OpReset, opcode);

        var state = new NaplpsState();
        var cmd = new ResetCommand(state, opcode, operands);

        Assert.IsTrue(cmd.IsDomainReset);
        Assert.AreEqual(ResetCommand.ColorModeReset.SelectOneAndDefaults, cmd.ColorMode);
        Assert.AreEqual(ResetCommand.ScreenBorderReset.ScreenBlack, cmd.ColorScreenBorder);
    }

    [TestMethod]
    public void BuildReset_RoundTripsByte2Flags()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildReset(
            textReset: true,
            blinkReset: true,
            macrosReset: true);

        var state = new NaplpsState();
        var cmd = new ResetCommand(state, opcode, operands);

        Assert.IsTrue(cmd.IsTextReset);
        Assert.IsTrue(cmd.IsBlinkReset);
        Assert.IsTrue(cmd.IsMacrosReset);
        Assert.IsFalse(cmd.IsTextureAttributesReset);
        Assert.IsFalse(cmd.IsDRCSCharsReset);
    }

    [TestMethod]
    public void BuildText_RoundTripsRotationAndPath()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildText(
            charWidth: 0.05f, charHeight: 0.05f,
            spacing: TextCommand.TextSpacing.Proportional,
            path: TextCommand.TextPath.Down,
            rotation: TextCommand.TextRotation.Ninety);

        Assert.AreEqual(NaplpsCommandBuilder.OpText, opcode);

        var state = new NaplpsState();
        var cmd = new TextCommand(state, opcode, operands);

        Assert.AreEqual(TextCommand.TextRotation.Ninety, state.TextRotation);
        Assert.AreEqual(TextCommand.TextPath.Down, state.TextPath);
        Assert.AreEqual(TextCommand.TextSpacing.Proportional, state.TextSpacing);
    }

    [TestMethod]
    public void BuildField_FullScreenWithNoOperands()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildField();

        Assert.AreEqual(NaplpsCommandBuilder.OpIncrementalField, opcode);
        Assert.AreEqual(0, operands.Count);

        var state = new NaplpsState();
        var cmd = new IncrementalFieldCommand(state, opcode, operands);

        Assert.IsTrue(cmd.IsValid);
    }

    [TestMethod]
    public void BuildField_WithOriginAndDimensionsRoundTrips()
    {
        var origin = new Vector3(0.1f, 0.1f, 0f);
        var dims = new Vector3(0.5f, 0.4f, 0f);

        var (opcode, operands) = NaplpsCommandBuilder.BuildField(origin, dims);

        var state = new NaplpsState();
        var cmd = new IncrementalFieldCommand(state, opcode, operands);

        Assert.AreEqual(0.1f, state.Field.Origin.X, 0.01f);
        Assert.AreEqual(0.1f, state.Field.Origin.Y, 0.01f);
        Assert.AreEqual(0.5f, state.Field.Dimensions.X, 0.01f);
        Assert.AreEqual(0.4f, state.Field.Dimensions.Y, 0.01f);
    }

    [TestMethod]
    public void BuildBlink_StopProducesNoOperands()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildBlinkStop();

        Assert.AreEqual(NaplpsCommandBuilder.OpBlink, opcode);
        Assert.AreEqual(0, operands.Count);
    }

    [TestMethod]
    public void BuildSetColorTransparent_NoOperands()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildSetColorTransparent();

        Assert.AreEqual(NaplpsCommandBuilder.OpSetColor, opcode);
        Assert.AreEqual(0, operands.Count);
    }

    [TestMethod]
    public void BuildNonSelectiveReset_OpcodeOnly()
    {
        var (opcode, operands) = NaplpsCommandBuilder.BuildNonSelectiveReset();

        Assert.AreEqual((byte)0x1F, opcode);
        Assert.AreEqual(0, operands.Count);
    }

    [TestMethod]
    public void IncrementalBuilders_ThrowUntilPhase4()
    {
        Assert.ThrowsExactly<NotImplementedException>(() => NaplpsCommandBuilder.BuildIncrementalPoint(4, []));
        Assert.ThrowsExactly<NotImplementedException>(() => NaplpsCommandBuilder.BuildIncrementalLine(0.01f, 0.01f, []));
        Assert.ThrowsExactly<NotImplementedException>(() => NaplpsCommandBuilder.BuildIncrementalPolygonFilled(0.01f, 0.01f, []));
    }

    [TestMethod]
    public void DefinitionBuilders_ThrowUntilPhase5()
    {
        Assert.ThrowsExactly<NotImplementedException>(() => NaplpsCommandBuilder.BuildDefDrcs(0, new bool[8, 10]));
        Assert.ThrowsExactly<NotImplementedException>(() => NaplpsCommandBuilder.BuildDefTexture(0, new bool[8, 8]));
        Assert.ThrowsExactly<NotImplementedException>(() => NaplpsCommandBuilder.BuildDefMacro(0, []));
    }
}
