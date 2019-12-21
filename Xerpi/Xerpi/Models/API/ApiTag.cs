using System.Runtime.Serialization;

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
        public TagCategory? Category { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public enum TagCategory
    {
        Rating = 0,
        Origin = 1,
        Character = 2,
        Species = 3,

        [EnumMember(Value = "content-official")]
        ContentOfficial = 4,

        [EnumMember(Value = "content-fanmade")]
        ContentFanmade = 5,

        Spoiler = 6,
    }
}
