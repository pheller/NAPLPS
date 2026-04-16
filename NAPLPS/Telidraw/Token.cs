// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Telidraw;

/// <summary>
/// One token produced by <see cref="Lexer"/>. Line/Column are 1-based for human-readable
/// error messages. Lexeme is the raw source text (useful for identifiers, string contents
/// after quote-stripping, and the numerator of a FractionLiteral). For FractionLiteral,
/// <see cref="SecondValue"/> carries the denominator.
/// </summary>
public readonly record struct Token(
    TokenKind Kind,
    string Lexeme,
    int Line,
    int Column,
    double Number = 0,
    int SecondValue = 0)
{
    public override string ToString()
    {
        return Kind switch
        {
            TokenKind.Identifier => $"Ident({Lexeme})",
            TokenKind.IntLiteral => $"Int({(long)Number})",
            TokenKind.FloatLiteral => $"Float({Number})",
            TokenKind.FractionLiteral => $"Fraction({(int)Number}/{SecondValue})",
            TokenKind.StringLiteral => $"Str(\"{Lexeme}\")",
            TokenKind.Directive => $"#{Lexeme}",
            _ => Kind.ToString(),
        };
    }
}
