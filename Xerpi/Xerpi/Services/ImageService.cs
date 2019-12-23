using DynamicData;
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

        private SourceCache<ApiImage, uint> _currentImages = new SourceCache<ApiImage, uint>(x => x.Id);
        public IObservableCache<ApiImage, uint> CurrentImages { get; private set; }

        private SourceCache<ApiTag, uint> _tags = new SourceCache<ApiTag, uint>(x => x.Id);
        public IObservableCache<ApiTag, uint> Tags { get; private set; }

        public ImageService(IDerpiNetworkService networkService)
        {
            CurrentImages = _currentImages.AsObservableCache();
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
            var images = await _networkService.GetImages(page, itemsPerPage);
            if (images == null)
            {
                return;
            }

            // TODO: perf - can change this to EditDiff
            _currentImages.Edit(x =>
            {
                foreach (var image in images)
                {
                    x.AddOrUpdate(image);
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
            if (searchResult?.Search == null)
            {
                return 0;
            }

            // TODO: perf - can change this to EditDiff
            _currentImages.Edit(x =>
            {
                foreach (var image in searchResult.Search)
                {
                    x.AddOrUpdate(image);
                }
            });

            return searchResult.Total;
        }

        public async Task UpdateTags(uint[] tags)
        {
            IEnumerable<uint> missingTags = tags.Except(_tags.Keys);
            var newTags = await _networkService.GetTags(missingTags);
            if (newTags == null)
            {
                return;
            }

            // TODO: perf - can change this to EditDiff
            _tags.Edit(x =>
            {
                foreach (var tag in newTags)
                {
                    x.AddOrUpdate(tag);
                }
            });
        }
    }
}
