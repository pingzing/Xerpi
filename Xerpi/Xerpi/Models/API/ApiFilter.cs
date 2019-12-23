using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    public class ApiFilter
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("hidden_tag_ids")]
        public uint[] HiddenTagIds { get; set; }

        [JsonPropertyName("hidden_tags")]
        public string[] HiddenTags { get; set; }

        [JsonPropertyName("spoilered_tag_ids")]
        public uint[] SpoileredTagIds { get; set; }

        [JsonPropertyName("spoilered_tags")]
        public string[] SpoileredTags { get; set; }

        [JsonPropertyName("hidden_complex")]
        public string HiddenComplex { get; set; }

        [JsonPropertyName("spoilered_complex")]
        public string SpoileredComplex { get; set; }
        public bool Public { get; set; }
        public bool System { get; set; }

        [JsonPropertyName("user_count")]
        public uint UserCount { get; set; }

        [JsonPropertyName("user_id")]
        public uint? UserId { get; set; }
    }
}
