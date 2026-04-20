# Telidraw Language Reference ✍️✨

Telidraw (`.td` files) is the canonical human-editable text format for NAPLPS scenes. Every `.td` file compiles to a `.nap` byte stream; every `.nap` decompiles back to a `.td` that recompiles byte-identical.

## Directives 🧭

```td
#coord fractions          // X in [0,1], Y in [0,0.75]  (default)
#coord pixels             // X,Y in pixel units relative to #resolution
#resolution 640 480       // pixel domain for #coord pixels mode
#bits 7                   // emit 7-bit (0x40-base) opcodes (rare)
#bits 8                   // emit 8-bit (0xC0-base) opcodes (default)
```

## Comments

```td
// single-line
/* multi-line block */
```

## Variables and expressions

```td
let x = 0.5
let w = 1/8                  // fraction literal
let center = (x + 0.1) * 2   // arithmetic
```

## Drawing verbs 🎨

| Keyword                | Emits                  | Args                              |
|------------------------|------------------------|-----------------------------------|
| `move x y`             | PointSetAbsolute       | absolute pen position             |
| `move-rel dx dy`       | PointSetRelative       | delta from pen                    |
| `point x y`            | PointAbsolute          | dot at coords (no pen move)       |
| `point-rel dx dy`      | PointRelative          | dot at pen+delta                  |
| `line x y`             | LineAbsolute           | line to absolute coord            |
| `line-rel dx dy`       | LineRelative           | line to pen+delta                 |
| `line-set x1 y1 x2 y2 ...` | LineSetAbsolute    | N absolute points                 |
| `line-set-rel dx1 dy1 ...` | LineSetRelative    | N relative deltas                 |
| `rect w h`             | RectangleFilled        | filled rect from pen, size w×h    |
| `rect-outline w h`     | RectangleOutlined      | outlined rect                     |
| `rect-set x y w h`     | RectangleSetFilled     | filled rect at absolute origin    |
| `rect-set-outline x y w h` | RectangleSetOutlined |                                 |
| `arc mx my ex ey`      | ArcFilled              | filled arc through mid + end      |
| `arc-outline mx my ex ey` | ArcOutlined         | outlined arc                      |
| `arc-set sx sy mx my ex ey` | ArcSetFilled      | absolute start + mid + end        |
| `arc-set abs sx sy dmx dmy dex dey` | ArcSetFilled | exact: start abs + relative deltas |
| `polygon x1 y1 x2 y2 ...` | PolygonFilled       | filled polygon, vertices relative |
| `polygon-outline x1 y1 ...` | PolygonOutlined   |                                  |
| `polygon-set sx sy v1x v1y ...` | PolygonSetFilled | absolute start, then absolute verts |
| `polygon-set abs sx sy dx1 dy1 ...` | PolygonSetFilled | exact form: start abs + relative tail |
| `text "..."`           | AsciiCharCommand seq   | ASCII string                      |

## Attributes 🎛️

| Keyword                       | Notes                                              |
|-------------------------------|----------------------------------------------------|
| `color N`                     | SelectColor: palette index (0-15)                  |
| `color FG BG`                 | SelectColor with bg                                |
| `set-color G R B`             | Define palette entry RGB (3-bit each, 0-7)         |
| `texture line highlight fill` | TEXTURE: line pattern (0-3), highlight bool, fill pattern (0-3) |
| `domain sv mv [dim]`          | DOMAIN: single-byte/multi-byte/dimensionality      |
| `wait n`                      | WAIT: n tenths of a second                         |
| `blink toIdx onTenths offTenths [delay]` | BLINK process                       |
| `field`                       | IncrementalField: full screen                      |
| `field x y w h`               | IncrementalField with bounds                       |
| `reset`                       | RESET (selective)                                  |
| `nsr`                         | Non-Selective Reset                                |

## Block structure

```td
proc square(size) {
  repeat 4 {
    forward size
    turn 90
  }
}

with color 5 {
  move 0.1 0.1
  square 0.2
}                  // color restored to whatever it was before the `with`
```

```td
for i in 0..7 {
  color i
  move (i / 8) 0.5
  rect (1/8) 0.1
}
```

## Raw fallback

When the decompiler can't find a high-level form (rare 7-bit edge cases, mosaic data, DRCS bitmap bytes), it emits the byte stream verbatim:

```td
raw 0x37 64 90 100 ...   // PolygonSetFilled with N raw operand bytes
```

This is **always** byte-exact. The `// CommandName` comment is for human reading; the compiler ignores it.

## Quick example

```td
// hello.td: minimal NAPLPS scene
#coord fractions

domain 1 3 2
color 7
move 0.1 0.6
text "Hello, NAPLPS!"

with color 1 {
  move 0.1 0.5
  rect 0.8 0.05
}

reset
```

## Byte-fidelity guarantee ✅

For any `.nap` file F:
```
decompile(F) → T   then   compile(T) → F'   ⟹   bytes(F) == bytes(F')
```

Tested across all 375 files in `Examples/` on every commit. See `NAPLPSTests/Telidraw/ExamplesRoundTripTests.cs`.
