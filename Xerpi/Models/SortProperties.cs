namespace Xerpi.Models
{
    public static class SortProperties
    {
        public const string UploadDate = "created_at";
        public const string ModifiedDate = "updated_at";
        public const string InitialPostDate = "first_seen_at";
        public const string AspectRatio = "aspect_ratio";
        public const string FaveCount = "faves";
        public const string Upvotes = "upvotes";
        public const string Downvotes = "downvotes";
        public const string Score = "score";
        public const string WilsonScore = "wilson_score";
        public const string Relevance = "_score"; // Underscore-score? really?
        public const string Width = "width";
        public const string Height = "height";
        public const string Comments = "comment_count";
        public const string TagCount = "tag_count";
    }
}
