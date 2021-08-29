using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xerpi.Converters;

namespace Xerpi.Models.API
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class ApiTag : IEquatable<ApiTag>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        public uint Images { get; set; }

        [JsonPropertyName("spoiler_image_url")]
        public string SpoilerImageUri { get; set; }

        [JsonPropertyName("aliased_to")]
        public string? AliasedTo { get; set; }

        [JsonPropertyName("aliased_to_id")]
        public uint? AlisedToId { get; set; }

        public string Namespace { get; set; }

        [JsonPropertyName("name_in_namespace")]
        public string NameInNamespace { get; set; }

        [JsonPropertyName("implied_tags")]
        public string[] ImpliedTags { get; set; }

        [JsonPropertyName("implied_tag_ids")]
        public uint[] ImpliedTagIds { get; set; }

        [JsonConverter(typeof(TagCategoryConverter))]
        public TagCategory Category { get; set; }

        [JsonIgnore]
        public string TagString => $"{Name} ({Images})";

        public override bool Equals(object? obj)
        {
            return Equals(obj as ApiTag);
        }

        public bool Equals(ApiTag? other)
        {
            return other != null &&
                   Id == other.Id &&
                   Images == other.Images;
        }

        public override int GetHashCode()
        {
            var hashCode = -1784744757;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Images.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ApiTag? left, ApiTag? right)
        {
            return EqualityComparer<ApiTag>.Default.Equals(left, right);
        }

        public static bool operator !=(ApiTag? left, ApiTag? right)
        {
            return !(left == right);
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public enum TagCategory
    {
        Rating = 0,
        Origin = 1,
        Character = 2,
        Species = 3,
        ContentOfficial = 4,
        ContentFanmade = 5,
        Spoiler = 6,
        OC = 7,
        None = 8,

        Unmapped = 99,
    }
}
