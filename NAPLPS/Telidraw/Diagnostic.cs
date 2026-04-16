// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Telidraw;

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error,
}

/// <summary>
/// One compile-time or parse-time diagnostic. Lexer, parser, compiler, and decompiler
/// all funnel issues through this single record type. Line/Column are 1-based.
/// </summary>
public readonly record struct Diagnostic(
    DiagnosticSeverity Severity,
    int Line,
    int Column,
    string Message,
    string? Hint = null)
{
    public override string ToString()
    {
        var severity = Severity.ToString().ToLowerInvariant();
        var hint = Hint != null ? $" ({Hint})" : string.Empty;
        return $"[{severity}] line {Line}, col {Column}: {Message}{hint}";
    }
}
