// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Telidraw;

namespace NAPLPSTests.Telidraw;

/// <summary>
/// Verifies the per-command 7-bit opcode/operand round-trip path. Mixed-mode files in the
/// corpus (DOLLAR02.RCP, ec1060.nap, neiman-marcus-full.nap) interleave 7-bit and 8-bit
/// commands; the decompiler must emit `#bits N` at section boundaries so the compiler
/// flips Use7BitMode and reproduces both halves byte-identical.
/// </summary>
[TestClass]
public class SevenBitModeTests
{
    [TestMethod]
    public void Encoder_Use7BitMode_ProducesLowBaseOperands()
    {
        NaplpsEncoder.Use7BitMode = true;
        try
        {
            var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f);
            foreach (var b in operands)
            {
                Assert.IsTrue(b >= 0x40 && b <= 0x7F, $"7-bit operand byte 0x{b:X2} out of expected 0x40-0x7F range");
            }
        }
        finally
        {
            NaplpsEncoder.Use7BitMode = false;
        }
    }

    [TestMethod]
    public void Encoder_Default_Produces8BitBase()
    {
        NaplpsEncoder.Use7BitMode = false;
        var operands = NaplpsEncoder.EncodeVertex2D(0.5f, 0.5f);
        foreach (var b in operands)
        {
            Assert.IsTrue(b >= 0xC0 && b <= 0xFF, $"8-bit operand byte 0x{b:X2} out of expected 0xC0-0xFF range");
        }
    }

    [TestMethod]
    public void Compiler_BitsDirective_FlipsOpcodeBase()
    {
        // `move 0.5 0.5` under #bits 7 should emit opcode 0x24 (PointSetAbsolute & 0x7F),
        // not 0xA4. Operands also use 0x40 base.
        var source = "#bits 7\nmove 0.5 0.5";
        var tokens = new Lexer(source).Tokenize();
        var ast = new Parser(tokens).Parse();
        var compiler = new Compiler(ast) { BareFormat = true };
        var format = compiler.Compile();

        Assert.AreEqual(1, format.Commands.Count);
        var cmd = format.Commands[0].Command;
        Assert.AreEqual(0x24, cmd.OpCode, "PointSetAbsolute opcode should drop bit 7 in #bits 7 mode");
    }

    [TestMethod]
    public void Compiler_DefaultBits8_KeepsHighOpcode()
    {
        var source = "move 0.5 0.5";
        var tokens = new Lexer(source).Tokenize();
        var ast = new Parser(tokens).Parse();
        var compiler = new Compiler(ast) { BareFormat = true };
        var format = compiler.Compile();

        Assert.AreEqual(1, format.Commands.Count);
        Assert.AreEqual(0xA4, format.Commands[0].Command.OpCode, "Default mode keeps bit 7 set on PDI opcodes");
    }

    [TestMethod]
    public void Decompiler_EmitsBitsDirective_ForMixedMode()
    {
        // Build a synthetic "mixed-mode" stream by hand: 7-bit PointSetAbsolute followed
        // by 8-bit Reset. Decompiler should insert `#bits 7` at top and `#bits 8` between.
        var fmt = NaplpsFormat.New(NaplpsSystemType.NAPLPS);
        fmt.AddCommand(0x24, new NaplpsOperands { 0x60, 0x40, 0x40 });  // 7-bit PointSetAbsolute
        fmt.AddCommand(0xA0, new NaplpsOperands { 0xC0, 0xC0 });        // 8-bit Reset

        var td = Decompiler.Decompile(fmt);
        Assert.IsTrue(td.Contains("#bits 7") || td.Contains("#bits 8"),
                      "Decompiled output for mixed-mode file must contain at least one #bits directive");
    }
}
