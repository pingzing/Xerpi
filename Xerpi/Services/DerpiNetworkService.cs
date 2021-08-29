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
using Xerpi.Models;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    public class DerpiNetworkService : IDerpiNetworkService
    {
        private const string Api = "api/v1/json";
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;

        public string BaseUri { get; private set; }

        // TODO: Handle exceptions when network adapters kerplode.
        public DerpiNetworkService(HttpClient httpClient, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            _jsonOptions.Converters.Add(new UtcDateTimeOffsetConverter());
            BaseUri = _httpClient.BaseAddress.ToString();
        }

        public async Task<ImageSearchResponse?> GetImages(uint page = 1, uint perPage = 15)
        {
            return await SearchImages(SearchParameters.Default, page, perPage);
        }

        public async Task<ImageSearchResponse?> SearchImages(SearchParameters parameters, uint page, uint itemsPerPage)
        {
            string sortPropertyFragment = parameters.SortProperty != null ? $"&sf={parameters.SortProperty}" : "";
            string requestUrl = $"{Api}/search/images?q={WebUtility.UrlEncode(parameters.SearchQuery)}" +
                $"&sd={parameters.SortOrder}" +
                $"{sortPropertyFragment}" +
                $"&page={page}" +
                $"&per_page={itemsPerPage}" +
                $"&filter_id={_settingsService.FilterId}";
            requestUrl = requestUrl.Replace("%20", "+"); // Because spaces are replaced with plusses ¯\_(ツ)_/¯
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"No successful response for SearchImages. Response: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            try
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ImageSearchResponse?>(jsonString, _jsonOptions);
            }
            catch (JsonException e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public async Task<IEnumerable<ApiTag>?> GetTags(IEnumerable<uint> ids)
        {
            string idQueryParams = string.Join(" || ", ids.Select(x => $"id:{x}"));
            string url = $"{Api}/search/tags?q={idQueryParams}";
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
            var response = await _httpClient.GetAsync($"{Api}/filters/system");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetDefaultFilters failed. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var filtersResponse = await JsonSerializer.DeserializeAsync<FiltersResponse?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
            return filtersResponse?.Filters;
        }

        public async Task<CommentsResponse?> GetComments(uint imageId, uint page = 1)
        {
            var response = await _httpClient.GetAsync($"{Api}/search/comments?q=image_id:{imageId}&page={page}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetComments failed for image ID {imageId}. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await JsonSerializer.DeserializeAsync<CommentsResponse?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
        }

        public async Task<ApiUser?> GetUserProfile(uint userId)
        {
            var response = await _httpClient.GetAsync($"{Api}/profiles/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetUserProfile failed for user ID: {userId}. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await JsonSerializer.DeserializeAsync<ApiUser?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
        }
    }
}