# NAPLPS Byte-Stream Quickref

Derived from ANSI X3.110-1983 + the BYTE Magazine 1983 series (Feb-May, see `docs/`). For the authoritative spec, read `naplps_standard_-_ansi_x3.110-1983_smaller.pdf`.

## Code structure

NAPLPS is a 7-bit ISO 2022-style code structure with 4 sets (G0/G1/G2/G3) and 2 in-use areas (GL = 0x20-0x7F, GR = 0xA0-0xFF). Default invocation:

| Set | Designation default | Invocation default |
|-----|---------------------|--------------------|
| G0  | Primary character set (ASCII)           | GL |
| G1  | General PDI set (drawing commands)      | GR |
| G2  | Supplementary character set (Latin-1ish) | (none — invoke via SS2) |
| G3  | Mosaic set (block graphics)             | (none — invoke via SS3 or LS3) |

## C0 control codes (0x00-0x1F)

| Hex  | Mnemonic | Meaning                                         |
|------|----------|-------------------------------------------------|
| 0x08 | BS       | Backspace (active position backward)           |
| 0x09 | HT       | Tab (active position forward)                  |
| 0x0A | LF       | Line feed (active position down)               |
| 0x0B | VT       | Active position up                             |
| 0x0C | FF       | Form feed / clear screen                       |
| 0x0D | CR       | Carriage return                                |
| 0x0E | SO       | Locking shift G1 → GL                          |
| 0x0F | SI       | Locking shift G0 → GL                          |
| 0x18 | CAN      | Cancel                                         |
| 0x19 | SS2      | Single shift G2 (next byte uses G2)            |
| 0x1A | SDC      | Service delimiter character                    |
| 0x1B | ESC      | Escape (G-set designation, locking shifts)     |
| 0x1C | APS      | Active position set (cursor row/col)           |
| 0x1D | SS3      | Single shift G3 (next byte uses G3)            |
| 0x1E | APH      | Active position home                           |
| 0x1F | NSR      | Non-selective reset (+ optional row/col bytes) |

## PDI commands (in G1, default at GR = 0xA0-0xBF)

| Hex  | Command                       | Operands                               |
|------|-------------------------------|----------------------------------------|
| 0xA0 | Reset                         | 2 fixed-format bytes                   |
| 0xA1 | Domain                        | 1 fixed byte + optional pel xy         |
| 0xA2 | Text                          | 1 fixed byte + optional rotation/path/spacing |
| 0xA3 | Texture                       | 1 fixed byte + optional mask size      |
| 0xA4 | Point Set Absolute            | mv-byte vertex                         |
| 0xA5 | Point Set Relative            | mv-byte vertex (signed delta)          |
| 0xA6 | Point Absolute                | mv-byte vertex (no pen move)           |
| 0xA7 | Point Relative                | mv-byte vertex                         |
| 0xA8 | Line Absolute                 | mv-byte vertex                         |
| 0xA9 | Line Relative                 | mv-byte vertex                         |
| 0xAA | Line Set Absolute             | N × mv-byte vertices                   |
| 0xAB | Line Set Relative             | N × mv-byte deltas                     |
| 0xAC | Arc Outlined                  | 2 × mv-byte (mid rel, end rel)         |
| 0xAD | Arc Filled                    | 2 × mv-byte                            |
| 0xAE | Arc Set Outlined              | 3 × mv-byte (start abs + mid + end)    |
| 0xAF | Arc Set Filled                | 3 × mv-byte                            |
| 0xB0 | Rectangle Outlined            | mv-byte (w, h) from pen                |
| 0xB1 | Rectangle Filled              | mv-byte (w, h)                         |
| 0xB2 | Rectangle Set Outlined        | mv-byte (x, y) + mv-byte (w, h)        |
| 0xB3 | Rectangle Set Filled          | mv-byte (x, y) + mv-byte (w, h)        |
| 0xB4 | Polygon Outlined              | N × mv-byte rel vertices               |
| 0xB5 | Polygon Filled                | N × mv-byte rel vertices               |
| 0xB6 | Polygon Set Outlined          | mv-byte abs start + N × rel deltas     |
| 0xB7 | Polygon Set Filled            | mv-byte abs start + N × rel deltas     |
| 0xB8 | Incremental Field             | optional mv-byte origin + mv-byte dim  |
| 0xB9 | Incremental Point             | bits-per-pixel + N pixel values        |
| 0xBA | Incremental Line              | step deltas + motion codes             |
| 0xBB | Incremental Polygon Filled    | step deltas + motion codes             |
| 0xBC | Set Color                     | N triplets (G/R/B bits)                |
| 0xBD | Wait                          | tenths-of-second bytes                 |
| 0xBE | Select Color                  | palette index in operand bits          |
| 0xBF | Blink                         | toIndex + (on/off/delay) processes     |

## C1 control codes (0x80-0x9F, accessed via ESC 0x40-0x5F)

| Hex  | Mnemonic | Meaning                                |
|------|----------|----------------------------------------|
| 0x80 | DefMacro | Begin macro definition                 |
| 0x81 | DefPMacro| Begin macro + immediate execute        |
| 0x82 | DefTMacro| Begin transmit macro                   |
| 0x83 | DefDRCS  | Begin DRCS character definition        |
| 0x84 | DefTexture| Begin texture mask definition         |
| 0x85 | End      | End buffered definition                |
| 0x86 | Repeat   | Repeat next primitive N times          |
| 0x88 | RV       | Reverse video on                       |
| 0x89 | NV       | Normal video                           |
| 0x8A-0x8F | Text size | Small/Med/Normal/DoubleH/DoubleSize |
| 0x95-0x96 | WW On/Off | Word-wrap mode                  |
| 0x97-0x98 | Scroll On/Off | Scroll mode                 |
| 0x99-0x9A | Underline On/Off |                          |
| 0x9E | BlinkStop|                                        |
| 0x9F | NSR      | (alias of 0x1F when in 8-bit C1 area)  |

## Operand encoding

**Numerical data** bytes carry 6 data bits in positions b1-b6. The high bits (b7, b8) are framing — `0x40` base in 7-bit transmission, `0xC0` base in 8-bit. `IsValidNumericalDataNext` peeks the next byte; if it falls in the numerical-data range of the current InUseTable, ReadOperands consumes it.

**Vertex2D** packs X bits in b6-b4 and Y bits in b3-b1 of each operand byte. `multiByteValue=3` ⇒ 9 bits per axis (1/512 precision) ⇒ 3 operand bytes per vertex.

**Sign convention** for relative deltas: bit b6 of the first operand byte is the sign for X; bit b3 is the sign for Y.

## In-use table

`NaplpsState.InUseTable[256]` maps every byte 0x00-0xFF to a `NaplpsCommandReference` describing what to do with it. Locking shifts (SO, SI, LS2, LS3, ESC + 6/4-6/15) repopulate GL/GR halves from the current G-set designations.

## Round-trip invariants enforced by `NAPLPSTests/`

1. Every example file in `Examples/` (375 total): `bytes(F) == bytes(FromFile(F).ToBytes())`
2. Every example: `bytes(F) == bytes(compile(decompile(FromFile(F))).ToBytes())`
3. Every command class: `parse(BuildXxx(...).bytes) ⟹ same operand values`
4. Visual regression: APNG of canvas matches `NAPLPSTests/Visual/Baselines/<name>.apng`
