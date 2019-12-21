using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xerpi.Converters;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    public class DerpiNetworkService : IDerpiNetworkService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly HttpClient _httpClient;

        public DerpiNetworkService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            _jsonOptions.Converters.Add(new EnhancedJsonStringEnumConverter(allowIntegerValues: true));
        }

        public async Task<ImagesResponse>? GetImages(uint page = 1, uint perPage = 15)
        {
            var response = await _httpClient.GetAsync($"images.json?page={page}&perpage={perPage}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Sadness.");
                return null;
            }

            return await JsonSerializer.DeserializeAsync<ImagesResponse>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
        }

        public async Task<TagsResponse>? GetTags(uint[] ids)
        {
            string idQueryParams = string.Join("&", ids.Select(x => $"ids[]={x}"));
            string url = $"api/v2/tags/fetch_many.json?{idQueryParams}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetTags failed. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await JsonSerializer.DeserializeAsync<TagsResponse>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
        }
    }
}