// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.File;

/// <summary>
/// The parser serialization invariant: every byte the parser consumes must land in some
/// command's opcode or operands, so FromBytes(x).ToBytes() reproduces x.
/// </summary>
[TestClass]
public class ParserRoundTripTests
{
    private static string ExamplesDir => Path.Combine(AppContext.BaseDirectory, "examples");

    // X3.110 inherits ISO 2022 code extension: in a 7-bit environment a C1 control is coded
    // ESC Fe. END is C1 8/5, i.e. ESC 4/5 (1B 45). A definition closed by the two-byte form
    // must re-serialize both bytes.
    [TestMethod]
    public void MacroDefinition_EscCodedEnd_RoundTrips()
    {
        byte[] stream =
        [
            0x1B, 0x40, 0x60,   // ESC 4/0 (DEF MACRO) + macro name
            0x20, 0x41, 0x42,   // body bytes (buffered, not executed)
            0x1B, 0x45,         // ESC 4/5 (END)
            0x41,               // trailing text after the definition
        ];

        var format = NaplpsFormat.FromBytes(stream);
        CollectionAssert.AreEqual(stream, format.ToBytes());
        AssertEscCodedEnd(format);
    }

    // Proves the buffered-definition path actually ran: the parser injected an END control
    // command carrying the ESC form's final byte as its operand (opcode 1B + operand 45).
    private static void AssertEscCodedEnd(NaplpsFormat format)
    {
        var end = format.Commands
            .Select(s => s.Command)
            .OfType<ControlCommand>()
            .SingleOrDefault(c => c.Command == NaplpsControlCommands.End);

        Assert.IsNotNull(end, "no END control command was injected - definition mode never entered");
        Assert.AreEqual(0x1B, end.OpCode);
        CollectionAssert.AreEqual(new byte[] { 0x45 }, end.Operands.ToArray());
    }
}
