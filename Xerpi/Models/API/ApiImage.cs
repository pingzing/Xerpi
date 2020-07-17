using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    [DebuggerDisplay("ID: {Id}, Score: {Score}")]
    public class ApiImage : IEquatable<ApiImage>
    {
        public uint Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonPropertyName("first_seen_at")]
        public DateTimeOffset FirstSeenAt { get; set; }

        public string[] Tags { get; set; } = null!;

        [JsonPropertyName("tag_ids")]
        public uint[] TagIds { get; set; } = null!;

        [JsonPropertyName("uploader_id")]
        public uint? UploaderId { get; set; }

        public int Score { get; set; }

        [JsonPropertyName("comment_count")]
        public uint CommentCount { get; set; }

        public uint Width { get; set; }
        public uint Height { get; set; }

        [JsonPropertyName("tag_count")]
        public uint TagCount { get; set; }

        public string Description { get; set; } = null!;
        public string Uploader { get; set; } = null!;
        public uint Upvotes { get; set; }
        public uint Downvotes { get; set; }
        public uint Faves { get; set; }

        [JsonPropertyName("aspect_ratio")]
        public double AspectRatio { get; set; }

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = null!;

        [JsonPropertyName("sha512_hash")]
        public string Sha512Hash { get; set; } = null!;

        [JsonPropertyName("orig_sha512_hash")]
        public string? OrigSha512Hash { get; set; }

        [JsonPropertyName("source_url")]
        public string SourceUrl { get; set; } = null!;

        public ApiRepresentations Representations { get; set; } = null!;

        public bool Spoilered { get; set; }

        /// <summary>
        /// NOT ACTUALLY PART OF THE API RESPONSE. This is a dirty hack to add page information to each image. TODO: Remove this and add it elsewhere.
        /// </summary>
        public uint? SearchPage { get; set; } = null!;

        /// <summary>
        /// ALSO NOT ACTUALLY PART OF THE API RESPONSE.
        /// A secondary dirty hack, because SearchPage handles gross sorting, but nothing
        /// deals with per-page sorting, and we need to maintain sort-order from the website.
        /// </summary>
        public uint? SortIndex { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ApiImage);
        }

        public bool Equals(ApiImage? other)
        {
            return other != null! &&
                   Id == other.Id &&
                   EqualityComparer<string[]>.Default.Equals(Tags, other.Tags) &&
                   EqualityComparer<uint[]>.Default.Equals(TagIds, other.TagIds) &&
                   Score == other.Score &&
                   CommentCount == other.CommentCount &&
                   TagCount == other.TagCount &&
                   Faves == other.Faves;
        }

        public override int GetHashCode()
        {
            var hashCode = 1625394605;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(Tags);
            hashCode = hashCode * -1521134295 + EqualityComparer<uint[]>.Default.GetHashCode(TagIds);
            hashCode = hashCode * -1521134295 + Score.GetHashCode();
            hashCode = hashCode * -1521134295 + CommentCount.GetHashCode();
            hashCode = hashCode * -1521134295 + TagCount.GetHashCode();
            hashCode = hashCode * -1521134295 + Faves.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ApiImage? left, ApiImage? right)
        {
            return EqualityComparer<ApiImage>.Default.Equals(left!, right!);
        }

        public static bool operator !=(ApiImage? left, ApiImage? right)
        {
            return !(left == right);
        }
    }

    public class ApiRepresentations
    {
        [JsonPropertyName("thumb_tiny")]
        public string ThumbTiny { get; set; } = null!;

        [JsonPropertyName("thumb_small")]
        public string ThumbSmall { get; set; } = null!;
        public string Thumb { get; set; } = null!;
        public string Small { get; set; } = null!;
        public string Medium { get; set; } = null!;
        public string Large { get; set; } = null!;
        public string Tall { get; set; } = null!;
        public string Full { get; set; } = null!;
    }
}