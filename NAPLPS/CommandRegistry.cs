// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Reflection;

namespace NAPLPS;

public sealed record CommandDescriptor(
    Type CommandType,
    string Name,
    string Description,
    int UiHeight,
    CommandCategory Category,
    string DslKeyword,
    IReadOnlyList<byte> DefaultOpcodes);

/// <summary>
/// Reflection-based registry of all NaplpsCommand subclasses decorated with
/// [AddCommand]. Built once on first access. Opcode mapping is derived from
/// NaplpsState's default NCR tables (C0, C1, PrimaryCharacterSet, GeneralPDISet)
/// at their default GLeft/GRight positions.
/// </summary>
public static class CommandRegistry
{
    private static readonly Lazy<Registry> _instance = new(Build);

    private sealed class Registry
    {
        public required Dictionary<Type, CommandDescriptor> ByType { get; init; }
        public required Dictionary<byte, CommandDescriptor> ByOpcode { get; init; }
        public required Dictionary<string, CommandDescriptor> ByKeyword { get; init; }
    }

    public static CommandDescriptor? GetByType(Type type)
    {
        return _instance.Value.ByType.TryGetValue(type, out var descriptor) ? descriptor : null;
    }

    public static CommandDescriptor? GetByOpcode(byte opcode)
    {
        return _instance.Value.ByOpcode.TryGetValue(opcode, out var descriptor) ? descriptor : null;
    }

    public static CommandDescriptor? GetByKeyword(string keyword)
    {
        return _instance.Value.ByKeyword.TryGetValue(keyword, out var descriptor) ? descriptor : null;
    }

    /// <summary>
    /// Look up the highest-priority opcode for a kebab-cased command name (e.g.
    /// `polygon-set-filled`). The decompiler emits this form for raw fallbacks; the
    /// compiler reads it back here. Returns 0 (Null opcode) when the name doesn't match
    /// any known command — caller should treat that as a parse error or pass-through.
    /// </summary>
    public static byte GetOpcodeByKebabName(string kebabName)
    {
        // Match against the descriptor's display name lowercased+kebab'd, OR the DslKeyword.
        // Only return a result when the name UNIQUELY identifies one opcode — multi-opcode
        // classes like ControlCommand/NumericalDataCommand/AsciiCharCommand share a name
        // across many opcodes so the kebab name isn't authoritative. For those, the caller
        // should use the explicit opcode-byte form instead.
        foreach (var d in _instance.Value.ByType.Values)
        {
            var nameKebab = d.Name.ToLowerInvariant().Replace(' ', '-');
            if (nameKebab == kebabName || d.DslKeyword == kebabName)
            {
                if (d.DefaultOpcodes.Count != 1) { return 0; }
                return d.DefaultOpcodes[0];
            }
        }
        return 0;
    }

    public static IEnumerable<CommandDescriptor> All => _instance.Value.ByType.Values;

    public static IEnumerable<CommandDescriptor> ByCategory(CommandCategory category)
    {
        return _instance.Value.ByType.Values.Where(d => d.Category == category);
    }

    private static Registry Build()
    {
        var opcodeTable = BuildDefaultOpcodeTable();

        var byType = new Dictionary<Type, CommandDescriptor>();
        var byKeyword = new Dictionary<string, CommandDescriptor>(StringComparer.OrdinalIgnoreCase);
        var byOpcode = new Dictionary<byte, CommandDescriptor>();

        var assembly = typeof(NaplpsCommand).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<AddCommandAttribute>();

            if (attr == null)
            {
                continue;
            }

            var opcodes = opcodeTable.TryGetValue(type, out var list) ? list.AsReadOnly() : (IReadOnlyList<byte>)Array.Empty<byte>();

            var descriptor = new CommandDescriptor(
                type,
                attr.Name,
                attr.Description,
                attr.Height,
                attr.Category,
                attr.DslKeyword,
                opcodes);

            byType[type] = descriptor;

            if (!string.IsNullOrEmpty(attr.DslKeyword))
            {
                byKeyword[attr.DslKeyword] = descriptor;
            }

            foreach (var opcode in opcodes)
            {
                byOpcode[opcode] = descriptor;
            }
        }

        return new Registry
        {
            ByType = byType,
            ByOpcode = byOpcode,
            ByKeyword = byKeyword,
        };
    }

    private static Dictionary<Type, List<byte>> BuildDefaultOpcodeTable()
    {
        var table = new Dictionary<Type, List<byte>>();

        AddRange(table, NaplpsState.C0Set, 0x00);
        AddRange(table, NaplpsState.PrimaryCharacterSet, 0x20);
        AddRange(table, NaplpsState.C1Set, 0x80);
        AddRange(table, NaplpsState.GeneralPDISet, 0xA0);

        return table;
    }

    private static void AddRange(Dictionary<Type, List<byte>> table, NaplpsCommandReference[] set, int baseOpcode)
    {
        for (int i = 0; i < set.Length; i++)
        {
            var reference = set[i];

            if (reference?.CommandType == null)
            {
                continue;
            }

            var opcode = (byte)(baseOpcode + i);

            if (!table.TryGetValue(reference.CommandType, out var list))
            {
                list = [];
                table[reference.CommandType] = list;
            }

            list.Add(opcode);
        }
    }
}
