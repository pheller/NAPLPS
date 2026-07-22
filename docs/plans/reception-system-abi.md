# Stateful NaplpsCtx C ABI for the reception-system

Work order: /Users/pheller/NewProjects/Prodigy/reception-system/docs/NAPLPS-INTEGRATION-ASKS.md
Branch: reception-system (worktree NAPLPS-reception-system, off main @ abbf2a2).

## Design

New `NativeExportsCtx.cs` implementing the requested surface over the existing
machinery. An opaque handle maps to a managed context via a static handle table
(nint id -> CtxState); UnmanagedCallersOnly entry points only.

CtxState holds:
- `List<byte> Bytes` - the accumulated stream (bytes are truth).
- `NaplpsFormat Format` - re-parsed from Bytes on every append (no incremental
  parser exists; re-parse reproduces identical decoder state and painted-command
  indices are stable because the stream only grows at the end).
- `DrawContext Draw` - created once per (re)parse against the SAME Image so
  painted pixels persist; per-call render options re-established before each step.
- `int Cursor` - count of commands already painted.
- Pinned RGBA8888 buffer (GCHandle-pinned byte[w*h*4]) refreshed from the
  ImageSharp image on `naplps_ctx_framebuffer` calls; pointer stable for the
  context lifetime (stronger than the ask's until-next-exec guarantee).

Entry points (exact signatures from the ask):
- `naplps_ctx_create(w, h, flags)` - flags bit 0 = NAPLPS_MODE_PRODIGY: forced
  Prodigy parse + ColorGunWidth 2, MVDI font, hard text, authentic geometry,
  Prodigy display ratio (same as naplps_render_png_prodigy).
- `naplps_ctx_destroy`, `naplps_ctx_reset` (clears bytes, state, framebuffer;
  Q1 keep-charsets variant deferred - DRCS state is re-fed cheaply by
  re-appending definition bytes, and reset+append is the simple model).
- `naplps_ctx_append(bytes, len)` -> total command count. Re-parse; Cursor
  unchanged; painting resumes from Cursor over the new Format.
- `naplps_ctx_exec_to(idx)` / `naplps_ctx_exec_next(out_dirty)` - step
  RenderCommand per unpainted command WITHOUT clearing the canvas (incremental;
  Render()'s full-repaint path is not used). Dirty rect v1 = full canvas
  (documented; refinement later from drawable bounds).
- `naplps_ctx_command_count`, `naplps_ctx_framebuffer`.

Caveats documented in the header:
- Mid-stream CLUT redefinition retroactive repaint (generic NAPLPS only;
  Prodigy ignores redefinition) is NOT applied in stepping mode.
- Thread safety: one context per thread of use; no cross-call locking.

## Q1 (DRCS/custom text) - research + answer
Verify glyph persistence end to end: DefDRCS parse -> State storage -> text
command invoking DRCS G-set draws glyphs. Produce the byte recipe (designate
DRCS into a G-set, invoke, SET POINT, chars) and validate through a probe that
appends definitions and text in SEPARATE append calls, asserting pixels.

## Q2 (partition coordinates) - research + answer
Inspect multi-partition Prodigy page data (reception-system extractions +
applications corpus): decompile partition streams, check whether leading
positioning uses absolute coordinates. Report with concrete examples.

## Validation
- managed probe: create/append/step parity vs one-shot Render for corpus files
  (pixel-identical final frames).
- AOT publish osx-arm64 + C smoke test (tools/aot/c) exercising create/append/
  exec_next/framebuffer; assert non-black pixels + version.
- Header tools/aot/include/naplps.h gains the declarations verbatim from the ask.
