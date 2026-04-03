using NAPLPS;
using NAPLPS.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IOFile = System.IO.File;

namespace NAPLPSTests.Diagnostics;

[TestClass]
public class LinePosDiag
{
    [TestMethod]
    public void DumpAroundUnderlines()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "examples", "1.nap");
        var naplps = NaplpsFormat.FromFile(path);
        int idx = 0;
        var lines = new System.Collections.Generic.List<string>();

        foreach (var (command, state) in naplps.Commands)
        {
            if (idx >= 735 && idx <= 755)
            {
                var cmdType = command.GetType().Name;
                var extra = "";

                if (command is GeometricDrawingCommandBase geo)
                {
                    extra = $" pts=[{string.Join(", ", geo.Points.Select(p => $"({p.X:F6},{p.Y:F6})"))}]";
                    extra += $" verts=[{string.Join(", ", geo.Vertices.Select(v => $"({v.X:F6},{v.Y:F6})"))}]";
                    extra += $" pel=({geo.LogicalPel.X:F6},{geo.LogicalPel.Y:F6})";
                }

                if (command is AsciiCharCommand ac)
                {
                    extra = $" '{ac.AsciiCharacter}'";
                }

                lines.Add($"{idx,4} | {cmdType,-30} | pen=({state.Pen.X:F6},{state.Pen.Y:F6}) | ops=[{string.Join(" ", command.Operands.Select(b => $"0x{b:X2}"))}]{extra}");
            }

            idx++;
        }

        var outputPath = Path.Combine(AppContext.BaseDirectory, "1nap_linepos_diag.txt");
        IOFile.WriteAllLines(outputPath, lines);
    }
}
