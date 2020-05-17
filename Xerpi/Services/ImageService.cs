using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    // TODO: At some point, if we want multiple tabs, we'll have to split this out into an 
    // ImageService (a singleton with image caches) and a SearchService (multiple-instance, has queries and sorting)
    public interface IImageService
    {
        IObservableCache<ApiTag, uint> Tags { get; }
        IObservableCache<ApiImage, uint> CurrentImages { get; }
        Task<uint> Search(string query, uint page = 1, uint itemsPerPage = 15);
        Task<uint> AddPageToSearch(uint page = 1, uint itemsPerPage = 15);
        Task UpdateTags(uint[] tags);
        HashSet<uint> PagesSeen { get; }
        string CurrentSearchQuery { get; }
        event EventHandler<string> CurrentSearchQueryChanged;
    }

    public class ImageService : IImageService
    {
        private readonly IDerpiNetworkService _networkService;
        private readonly IEqualityComparer<ApiImage> _imageComparer = EqualityComparer<ApiImage>.Default;
        private readonly IEqualityComparer<ApiTag> _tagComparer = EqualityComparer<ApiTag>.Default;

        private SourceCache<ApiImage, uint> _currentImages = new SourceCache<ApiImage, uint>(x => x.Id);
        public IObservableCache<ApiImage, uint> CurrentImages { get; private set; }

        private SourceCache<ApiTag, uint> _tags = new SourceCache<ApiTag, uint>(x => x.Id);

        public IObservableCache<ApiTag, uint> Tags { get; private set; }

        public HashSet<uint> PagesSeen { get; private set; }
        public string CurrentSearchQuery { get; private set; } = "*";
        public event EventHandler<string>? CurrentSearchQueryChanged;

        public ImageService(IDerpiNetworkService networkService)
        {
            PagesSeen = new HashSet<uint>() { 1 };

            CurrentImages = _currentImages.AsObservableCache();
            _tags.LimitSizeTo(200).Subscribe();
            Tags = _tags.AsObservableCache();
            _networkService = networkService;
        }

        public async Task<uint> Search(string query, uint page = 1, uint itemsPerPage = 15)
        {
            _currentImages.Clear();

            PagesSeen.Clear();
            PagesSeen.Add(1);

            query = string.IsNullOrWhiteSpace(query) ? "*" : query; // Coerce empties or nulls to wildcard, because that's probably what the user expects
            CurrentSearchQuery = query;
            CurrentSearchQueryChanged?.Invoke(this, CurrentSearchQuery);

            return await SearchCore(query, page, itemsPerPage);
        }

        public async Task<uint> AddPageToSearch(uint page, uint itemsPerPage = 15)
        {
            if (PagesSeen.Contains(page))
            {
                return 0;
            }

            PagesSeen.Add(page);
            return await SearchCore(CurrentSearchQuery, page, itemsPerPage);
        }

        private async Task<uint> SearchCore(string query, uint page = 1, uint itemsPerPage = 15)
        {
            var searchResult = await _networkService.SearchImages(query, page, itemsPerPage);
            if (searchResult?.Images == null)
            {
                return 0;
            }

            // TODO: perf - can change this to EditDiff
            _currentImages.Edit(x =>
            {
                foreach (var image in searchResult.Images)
                {
                    x.AddOrUpdate(image, _imageComparer);
                }
            });

            return searchResult.Total;
        }

        public async Task UpdateTags(uint[] tagIds)
        {
            IEnumerable<uint> missingTags = tagIds.Except(_tags.Keys);
            if (!missingTags.Any())
            {
                return;
            }

            var newTags = await _networkService.GetTags(missingTags).ConfigureAwait(false);
            if (newTags == null)
            {
                return;
            }

            // TODO: perf - can change this to EditDiff
            _tags.Edit(x =>
            {
                foreach (var tag in newTags)
                {
                    x.AddOrUpdate(tag, _tagComparer);
                }
            });
        }
    }
}
