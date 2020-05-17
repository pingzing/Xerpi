namespace Xerpi.Models
{
    public class SearchParameters
    {
        public string SearchQuery { get; set; } = "*";
        public string? SortProperty { get; set; } = null;
        public SortOrderKind SortOrder { get; set; } = SortOrderKind.Descending;

        public static readonly SearchParameters Default = new SearchParameters
        {
            SearchQuery = "*",
            SortProperty = null,
            SortOrder = SortOrderKind.Descending
        };
    }
}
