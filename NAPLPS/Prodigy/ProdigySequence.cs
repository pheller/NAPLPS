// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Text.RegularExpressions;

namespace NAPLPS.Prodigy;

/// <summary>
/// Detects multi-page Prodigy file groups by common filename prefix.
/// For example: COKE1-COKE8 → sequence "COKE" with 8 pages.
/// </summary>
public static partial class ProdigySequence
{
    public record FileGroup(string Prefix, List<string> Files);

    /// <summary>
    /// Scans a directory for Prodigy file groups. Groups files that share
    /// a common alphabetic prefix followed by digits (e.g., COKE1, COKE2).
    /// Only includes groups with 2+ files.
    /// </summary>
    public static List<FileGroup> DetectSequences(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return [];

        var files = Directory.GetFiles(directoryPath)
            .Select(System.IO.Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>()
            .OrderBy(f => f)
            .ToList();

        var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var match = PrefixPattern().Match(file);
            if (match.Success)
            {
                var prefix = match.Groups[1].Value;
                if (!groups.ContainsKey(prefix))
                    groups[prefix] = [];
                groups[prefix].Add(file);
            }
        }

        return groups
            .Where(kvp => kvp.Value.Count >= 2)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => new FileGroup(kvp.Key, kvp.Value))
            .ToList();
    }

    [GeneratedRegex(@"^([A-Za-z]+)\d+", RegexOptions.Compiled)]
    private static partial Regex PrefixPattern();
}
