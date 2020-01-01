using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        private readonly ISettingsService _settingsService;

        public DerpiNetworkService(HttpClient httpClient, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            _jsonOptions.Converters.Add(new EnhancedJsonStringEnumConverter(allowIntegerValues: true));
        }

        public async Task<IEnumerable<ApiImage>?> GetImages(uint page = 1, uint perPage = 15)
        {
            var response = await _httpClient.GetAsync($"images.json?page={page}&perpage={perPage}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Sadness.");
                return null;
            }

            var imagesResponse = await JsonSerializer.DeserializeAsync<ImagesResponse?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
            return imagesResponse?.Images;
        }

        // Query is a comma-separated list, with spaces transformed into plusses. Everything else seems to get url-encoded.
        public async Task<SearchResponse?> SearchImages(string query, uint page, uint itemsPerPage)
        {
            string requestUrl = $"search.json?q={WebUtility.UrlEncode(query)}&page={page}&perpage={itemsPerPage}&filter_id={_settingsService.FilterId}";
            requestUrl = requestUrl.Replace("%20", "+"); // Because spaces are replaced with plusses ¯\_(ツ)_/¯
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"No successful response for SearchImages. Response: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            try
            {
                return await JsonSerializer.DeserializeAsync<SearchResponse?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
            }
            catch (JsonException e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public async Task<IEnumerable<ApiTag>?> GetTags(IEnumerable<uint> ids)
        {
            string idQueryParams = string.Join("&", ids.Select(x => $"ids[]={x}"));
            string url = $"api/v2/tags/fetch_many.json?{idQueryParams}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetTags failed. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            try
            {
                var tags = await JsonSerializer.DeserializeAsync<TagsResponse?>(stream, _jsonOptions);
                return tags?.Tags;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Unable to parse. {ex}. This content: {await response.Content.ReadAsStringAsync()}");
                return null;
            }
        }

        public async Task<IEnumerable<ApiFilter>?> GetDefaultFilters()
        {
            var response = await _httpClient.GetAsync("filters.json");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetDefaultFilters failed. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var filtersResponse = await JsonSerializer.DeserializeAsync<FiltersResponse?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
            return filtersResponse?.SystemFilters;
        }
    }
}