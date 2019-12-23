using System;
using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class ApiImage
    {
        public uint Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonPropertyName("first_seen_at")]
        public DateTimeOffset FirstSeenAt { get; set; }

        public string Tags { get; set; }

        [JsonPropertyName("tag_ids")]
        public uint[] TagIds { get; set; }

        [JsonPropertyName("uploader_id")]
        public uint? UploaderId { get; set; }

        public int Score { get; set; }

        [JsonPropertyName("comment_count")]
        public uint CommentCount { get; set; }

        public uint Width { get; set; }
        public uint Height { get; set; }

        [JsonPropertyName("tag_count")]
        public uint TagCount { get; set; }

        [JsonPropertyName("file_name")]
        public string FileName { get; set; }

        public string Description { get; set; }
        public string Uploader { get; set; }
        public string Image { get; set; }
        public uint Upvotes { get; set; }
        public uint Downvotes { get; set; }
        public uint Faves { get; set; }

        [JsonPropertyName("aspect_ratio")]
        public double AspectRatio { get; set; }

        [JsonPropertyName("original_format")]
        public string OriginalFormat { get; set; }

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; }

        [JsonPropertyName("sha512_hash")]
        public string Sha512Hash { get; set; }

        [JsonPropertyName("orig_sha512_hash")]
        public string? OrigSha512Hash { get; set; }

        [JsonPropertyName("source_url")]
        public string SourceUrl { get; set; }

        public ApiRepresentations Representations { get; set; }

        [JsonPropertyName("is_rendered")]
        public bool IsRendered { get; set; }

        [JsonPropertyName("is_optimized")]
        public bool IsOptimized { get; set; }

        public bool Spoilered { get; set; }
    }

    public class ApiRepresentations
    {
        [JsonPropertyName("thumb_tiny")]
        public string ThumbTiny { get; set; }

        [JsonPropertyName("thumb_small")]
        public string ThumbSmall { get; set; }
        public string Thumb { get; set; }
        public string Small { get; set; }
        public string Medium { get; set; }
        public string Large { get; set; }
        public string Tall { get; set; }
        public string Full { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.