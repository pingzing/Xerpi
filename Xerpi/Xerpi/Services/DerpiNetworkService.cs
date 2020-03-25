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
        }

        public async Task<ImageSearchResponse?> GetImages(uint page = 1, uint perPage = 15)
        {
            return await SearchImages("*", page, perPage);
        }

        // Query is a comma-separated list, with spaces transformed into plusses. Everything else seems to get url-encoded.
        public async Task<ImageSearchResponse?> SearchImages(string query, uint page, uint itemsPerPage) // TODO: sd (sort direction), sf (sort field)
        {
            string requestUrl = $"search/images?q={WebUtility.UrlEncode(query)}&page={page}&per_page={itemsPerPage}&filter_id={_settingsService.FilterId}";
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
            string idQueryParams = string.Join("||", ids.Select(x => $"id:{x}"));
            string url = $"search/tags?q={idQueryParams}";
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
            // No replacement for this in the new API. Update: Damaged is working on it
            // Default filters are:
            /*
             * 37431
             * 37429
             * 100073
             * 56027
             * 37432
             * 37430
             */
            return null; // TODO: Wait for the endpoint to exist.
            var response = await _httpClient.GetAsync("filters");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetDefaultFilters failed. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var filtersResponse = await JsonSerializer.DeserializeAsync<FiltersResponse?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
            return filtersResponse?.SystemFilters;
        }

        public async Task<CommentsResponse?> GetComments(uint imageId, uint page = 1)
        {
            var response = await _httpClient.GetAsync($"search/comments?q=image_id:{imageId}&page={page}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetComments failed for image ID {imageId}. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await JsonSerializer.DeserializeAsync<CommentsResponse?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
        }

        public async Task<ApiUser?> GetUserProfile(uint userId)
        {
            var response = await _httpClient.GetAsync($"profiles/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetUserProfile failed for user ID: {userId}. HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await JsonSerializer.DeserializeAsync<ApiUser?>(await response.Content.ReadAsStreamAsync(), _jsonOptions);
        }
    }
}