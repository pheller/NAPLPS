// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics.CodeAnalysis;

// AOT: the System.Text.Json source gen emits fast-path metadata tables that include the
// NaplpsCommandReference constructor, even though at runtime NCR[] round-trips exclusively
// through NCRArrayJsonConverter (registered on the NaplpsStateJsonContext options). The
// generated metadata is DEAD CODE — never invoked — but the trimmer analyzes it anyway
// and emits IL2062/IL2111 because NaplpsCommandReference.ctor has a DAM-constrained Type
// parameter that can't be satisfied by a statically-unknown value. Suppressing globally is
// safe because we've verified the runtime path uses the converter end-to-end. The source
// gen has no per-property opt-out to skip a specific transitively-referenced type.
[assembly: SuppressMessage("Trimming", "IL2062", Scope = "module", Justification = "Generated JSON metadata for NaplpsCommandReference is dead code; NCR[] is handled end-to-end by NCRArrayJsonConverter.")]
[assembly: SuppressMessage("Trimming", "IL2111", Scope = "module", Justification = "Generated JSON metadata for NaplpsCommandReference is dead code; NCR[] is handled end-to-end by NCRArrayJsonConverter.")]
