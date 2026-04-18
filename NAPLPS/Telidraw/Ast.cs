// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Telidraw;

/// <summary>
/// Base record for every Telidraw AST node. Carries source position so diagnostics in
/// later phases (compile, decompile) can refer back to the originating tokens.
/// </summary>
public abstract record AstNode(int Line, int Column);

// ---- Top-level -----------------------------------------------------------

public sealed record ProgramNode(IReadOnlyList<DirectiveNode> Directives, IReadOnlyList<StatementNode> Statements, int Line = 1, int Column = 1) : AstNode(Line, Column);

// ---- Statements ---------------------------------------------------------

public abstract record StatementNode(int Line, int Column) : AstNode(Line, Column);

/// <summary>
/// A <c>#coord pixels</c>-style directive. Typically hoisted to the program header, but
/// also permitted mid-body so compile-time toggles (e.g. coord mode) can fire after a
/// section of fractions has been emitted.
/// </summary>
public sealed record DirectiveNode(string Name, IReadOnlyList<ExpressionNode> Args, int Line, int Column) : StatementNode(Line, Column);

/// <summary>A command invocation like `move 0.5 0.5`, `forward 100`, `rect 0.3 0.2`.</summary>
public sealed record CommandCallNode(TokenKind Command, IReadOnlyList<ExpressionNode> Args, int Line, int Column) : StatementNode(Line, Column);

/// <summary>A user procedure call like `star 0.2 0.8 0.1`.</summary>
public sealed record ProcCallNode(string Name, IReadOnlyList<ExpressionNode> Args, int Line, int Column) : StatementNode(Line, Column);

/// <summary>`with color 3 { ... }` or `with texture hatched { ... }`.</summary>
public sealed record WithBlockNode(TokenKind Attribute, IReadOnlyList<ExpressionNode> AttributeArgs, IReadOnlyList<StatementNode> Body, int Line, int Column) : StatementNode(Line, Column);

/// <summary>`repeat N { ... }` — unrolled at compile time.</summary>
public sealed record RepeatNode(ExpressionNode Count, IReadOnlyList<StatementNode> Body, int Line, int Column) : StatementNode(Line, Column);

/// <summary>`for i in A..B { ... }` — unrolled at compile time.</summary>
public sealed record ForNode(string Variable, ExpressionNode From, ExpressionNode To, IReadOnlyList<StatementNode> Body, int Line, int Column) : StatementNode(Line, Column);

/// <summary>`if cond { ... } else { ... }` — evaluated at compile time.</summary>
public sealed record IfNode(ExpressionNode Condition, IReadOnlyList<StatementNode> Then, IReadOnlyList<StatementNode>? Else, int Line, int Column) : StatementNode(Line, Column);

/// <summary>`proc name(p1, p2, ...) { body }`, with optional `@macro` attribute on the previous token.</summary>
public sealed record ProcDeclNode(string Name, IReadOnlyList<string> Parameters, IReadOnlyList<StatementNode> Body, bool AsMacro, int Line, int Column) : StatementNode(Line, Column);

/// <summary>`palette cyan = 1` — creates an identifier-to-integer alias in the compiler's symbol table.</summary>
public sealed record PaletteAliasNode(string Name, ExpressionNode Value, int Line, int Column) : StatementNode(Line, Column);

/// <summary>`let x = expr` — scoped variable binding used inside procs and blocks.</summary>
public sealed record LetNode(string Name, ExpressionNode Value, int Line, int Column) : StatementNode(Line, Column);

/// <summary>
/// `raw 0xA4 C0 D2 C0` — pass-through: emit the opcode byte and operand bytes verbatim.
/// Used by the decompiler for any command that doesn't have a higher-level DSL mapping,
/// guaranteeing lossless round-trip for every NAPLPS byte stream.
/// </summary>
public sealed record RawStatementNode(IReadOnlyList<byte> Bytes, int Line, int Column, bool IsLogicalForm = false) : StatementNode(Line, Column);

// ---- Expressions --------------------------------------------------------

public abstract record ExpressionNode(int Line, int Column) : AstNode(Line, Column);

public sealed record NumberLiteralNode(double Value, int Line, int Column) : ExpressionNode(Line, Column);

public sealed record FractionLiteralNode(int Numerator, int Denominator, int Line, int Column) : ExpressionNode(Line, Column)
{
    public double AsDouble => (double)Numerator / Denominator;
}

public sealed record StringLiteralNode(string Value, int Line, int Column) : ExpressionNode(Line, Column);

public sealed record IdentifierNode(string Name, int Line, int Column) : ExpressionNode(Line, Column);

public sealed record BinaryOpNode(string Op, ExpressionNode Left, ExpressionNode Right, int Line, int Column) : ExpressionNode(Line, Column);

public sealed record UnaryOpNode(string Op, ExpressionNode Operand, int Line, int Column) : ExpressionNode(Line, Column);

public sealed record CallExpressionNode(string Name, IReadOnlyList<ExpressionNode> Args, int Line, int Column) : ExpressionNode(Line, Column);
