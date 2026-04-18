# NAPLPS

A modern .NET 10 toolkit for the **North American Presentation Level Protocol Syntax** (NAPLPS, ANSI X3.110-1983) — the videotex / Prodigy graphics format that ran across millions of dial-up terminals in the 1980s and 90s.

This repo contains:

- **`NAPLPS/`** — the spec-compliant parser, renderer, encoder, and command-builder library. Reads and writes `.nap` byte streams; renders to ImageSharp `Image<Rgba32>`.
- **`NAPLPSApp/`** — an Avalonia desktop editor + viewer with drawing tools, attribute editors, palette/DRCS/texture designers, sequence inspector, and Telidraw text-source pane.
- **`Telidraw`** — a Logo-style domain-specific language (`.td` files) that compiles to `.nap` and decompiles back, byte-identical. The canonical human-editable source format for NAPLPS scenes.
- **`NAPLPSTests/`** — 811 unit tests, full visual-regression suite, and the headline byte-round-trip test across 379 historical example files.

## Status

- All 379 example files in `Examples/` round-trip **byte-identical** through the parser → serializer.
- All 379 round-trip through the Telidraw decompile → recompile cycle (only ~3.7% of commands fall back to the raw-byte form, mostly Mosaic Element and unhandled C1 control codes).
- ANSI X3.110 spec coverage is comprehensive — G-set designation, locking shifts, single shifts, transparency, CAN, SDC, full PDI command family, DRCS PDI rendering, dynamic >16-color palette, Repeat-to-EOL, and G2 supplementary set with non-spacing accent state machinery (visual composition deferred to preserve PP3-matching baselines).

Current version: **0.10.0**

## Quick start

```pwsh
dotnet build NAPLPS.sln
dotnet run --project NAPLPSApp
```

Open any file from `Examples/` to see it render. Toggle **View → Toolbox** to enter authoring mode. Toggle **View → Telidraw Source** to see the live `.td` representation; edits in either pane sync to the other.

### Compile a `.td` file from the CLI

```pwsh
dotnet run --project NAPLPSApp -- compile path/to/scene.td -o path/to/scene.nap
```

## Documentation

- [Telidraw language reference](docs/TELIDRAW.md) — keyword cheatsheet for the DSL
- [NAP file quickref](docs/NAPLPS_QUICKREF.md) — opcode tables + byte layout summary
- [`docs/`](docs/) — original BYTE Magazine NAPLPS articles (1983) and the ANSI X3.110-1983 standard PDF
- [`memory/`](memory/) — persistent notes on spec gaps, Ghidra reverse-engineering of PP3/MVDI renderers, and project history

## Repo layout

```
NAPLPS/                      Spec library (renderer, parser, encoder, builders)
  Commands/                  One class per PDI/control command (59 total)
  Drawing/                   Per-command drawables + DrawContext
  Telidraw/                  DSL: Lexer, Parser, AST, Compiler, Decompiler
  Helpers/                   Reflection-based command registry
NAPLPSApp/                   Avalonia editor (desktop)
  Views/                     XAML windows + dialogs
  ViewModels/                MVVM state holders
  Editor/                    Tools, Undo manager, command actions
  Assets/                    Embedded resources (icons, syntax XSHD)
NAPLPSTests/                 MSTest 4 suites (unit + RoundTrip + Visual)
  Visual/Baselines/          APNG reference images for visual regression
Examples/                    Historical .nap corpus (375 files) + .td templates
docs/                        BYTE PDFs, ANSI spec, Telidraw cheatsheet
memory/                      Project-history notes (auto-loaded by Claude Code)
```

## License

MIT — see `LICENSE`. The 1983 BYTE Magazine NAPLPS articles bundled in `docs/` are reprinted as historical reference; check Byte Inc / McGraw-Hill for current rights status.
