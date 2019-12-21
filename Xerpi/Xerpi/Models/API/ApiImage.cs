using System;

namespace Xerpi.Models.API
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class ApiImage
    {
        public uint Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset FirstSeenAt { get; set; }
        public string Tags { get; set; }

        public uint[] TagIds { get; set; }
        public uint UploaderId { get; set; }
        public int Score { get; set; }
        public uint CommentCount { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint TagCount { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Uploader { get; set; }
        public string Image { get; set; }
        public uint Upvotes { get; set; }
        public uint Downvotes { get; set; }
        public uint Faves { get; set; }
        public int AspectRatio { get; set; }
        public string OriginalFormat { get; set; }
        public string MimeType { get; set; }
        public string Sha512Hash { get; set; }
        public string? OrigSha512Hash { get; set; }
        public string SourceUrl { get; set; }
        public ApiRepresentations Representations { get; set; }
        public bool IsRendered { get; set; }
        public bool IsOptimized { get; set; }
        public bool Spoilered { get; set; }
    }

    public class ApiRepresentations
    {
        public string ThumbTiny { get; set; }
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