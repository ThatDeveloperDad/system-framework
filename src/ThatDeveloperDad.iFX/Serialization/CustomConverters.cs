using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThatDeveloperDad.iFX.Serialization;

public class EmptyStringToNullConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            // FAFO:  When the string is null, don't write anything.
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}


