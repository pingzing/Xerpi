using System;
using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    public class ApiUser
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }

        [JsonPropertyName("uploads_count")]
        public int UploadsCount { get; set; }

        [JsonPropertyName("post_count")]
        public int PostCount { get; set; }

        public object[] Links { get; set; }
        public object[] Awards { get; set; }
    }
}
