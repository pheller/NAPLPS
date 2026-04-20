# NAPLPS Quickref 🔖✨

Compressed normative reference: opcodes, operand layouts, in-use table rules, round-trip invariants. For the walkthrough see [naplps.md](naplps.md). For the DSL see [telidraw.md](telidraw.md). For the authoritative spec read `naplps_standard_-_ansi_x3.110-1983_smaller.pdf`.

## Code page 🧬

ISO 2022-style 256-byte code page, four regions:

| Bytes | Region | Contents |
|---|---|---|
| `0x00 – 0x1F` | C0 | 32 control codes (fixed) |
| `0x20 – 0x7F` | GL | 96-entry graphic-left, invoked from one of G0–G3 |
| `0x80 – 0x9F` | C1 | 32 control codes (fixed, NAPLPS-specific) |
| `0xA0 – 0xFF` | GR | 96-entry graphic-right, invoked from one of G0–G3 |

## G-sets (defaults) 🔠

| Slot | Default set | Default invocation |
|---|---|---|
| G0 | Primary Character Set (ASCII) | GL |
| G1 | General PDI Set (drawing commands) | GR |
| G2 | Supplementary Character Set (94 symbols) | none (single-shift via SS2) |
| G3 | Mosaic Set (2×3 block graphics) | none (SS3 / LS3) |

## C0 control codes (0x00 – 0x1F) 🎛️

| Hex | Mnemonic | Meaning |
|---|---|---|
| `0x08` | BS | Backspace (active position backward) |
| `0x09` | HT | Horizontal tab (active position forward) |
| `0x0A` | LF | Line feed (active position down) |
| `0x0B` | VT | Active position up |
| `0x0C` | FF | Form feed / clear screen |
| `0x0D` | CR | Carriage return |
| `0x0E` | SO | Locking shift: G1 → GL |
| `0x0F` | SI | Locking shift: G0 → GL |
| `0x18` | CAN | Cancel |
| `0x19` | SS2 | Single shift: next byte uses G2 |
| `0x1A` | SDC | Service delimiter character |
| `0x1B` | ESC | Escape (G-set designation, locking shifts) |
| `0x1C` | APS | Active position set (row, col) |
| `0x1D` | SS3 | Single shift: next byte uses G3 |
| `0x1E` | APH | Active position home |
| `0x1F` | NSR | Non-selective reset (+ optional row/col bytes) |

## ESC sequences 🔀

| Sequence | Mnemonic | Effect |
|---|---|---|
| `ESC 0x6E` | LS2 | Locking shift: G2 → GL |
| `ESC 0x6F` | LS3 | Locking shift: G3 → GL |
| `ESC 0x7E` | LS1R | Locking shift: G1 → GR |
| `ESC 0x7D` | LS2R | Locking shift: G2 → GR |
| `ESC 0x7C` | LS3R | Locking shift: G3 → GR |
| `ESC 0x28 F` | (designation) | Designate 94-char set into G0 (F is the set identifier) |
| `ESC 0x29 F` | (designation) | Designate into G1 |
| `ESC 0x2A F` | (designation) | Designate into G2 |
| `ESC 0x2B F` | (designation) | Designate into G3 |
| `ESC 0x40 – 0x5F` | (C1 access) | 7-bit C1 access (byte 0x80 + offset) |

Final byte ranges: `0x30 – 0x7E` (7-bit) or `0xA0 – 0xFE` (8-bit, with bit 7 set).

## PDI commands (General PDI Set at GR base 0xA0) 🎨

| Hex | Command | Operands |
|---|---|---|
| `0xA0` | Reset | 0, 1, or 2 fixed-format bytes |
| `0xA1` | Domain | 1 fixed byte (sv/mv/dim) + optional logical-pel vertex |
| `0xA2` | Text | 1–2 fixed attribute bytes |
| `0xA3` | Texture | 1 fixed byte (linePattern, highlight, fillPattern) + optional mask-size vertex |
| `0xA4` | Point Set Absolute | `mv`-byte vertex (moves pen, no draw) |
| `0xA5` | Point Set Relative | `mv`-byte relative vertex |
| `0xA6` | Point Absolute | `mv`-byte vertex (draws pixel at vertex, no pen move) |
| `0xA7` | Point Relative | `mv`-byte relative |
| `0xA8` | Line Absolute | `mv`-byte vertex (endpoint) |
| `0xA9` | Line Relative | `mv`-byte relative endpoint |
| `0xAA` | Line Set Absolute | N × `mv`-byte vertices |
| `0xAB` | Line Set Relative | N × `mv`-byte deltas |
| `0xAC` | Arc Outlined | 2 × `mv`-byte (mid-rel, end-rel) |
| `0xAD` | Arc Filled | 2 × `mv`-byte |
| `0xAE` | Arc Set Outlined | 3 × `mv`-byte (start-abs, mid-rel, end-rel) |
| `0xAF` | Arc Set Filled | 3 × `mv`-byte |
| `0xB0` | Rectangle Outlined | `mv`-byte (w, h) from pen |
| `0xB1` | Rectangle Filled | `mv`-byte (w, h) |
| `0xB2` | Rectangle Set Outlined | `mv`-byte (x, y) + `mv`-byte (w, h) |
| `0xB3` | Rectangle Set Filled | `mv`-byte (x, y) + `mv`-byte (w, h) |
| `0xB4` | Polygon Outlined | N × `mv`-byte relative vertices from pen |
| `0xB5` | Polygon Filled | N × `mv`-byte relative vertices |
| `0xB6` | Polygon Set Outlined | `mv`-byte absolute start + N × relative tail |
| `0xB7` | Polygon Set Filled | `mv`-byte absolute start + N × relative tail |
| `0xB8` | Incremental Field | 0, 1, or 2 vertices (empty = full screen) |
| `0xB9` | Incremental Point | 1 header byte (bits-per-pixel) + packed pixel data |
| `0xBA` | Incremental Line | start vertex + packed motion codes |
| `0xBB` | Incremental Polygon Filled | start vertex + packed motion codes |
| `0xBC` | Set Color | N × RGB triplets (3-bit G/R/B each), mode-dependent length |
| `0xBD` | Wait | 1 byte tenths-of-a-second (captures a frame) |
| `0xBE` | Select Color | 1 or 2 palette-index bytes depending on color mode |
| `0xBF` | Blink | 1 target-index byte + triples of (on, off, start-delay) bytes |

In 7-bit transmission these shift down to `0x20 – 0x3F`.

## C1 control codes (0x80 – 0x9F) 🎚️

Accessed directly in 8-bit transmission, or via `ESC 0x40 – 0x5F` in 7-bit.

| Hex | Mnemonic | Meaning |
|---|---|---|
| `0x80` | DEF MACRO | Begin macro definition (body until END) |
| `0x81` | DEF P-MACRO | Protected macro variant |
| `0x82` | DEF T-MACRO | Transient macro |
| `0x83` | DEF DRCS | Dynamically Redefinable Character Set entry |
| `0x84` | DEF TEXTURE | Custom texture mask |
| `0x85` | END | Terminates a DEF block |
| `0x86` | REPEAT | Repeat next character N times |
| `0x87` | REPEAT TO EOL | Repeat last character until end of line |
| `0x88` | REVERSE VIDEO | FG / BG swap |
| `0x89` | NORMAL VIDEO | Cancel reverse |
| `0x8A` | SMALL TEXT | Attribute: small text cell |
| `0x8B` | MED TEXT | Attribute: medium text cell |
| `0x8C` | NORMAL TEXT | Attribute: normal text cell |
| `0x8D` | DOUBLE HEIGHT | Attribute: double-height glyphs |
| `0x8E` | BLINK START | Attribute: begin blink range |
| `0x8F` | DOUBLE SIZE | Attribute: double-size glyphs |
| `0x90` | PROTECT | Mark field protected |
| `0x91 – 0x94` | EDC1 – EDC4 | End of Data Codes (reserved) |
| `0x95` | WORD WRAP ON | Toggle auto-wrap on |
| `0x96` | WORD WRAP OFF | Toggle auto-wrap off |
| `0x97` | SCROLL ON | Toggle scroll-on-LF on |
| `0x98` | SCROLL OFF | Toggle scroll-on-LF off |
| `0x99` | UNDERLINE START | Begin underline range |
| `0x9A` | UNDERLINE STOP | End underline range |
| `0x9B` | FLASH CURSOR | Cursor attribute: flashing |
| `0x9C` | STEADY CURSOR | Cursor attribute: steady |
| `0x9D` | CURSOR OFF | Cursor attribute: hidden |
| `0x9E` | BLINK STOP | End blink range / kill active blink processes |
| `0x9F` | UNPROTECT | Remove field protection |

## Operand encoding 🔢

**Numerical-data bytes** carry 6 data bits in bit positions 1–6. The high bits (7, 8) are framing.

| Mode | Base | Effective range | Notes |
|---|---|---|---|
| 7-bit | `0x40` | `0x40 – 0x7F` | bit 7 clear |
| 8-bit | `0xC0` | `0xC0 – 0xFF` | bit 7 set |

**Vertex2D** packs X bits in positions 6–4 and Y bits in positions 3–1 of each operand byte.

| `mv` | Bytes / vertex | Bits per axis | Precision |
|---|---|---|---|
| 2 | 2 | 6 (5 fraction + sign) | 1/32 |
| 3 | 3 | 9 (8 fraction + sign) | 1/256 |
| 4 | 4 | 12 (11 fraction + sign) | 1/2048 |

**Sign convention**: bit 6 of the first operand byte is the sign for X; bit 3 is the sign for Y. Negative values decode as `-1 + fraction`.

## Coordinate space 📐

Unit screen: `X ∈ [0, 1]`, `Y ∈ [0, 0.75]` (4:3 aspect).

Decode algorithm: for each axis, bit 0 is sign. If positive, the remaining N-1 bits are summed at weights 1/2, 1/4, …, 1/2^(N-1). If negative, the same sum is computed then `-1` is added.

## In-use table 📋

`NaplpsState.InUseTable[256]` maps every byte 0x00 – 0xFF to a `NaplpsCommandReference`. Locking shifts (SO, SI, LS2, LS3, LS1R, LS2R, LS3R) repopulate the GL and GR halves from the current G-set designations.

| Event | Effect on in-use table |
|---|---|
| SO / SI | Rebuild GL half |
| LS2 / LS3 | Rebuild GL half |
| LS1R / LS2R / LS3R | Rebuild GR half |
| SS2 / SS3 | No table change; resolves the next byte via G2/G3 directly |
| ESC designation into currently-invoked slot | Rebuild affected half |

## Round-trip invariants ✅

Enforced by `NAPLPSTests/`:

1. **Disk-honest**: for every example file F, `bytes(F) == bytes(FromFile(F).ToBytes())`.
2. **Telidraw round-trip**: `bytes(F) == bytes(compile(decompile(FromFile(F))).ToBytes())`.
3. **Command builder round-trip**: `parse(BuildXxx(...).bytes)` yields the same operand values.
4. **Visual regression**: APNG render matches `NAPLPSTests/Visual/Baselines/<name>.apng` bit-for-bit.

Full suite runs on every commit.
