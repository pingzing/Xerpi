namespace Xerpi.Models
{
    public class SearchSortOptions
    {
        /// <summary>
        /// Null indicates "no change".
        /// </summary>
        public SortOrderKind? SortOrder { get; set; }

        /// <summary>
        /// Null indicates "no change".
        /// </summary>
        public string? SortByProperty { get; set; }
    }
}
