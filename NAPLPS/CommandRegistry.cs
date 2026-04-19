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
        // Case-insensitive comparison — Telidraw accepts both `DOMAIN 1 3 2` and
        // `domain 1 3 2` as the same raw-byte form. The decompiler emits uppercase
        // canonically to avoid collision with lowercase high-level keywords.
        foreach (var d in _instance.Value.ByType.Values)
        {
            var nameKebab = d.Name.Replace(' ', '-');
            if (string.Equals(nameKebab, kebabName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(d.DslKeyword, kebabName, StringComparison.OrdinalIgnoreCase))
            {
                // Prefer 8-bit opcode (>= 0xA0) when both 7-bit and 8-bit variants exist.
                foreach (var op in d.DefaultOpcodes) { if (op >= 0xA0) { return op; } }
                if (d.DefaultOpcodes.Count > 0) { return d.DefaultOpcodes[0]; }
            }
        }
        return 0;
    }

    /// <summary>
    /// ANSI X3.110 per-opcode mnemonic names (NUL, SOH, ..., CAN, ESC, APS, NSR).
    /// These ARE the Telidraw keywords for raw-byte commands: a line like `NSR 127 79`
    /// emits opcode 0x1F + operands [127, 79]. Uppercase matches the spec + SequenceWindow.
    /// </summary>
    public static readonly Dictionary<byte, string> OpcodeMnemonics = new()
    {
        [0x00] = "NUL", [0x01] = "SOH", [0x02] = "STX", [0x03] = "ETX",
        [0x04] = "EOT", [0x05] = "ENQ", [0x06] = "ACK", [0x07] = "BEL",
        [0x08] = "APB", [0x09] = "APF", [0x0A] = "APD", [0x0B] = "APU",
        [0x0C] = "CS",  [0x0D] = "APR", [0x0E] = "SO",  [0x0F] = "SI",
        [0x10] = "DLE", [0x11] = "DC1", [0x12] = "DC2", [0x13] = "DC3",
        [0x14] = "DC4", [0x15] = "NAK", [0x16] = "SYN", [0x17] = "ETB",
        [0x18] = "CAN", [0x19] = "SS2", [0x1A] = "SDC", [0x1B] = "ESC",
        [0x1C] = "APS", [0x1D] = "SS3", [0x1E] = "APH", [0x1F] = "NSR",
    };

    private static readonly Dictionary<string, byte> _mnemonicLookup =
        OpcodeMnemonics.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>Resolve a case-sensitive ANSI mnemonic (NSR/CAN/...) to its opcode.</summary>
    public static bool TryResolveMnemonic(string mnemonic, out byte opcode) =>
        _mnemonicLookup.TryGetValue(mnemonic, out opcode);

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

        // AOT: iterate the hand-maintained static type list instead of Assembly.GetTypes()
        // so the trimmer can prove every command subclass is reachable. New commands need
        // to be added to CommandRegistryKnownTypes.All — the build succeeds without the
        // entry, but parsing will fall back to the generic NaplpsCommand for that opcode.
        foreach (var type in CommandRegistryKnownTypes.All)
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
