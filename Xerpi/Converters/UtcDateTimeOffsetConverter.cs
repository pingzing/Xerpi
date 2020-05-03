using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xerpi.Converters
{
    public class UtcDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!reader.TryGetDateTimeOffset(out DateTimeOffset readDateTime))
            {
                return null;
            }

            return new DateTimeOffset(readDateTime.DateTime, TimeSpan.Zero);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
