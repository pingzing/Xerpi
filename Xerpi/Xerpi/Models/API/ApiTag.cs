using System.Text.Json.Serialization;
using Xerpi.Converters;

namespace Xerpi.Models.API
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class ApiTag
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public uint Images { get; set; }
        public string SpoilerImageUri { get; set; }
        public string? AliasedTo { get; set; }
        public uint? AlisedToId { get; set; }
        public string Namespace { get; set; }
        public string NameInNamespace { get; set; }
        public string[] ImpliedTags { get; set; }
        public uint[] ImpliedTagIds { get; set; }

        [JsonConverter(typeof(TagCategoryConverter))]
        public TagCategory Category { get; set; }

        [JsonIgnore]
        public string TagString => $"{Name} ({Images})";
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public enum TagCategory
    {
        None = 0,
        Rating = 1,
        Origin = 2,
        Character = 3,
        Species = 4,
        ContentOfficial = 5,
        ContentFanmade = 6,
        Spoiler = 7,
        OC = 8,

        Unmapped = 99,
    }
}
