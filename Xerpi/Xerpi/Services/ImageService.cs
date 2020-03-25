using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    public interface IImageService
    {
        IObservableCache<ApiTag, uint> Tags { get; }
        IObservableCache<ApiImage, uint> CurrentImages { get; }
        Task SetImagesToFrontPage(uint page = 1, uint itemsPerPage = 15);
        Task AddPageToFrontPage(uint page = 1, uint itemsPerPage = 15);
        Task<uint> SetImagesToSearch(string query, uint page = 1, uint itemsPerPage = 15);
        Task<uint> AddPageToSearch(string query, uint page = 1, uint itemsPerPage = 15);
        Task UpdateTags(uint[] tags);
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

        public ImageService(IDerpiNetworkService networkService)
        {
            CurrentImages = _currentImages.AsObservableCache();
            _tags.LimitSizeTo(200).Subscribe();
            Tags = _tags.AsObservableCache();
            _networkService = networkService;
        }

        public async Task SetImagesToFrontPage(uint page = 1, uint itemsPerPage = 15)
        {
            _currentImages.Clear();
            await AddToFrontPage(page, itemsPerPage);
        }

        public async Task AddPageToFrontPage(uint page = 1, uint itemsPerPage = 15)
        {
            await AddToFrontPage(page, itemsPerPage);
        }

        private async Task AddToFrontPage(uint page = 1, uint itemsPerPage = 15)
        {
            var imageResponse = await _networkService.GetImages(page, itemsPerPage);
            if (imageResponse == null && imageResponse?.Total == 0)
            {
                return;
            }

            // TODO: perf - can change this to EditDiff
            _currentImages.Edit(x =>
            {
                foreach (var image in imageResponse.Images)
                {
                    x.AddOrUpdate(image, _imageComparer);
                }
            });
        }

        public async Task<uint> SetImagesToSearch(string query, uint page = 1, uint itemsPerPage = 15)
        {
            _currentImages.Clear();
            return await Search(query, page, itemsPerPage);
        }

        public async Task<uint> AddPageToSearch(string query, uint page = 1, uint itemsPerPage = 15)
        {
            return await Search(query, page, itemsPerPage);
        }

        private async Task<uint> Search(string query, uint page = 1, uint itemsPerPage = 15)
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
