using System;
using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    public class ApiComment
    {
        public uint Id { get; set; }

        [JsonPropertyName("image_id")]
        public uint ImageId { get; set; }

        // Everything below gets nulled out for comments on deleted images.
        // Body gets nulled out for hidden(?) comments
        // But mostly, these won't be null

        public string? Body { get; set; } // Note: Newlines come through as \r\n.
        public string? Author { get; set; }
        public string? Avatar { get; set; } // This might be a url, or it might be a literal 'data:image/svg+xml;base64' SVG.

        [JsonPropertyName("user_id")]
        public uint? UserId { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
