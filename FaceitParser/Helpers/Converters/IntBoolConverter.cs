using System.Buffers.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Buffers;

namespace FaceitParser.Helpers
{
    public class IntBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                // try to parse number directly from bytes
                ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out int number, out int bytesConsumed) && span.Length == bytesConsumed)
                {
                    return Convert.ToBoolean(number);
                }
                // try to parse from a string if the above failed, this covers cases with other escaped/UTF characters
                var str = reader.GetString();
                if (str.Contains("1") || str.Contains("0"))
                    return str == "1";
            }

            // fallback to default handling
            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteStringValue((value ? 1 : 0).ToString());
        }
    }
}
