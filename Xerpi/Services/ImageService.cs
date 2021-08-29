using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xerpi.Extensions;
using Xerpi.Models;
using Xerpi.Models.API;

namespace Xerpi.Services
{
    // TODO: At some point, if we want multiple tabs, we'll have to split this out into an 
    // ImageService (a singleton with image caches) and a SearchService (multiple-instance, has queries and sorting)
    public interface IImageService
    {
        IObservableCache<ApiImage, uint> CurrentImages { get; }
        Task<uint> Search(SearchParameters searchParameters, uint page = 1, uint itemsPerPage = 15);
        Task<uint> AddPageToSearch(uint page = 1, uint itemsPerPage = 15);
        List<uint> PagesVisible { get; }
        SearchParameters CurrentSearchParameters { get; }
        event EventHandler<SearchParameters>? CurrentSearchParametersChanged;
        uint CurrentTotalImages { get; }
    }

    public class ImageService : IImageService
    {
        private readonly IDerpiNetworkService _networkService;
        private readonly IEqualityComparer<ApiImage> _imageComparer = EqualityComparer<ApiImage>.Default;
        private readonly IEqualityComparer<ApiTag> _tagComparer = EqualityComparer<ApiTag>.Default;

        private SourceCache<ApiImage, uint> _currentImages = new SourceCache<ApiImage, uint>(x => x.Id);
        public IObservableCache<ApiImage, uint> CurrentImages { get; private set; }


        public List<uint> PagesVisible { get; private set; }
        public SearchParameters CurrentSearchParameters { get; private set; } = new SearchParameters();
        public event EventHandler<SearchParameters>? CurrentSearchParametersChanged;
        public uint CurrentTotalImages { get; private set; }

        public ImageService(IDerpiNetworkService networkService)
        {
            PagesVisible = new List<uint>() { 1 };

            CurrentImages = _currentImages.AsObservableCache();
            _networkService = networkService;
        }

        public async Task<uint> Search(SearchParameters parameters, uint page = 1, uint itemsPerPage = 15)
        {
            _currentImages.Clear();

            PagesVisible.Clear();
            PagesVisible.Add(1);

            // Coerce empties or nulls to wildcard, because that's probably what the user expects            
            parameters.SearchQuery = string.IsNullOrWhiteSpace(parameters.SearchQuery) ? "*" : parameters.SearchQuery;
            CurrentSearchParameters = parameters;
            CurrentSearchParametersChanged?.Invoke(this, CurrentSearchParameters);

            return await SearchCore(parameters, page, itemsPerPage);
        }

        public async Task<uint> AddPageToSearch(uint pageNumber, uint itemsPerPage = 15)
        {
            if (PagesVisible.Contains(pageNumber))
            {
                return 0;
            }

            PagesVisible.AddSorted(pageNumber);
            return await SearchCore(CurrentSearchParameters, pageNumber, itemsPerPage);
        }

        private async Task<uint> SearchCore(SearchParameters parameters, uint page = 1, uint itemsPerPage = 15)
        {
            var searchResult = await _networkService.SearchImages(parameters, page, itemsPerPage);
            if (searchResult?.Images == null)
            {
                return 0;
            }

            _currentImages.Edit(x =>
            {
                uint searchPage = page;
                uint sortIndex = 0;
                foreach (var image in searchResult.Images)
                {
                    image.SearchPage = searchPage;
                    image.SortIndex = sortIndex;
                    x.AddOrUpdate(image, _imageComparer);
                    sortIndex++;
                }
            });

            CurrentTotalImages = searchResult.Total;
            return searchResult.Total;
        }
    }
}


