using NCR = NAPLPS.NaplpsCommandReference;

namespace NAPLPS.Helpers;

public sealed class NCRArrayJsonConverter : JsonConverter<NCR[]>
{
    public override NCR[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var table = new NCR[256];

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return table;

            var slot = reader.GetString();
            reader.Read();
            var setName = reader.GetString();

            (int offset, NCR[] set) = slot switch
            {
                "C0" => (0, ResolveSet(setName)),
                "GLeft" => (32, ResolveSet(setName)),
                "C1" => (128, ResolveSet(setName)),
                "GRight" => (160, ResolveSet(setName)),
                _ => throw new JsonException($"Unknown slot {slot}")
            };

            set.CopyTo(table, offset);
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, NCR[] value, JsonSerializerOptions options)
    {
        static bool Matches(NCR[] table, int offset, NCR[] set)
        {
            for (int i = 0; i < set.Length; i++)
                if (!ReferenceEquals(table[offset + i], set[i]))
                    return false;
            return true;
        }

        static string Resolve(NCR[] table, int offset, params (string name, NCR[] set)[] candidates)
        {
            foreach (var (name, set) in candidates)
                if (Matches(table, offset, set))
                    return name;
            return "Unknown";
        }

        writer.WriteStartObject();

        writer.WriteString("C0",
            Resolve(value, NaplpsState.C0,
                ("C0Set", NaplpsState.C0Set)));

        writer.WriteString("GLeft",
            Resolve(value, 32,
                ("PrimaryCharacterSet", NaplpsState.PrimaryCharacterSet),
                ("SupplementaryCharacterSet", NaplpsState.SupplementaryCharacterSet),
                ("GeneralPDISet", NaplpsState.GeneralPDISet),
                ("MosiacSet", NaplpsState.MosiacSet)));

        writer.WriteString("C1",
            Resolve(value, 128,
                ("C1Set", NaplpsState.C1Set)));

        writer.WriteString("GRight",
            Resolve(value, 160,
                ("GeneralPDISet", NaplpsState.GeneralPDISet),
                ("MosiacSet", NaplpsState.MosiacSet)));

        writer.WriteEndObject();
    }

    private static NCR[] ResolveSet(string? name) => name switch
    {
        "C0Set" => NaplpsState.C0Set,
        "C1Set" => NaplpsState.C1Set,
        "PrimaryCharacterSet" => NaplpsState.PrimaryCharacterSet,
        "SupplementaryCharacterSet" => NaplpsState.SupplementaryCharacterSet,
        "GeneralPDISet" => NaplpsState.GeneralPDISet,
        "MosiacSet" => NaplpsState.MosiacSet,
        _ => throw new JsonException($"Unknown NCR set '{name}'")
    };
}


