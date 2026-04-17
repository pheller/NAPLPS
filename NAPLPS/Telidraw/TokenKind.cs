// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Telidraw;

/// <summary>
/// Token categories produced by <see cref="Lexer"/>. Keyword tokens have specific kinds
/// rather than a generic "keyword" + lexeme so the parser can switch cleanly; reserved-word
/// matches happen in the lexer against a static dictionary.
/// </summary>
public enum TokenKind
{
    // Literals
    Identifier,
    IntLiteral,
    FloatLiteral,
    FractionLiteral,   // e.g. 1/40 (two integers separated by /)
    StringLiteral,

    // Top-level structure / declaration keywords
    Proc,              // proc name(params) { ... }
    With,              // with color 3 { ... }
    Repeat,            // repeat N { ... }
    For,               // for i in 1..10 { ... }
    In,                // "in" inside for-loops
    If,                // if cond { ... }
    Else,
    Palette,           // palette cyan = 1 (aliases a name to a palette index)
    Let,               // let x = <expr>

    // Turtle / drawing verbs
    Forward,
    Back,
    Turn,
    Move,              // absolute position set (PointSetAbsolute)
    MoveRel,           // relative position set (PointSetRelative)
    Goto,              // alias for move
    Line,              // absolute line
    LineRel,           // relative line
    Rect,              // filled rectangle
    RectOutline,       // outlined rectangle
    Arc,               // filled arc (default)
    ArcOutline,        // outlined arc
    Polygon,           // filled polygon (default)
    PolyOutline,       // outlined polygon
    Point,             // absolute point (no pen move)
    PointRel,          // relative point (no pen move)
    Text,
    Color,             // SelectColor (pick from palette)
    SetColor,          // SetColor (define palette entry RGB)
    Nsr,               // Non-Selective Reset (clear screen + reset most state)
    Texture,
    Domain,
    Blink,
    Wait,
    Reset,
    Drcs,
    Field,
    Scribble,          // IncrementalLine shortcut
    Bitmap,            // IncrementalPoint shortcut
    Close,             // close current polygon path
    Raw,               // raw 0xOP HH HH ... (pass-through opcode + operand bytes)

    // Operators / punctuation
    Equals,            // =
    Plus,
    Minus,
    Star,
    Slash,
    Percent,
    LParen,
    RParen,
    LBrace,
    RBrace,
    LBracket,
    RBracket,
    Comma,
    Colon,
    Semicolon,
    DotDot,            // 1..10 range operator
    At,                // @macro attribute prefix

    // Directives and end-markers
    Directive,         // #coord pixels  (lexeme stores the full directive name)
    Newline,           // optional statement separator; parser usually ignores
    Eof,
}
