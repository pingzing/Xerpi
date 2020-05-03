using System.Text.Json.Serialization;

namespace Xerpi.Models.API
{
    public class FiltersResponse
    {
        [JsonPropertyName("filters")]
        public ApiFilter[] Filters { get; set; }
    }
}
