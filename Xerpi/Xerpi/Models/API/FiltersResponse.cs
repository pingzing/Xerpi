using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    public class FiltersResponse
    {
        [JsonPropertyName("system_filters")]
        public ApiFilter[] SystemFilters { get; set; }

        [JsonPropertyName("user_filters")]
        public ApiFilter[] UserFilters { get; set; }

        [JsonPropertyName("search_filters")]
        public ApiFilter[] SearchFilters { get; set; }
    }
}
