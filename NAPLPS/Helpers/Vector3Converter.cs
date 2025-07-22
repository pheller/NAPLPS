// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Helpers;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var vector = new Vector3();

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return vector;
            }

            string propertyName = reader.GetString() ?? string.Empty;

            reader.Read();

            switch (propertyName)
            {
                case "X":
                {
                    vector.X = reader.GetSingle();
                }
                break;

                case "Y":
                {
                    vector.Y = reader.GetSingle();
                }
                break;

                case "Z":
                {
                    vector.Z = reader.GetSingle();
                }
                break;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}

