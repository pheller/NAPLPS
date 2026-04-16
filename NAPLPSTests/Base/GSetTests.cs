// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Base;

/// <summary>
/// Tests for ANSI X3.110-1983 §4.3.2 / §4.3.3 code extension: G-set designation
/// (ESC I F), locking shifts (SI, SO, LS2, LS3, LS*R), and single shifts (SS2, SS3).
/// </summary>
[TestClass]
public class GSetTests
{
    [TestMethod]
    public void DefaultState_G0IsPrimaryInvokedToGL()
    {
        var state = new NaplpsState();

        // Default: G0 = Primary character set, invoked into GL (positions 0x20-0x7F).
        // Position 0x41 is 'A' in PrimaryCharacterSet.
        var ncr = state.InUseTable[0x41];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(AsciiCharCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DefaultState_G1IsGeneralPdiInvokedToGR()
    {
        var state = new NaplpsState();

        // Default: G1 = General PDI set, invoked into GR (positions 0xA0-0xFF).
        // Position 0xA0 should be ResetCommand (PDI opcode 0).
        var ncr = state.InUseTable[0xA0];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(ResetCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoLockingShiftTwo_PutsG2IntoGL()
    {
        var state = new NaplpsState();

        state.DoLockingShiftTwo();

        // After LS2, G2 (Supplementary) is invoked into GL.
        // Supplementary at offset 0x21 is '\u00A1' (¡).
        var ncr = state.InUseTable[0x21];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(AsciiCharCommand), ncr.CommandType);
        Assert.AreEqual('\u00A1', (char)ncr.Parameters[0]);
    }

    [TestMethod]
    public void DoLockingShiftThree_PutsG3IntoGL()
    {
        var state = new NaplpsState();

        state.DoLockingShiftThree();

        // After LS3, G3 (Mosaic) is invoked into GL.
        var ncr = state.InUseTable[0x21];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(MosaicElementCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoShiftIn_RestoresG0ToGL()
    {
        var state = new NaplpsState();

        state.DoLockingShiftTwo();
        state.DoShiftIn();

        // After SI, G0 (Primary) is invoked into GL.
        var ncr = state.InUseTable[0x41];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(AsciiCharCommand), ncr.CommandType);
        Assert.AreEqual('A', (char)ncr.Parameters[0]);
    }

    [TestMethod]
    public void DoShiftOut_PutsG1IntoGL()
    {
        var state = new NaplpsState();

        state.DoShiftOut();

        // After SO, G1 (General PDI) is invoked into GL.
        // Position 0x20 should now be ResetCommand (PDI opcode 0).
        var ncr = state.InUseTable[0x20];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(ResetCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoLockingShiftTwoRight_PutsG2IntoGR()
    {
        var state = new NaplpsState();

        state.DoLockingShiftTwoRight();

        // After LS2R, G2 (Supplementary) is invoked into GR.
        var ncr = state.InUseTable[0xA1];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(AsciiCharCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoLockingShiftThreeRight_PutsG3IntoGR()
    {
        var state = new NaplpsState();

        state.DoLockingShiftThreeRight();

        // After LS3R, G3 (Mosaic) is invoked into GR.
        var ncr = state.InUseTable[0xA0];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(MosaicElementCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoEscape_LS2_AsSingleByteSequence_InvokesG2()
    {
        var state = new NaplpsState();

        // ESC 6/14 = LS2
        var handled = state.DoEscape(new NaplpsOperands([0x6E]));

        Assert.IsTrue(handled);

        var ncr = state.InUseTable[0x21];
        Assert.IsNotNull(ncr);
        Assert.AreEqual('\u00A1', (char)ncr.Parameters[0]);
    }

    [TestMethod]
    public void DoEscape_LS3_AsSingleByteSequence_InvokesG3()
    {
        var state = new NaplpsState();

        // ESC 6/15 = LS3
        var handled = state.DoEscape(new NaplpsOperands([0x6F]));

        Assert.IsTrue(handled);

        var ncr = state.InUseTable[0x21];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(MosaicElementCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoEscape_LS3R_AsSingleByteSequence_InvokesG3IntoGR()
    {
        var state = new NaplpsState();

        // ESC 7/12 = LS3R
        var handled = state.DoEscape(new NaplpsOperands([0x7C]));

        Assert.IsTrue(handled);

        var ncr = state.InUseTable[0xA0];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(MosaicElementCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoEscape_DesignatePrimaryToG0_RebuildsGL()
    {
        var state = new NaplpsState();

        // ESC 2/8 4/2 = designate Primary as G0 (no-op since already default, but verify it parses)
        var handled = state.DoEscape(new NaplpsOperands([0x28, 0x42]));

        Assert.IsTrue(handled);

        var ncr = state.InUseTable[0x41];
        Assert.IsNotNull(ncr);
        Assert.AreEqual('A', (char)ncr.Parameters[0]);
    }

    [TestMethod]
    public void DoEscape_DesignateMosaicToG1_RebuildsGR()
    {
        var state = new NaplpsState();

        // ESC 2/13 7/13 = designate Mosaic (96-char) as G1
        var handled = state.DoEscape(new NaplpsOperands([0x2D, 0x7D]));

        Assert.IsTrue(handled);

        // G1 is invoked into GR by default — so 0xA0 should now resolve to MosaicElement.
        var ncr = state.InUseTable[0xA0];
        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(MosaicElementCommand), ncr.CommandType);
    }

    [TestMethod]
    public void DoSingleShiftTwo_SetsPendingShift()
    {
        var state = new NaplpsState();
        Assert.IsNull(state.PendingSingleShift);

        state.DoSingleShiftTwo();

        Assert.AreEqual(NaplpsState.GsetSlot.G2, state.PendingSingleShift);
    }

    [TestMethod]
    public void ResolveByte_ConsumesPendingShiftAfterOneByte()
    {
        var state = new NaplpsState();

        state.DoSingleShiftTwo();
        Assert.IsNotNull(state.PendingSingleShift);

        // The resolved byte should come from G2 (Supplementary), not GL's normal G0.
        var ncr = state.ResolveByte(0x21);
        Assert.IsNotNull(ncr);
        Assert.AreEqual('\u00A1', (char)ncr.Parameters[0]);

        // Pending shift should be consumed.
        Assert.IsNull(state.PendingSingleShift);

        // Subsequent byte should resolve normally (G0).
        var ncr2 = state.ResolveByte(0x21);
        Assert.IsNotNull(ncr2);
        Assert.AreEqual('!', (char)ncr2.Parameters[0]);
    }

    [TestMethod]
    public void ResolveByte_NoShift_FallsThroughToInUseTable()
    {
        var state = new NaplpsState();

        var ncr = state.ResolveByte(0x41);
        Assert.IsNotNull(ncr);
        Assert.AreEqual('A', (char)ncr.Parameters[0]);
    }

    [TestMethod]
    public void DoSingleShiftThree_PendingShiftFiresAgainstG3()
    {
        var state = new NaplpsState();

        state.DoSingleShiftThree();
        var ncr = state.ResolveByte(0x21);

        Assert.IsNotNull(ncr);
        Assert.AreEqual(typeof(MosaicElementCommand), ncr.CommandType);
        Assert.IsNull(state.PendingSingleShift);
    }

    [TestMethod]
    public void MosaicElement_SixBoolCtor_DispatchesViaActivator()
    {
        // Regression: the NCR table stores 6 bits as 6 separate params, not a single bool[6].
        // The 6-bool ctor is the one Activator picks. Confirm it doesn't throw and packs bits.
        var state = new NaplpsState();
        var ops = new NaplpsOperands();

        var cmd = new MosaicElementCommand(true, false, true, false, true, false, state, 0x21, ops);

        Assert.IsTrue(cmd.Bit1);
        Assert.IsFalse(cmd.Bit2);
        Assert.IsTrue(cmd.Bit3);
        Assert.IsFalse(cmd.Bit4);
        Assert.IsTrue(cmd.Bit5);
        Assert.IsFalse(cmd.Bit6);
    }

    [TestMethod]
    public void ResetState_RestoresDefaultDesignations()
    {
        var state = new NaplpsState();

        state.DoLockingShiftThree();
        // Verify it took effect
        Assert.AreEqual(typeof(MosaicElementCommand), state.InUseTable[0x21]?.CommandType);

        state.Reset();
        // Back to G0=Primary in GL
        Assert.AreEqual('A', (char)state.InUseTable[0x41]!.Parameters[0]);
    }
}
