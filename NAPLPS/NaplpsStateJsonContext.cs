// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Text.Json.Serialization;

namespace NAPLPS;

/// <summary>
/// Source-generated JSON type info for <see cref="NaplpsState"/> Clone / ToJson / FromJson.
/// Required under NativeAOT — the generic Serialize/Deserialize&lt;T&gt; overloads pull in
/// reflection-based type discovery that the AOT compiler can't trace. The source gen emits
/// a static metadata graph at build time so NaplpsState + its transitively-serialized
/// types (including Dictionary&lt;byte, NaplpsColor&gt;, NaplpsField, NaplpsColor, etc.) are
/// preserved without the runtime reflection fallback.
///
/// NOTE: custom converters (NCRArrayJsonConverter, Vector3JsonConverter, Vector2JsonConverter)
/// stay on <c>NaplpsState.GlobalJsonSerializerOptions.Converters</c> — the context consults
/// those at serialize time via <c>JsonSerializerOptions</c>, so no source-gen-of-converter
/// is required.
/// </summary>
// GenerationMode = Metadata: defer actual serialization to the custom converters on
// GlobalJsonSerializerOptions at runtime. The default (Default = Metadata | Serialization)
// generates a fast-path path that bypasses our NCRArrayJsonConverter and attempts to
// reflect over NaplpsCommandReference — which has a DAM-constrained Type parameter that
// can't be statically satisfied. Metadata-only keeps the type-info graph but always
// routes through the converter chain, which IS AOT-safe since NCRArrayJsonConverter
// handles NCR[] end-to-end without reflection.
[JsonSerializable(typeof(NaplpsState), GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class NaplpsStateJsonContext : JsonSerializerContext
{
}
