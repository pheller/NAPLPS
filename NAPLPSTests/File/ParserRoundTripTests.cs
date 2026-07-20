// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.File;

/// <summary>
/// The parser serialization invariant: every byte the parser consumes must land in some
/// command's opcode or operands, so FromBytes(x).ToBytes() reproduces x. Guards against
/// consuming bytes without preserving them (the ESC-coded END final byte, macro invocation
/// bytes) and against splicing parser-materialized output into the serialized stream
/// (macro expansions, DEFP MACRO define-and-display execution).
/// </summary>
[TestClass]
public class ParserRoundTripTests
{
    private static string ExamplesDir => Path.Combine(AppContext.BaseDirectory, "examples");

    private static readonly string[] SkipExtensions = [".jpg", ".png", ".txt", ".exe"];

    [TestMethod]
    [TestCategory("RoundTrip")]
    public void FromBytesToBytes_AllExamples_ByteExact()
    {
        if (!Directory.Exists(ExamplesDir))
        {
            Assert.Inconclusive($"Examples directory not found: {ExamplesDir}");
            return;
        }

        var failures = new List<string>();
        var checkedCount = 0;

        var files = Directory.GetFiles(ExamplesDir, "*", SearchOption.AllDirectories)
            .Where(f => !SkipExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f);

        foreach (var file in files)
        {
            var raw = System.IO.File.ReadAllBytes(file);
            var format = NaplpsFormat.FromBytes(raw);

            if (format.IsErrored)
            {
                // Files with parse errors are another suite's concern.
                continue;
            }

            checkedCount++;
            var reEmitted = format.ToBytes();

            if (!raw.SequenceEqual(reEmitted))
            {
                failures.Add($"{Path.GetRelativePath(ExamplesDir, file)}: {raw.Length}B -> {reEmitted.Length}B");
            }
        }

        Assert.AreNotEqual(0, checkedCount, "no corpus files checked");
        Assert.AreEqual(0, failures.Count,
            "parser round-trip mismatches:\n" + string.Join("\n", failures.Take(20)));
    }

    // Named anchors for the historically lossy files: all use the macro machinery (7-bit
    // ESC-coded END, macro invocation bytes, DEFP define-and-display). Duplicates part of the
    // corpus sweep above, but survives corpus reshuffles and names the regression precisely.
    [DataTestMethod]
    [DataRow("bre.nap")]
    [DataRow("crap1.nap")]
    [DataRow("crap2.nap")]
    [DataRow("email2.nap")]
    [DataRow("song.nap")]
    public void MacroCorpusFiles_RoundTripByteExact(string name)
    {
        var path = Path.Combine(ExamplesDir, name);
        if (!System.IO.File.Exists(path))
        {
            Assert.Inconclusive($"corpus file missing: {name}");
            return;
        }

        var raw = System.IO.File.ReadAllBytes(path);
        CollectionAssert.AreEqual(raw, NaplpsFormat.FromBytes(raw).ToBytes());
    }

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
