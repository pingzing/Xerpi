using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xerpi.Models.API;

namespace Xerpi.Converters
{
    public class TagCategoryConverter : JsonConverter<TagCategory>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(TagCategory);
        }

        public override TagCategory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonString = reader.GetString();
            if (reader.TokenType == JsonTokenType.Null)
            {
                return TagCategory.None;
            }

            return jsonString switch
            {
                "rating" => TagCategory.Rating,
                "origin" => TagCategory.Origin,
                "character" => TagCategory.Character,
                "species" => TagCategory.Species,
                "content-official" => TagCategory.ContentOfficial,
                "content-fanmade" => TagCategory.ContentFanmade,
                "spoiler" => TagCategory.Spoiler,
                "oc" => TagCategory.OC,
                _ => TagCategory.Unmapped
            };
        }

        public override void Write(Utf8JsonWriter writer, TagCategory value, JsonSerializerOptions options)
        {
            const string c = "category";
            switch (value)
            {
                case (TagCategory.None):
                    writer.WriteNull(c);
                    break;
                case (TagCategory.Rating):
                    writer.WriteString(c, "rating");
                    break;
                case (TagCategory.Origin):
                    writer.WriteString(c, "origin");
                    break;
                case (TagCategory.Character):
                    writer.WriteString(c, "character");
                    break;
                case (TagCategory.Species):
                    writer.WriteString(c, "species");
                    break;
                case (TagCategory.ContentOfficial):
                    writer.WriteString(c, "content-official");
                    break;
                case (TagCategory.ContentFanmade):
                    writer.WriteString(c, "content-fanmade");
                    break;
                case (TagCategory.Spoiler):
                    writer.WriteString(c, "spoiler");
                    break;
                case (TagCategory.OC):
                    writer.WriteString(c, "spoiler");
                    break;
                default:
                    writer.WriteString(c, "unknown");
                    break;
            };
        }
    }
}
