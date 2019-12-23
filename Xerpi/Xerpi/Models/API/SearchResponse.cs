namespace Xerpi.Models.API
{
    public class SearchResponse
    {
        public ApiImage[] Search { get; set; }
        public uint Total { get; set; }
    }
}
