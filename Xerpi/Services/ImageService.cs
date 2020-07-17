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
        IObservableCache<ApiTag, uint> Tags { get; }
        IObservableCache<ApiImage, uint> CurrentImages { get; }
        Task<uint> Search(SearchParameters searchParameters, uint page = 1, uint itemsPerPage = 15);
        Task<uint> AddPageToSearch(uint page = 1, uint itemsPerPage = 15);
        Task UpdateTags(uint[] tags);
        List<uint> PagesVisible { get; }
        SearchParameters CurrentSearchParameters { get; }
        event EventHandler<SearchParameters>? CurrentSearchParametersChanged;
    }

    public class ImageService : IImageService
    {
        private const uint VisibleImages = 200;

        private readonly IDerpiNetworkService _networkService;
        private readonly IEqualityComparer<ApiImage> _imageComparer = EqualityComparer<ApiImage>.Default;
        private readonly IEqualityComparer<ApiTag> _tagComparer = EqualityComparer<ApiTag>.Default;

        private SourceCache<ApiImage, uint> _currentImages = new SourceCache<ApiImage, uint>(x => x.Id);
        public IObservableCache<ApiImage, uint> CurrentImages { get; private set; }

        private SourceCache<ApiTag, uint> _tags = new SourceCache<ApiTag, uint>(x => x.Id);

        public IObservableCache<ApiTag, uint> Tags { get; private set; }

        public List<uint> PagesVisible { get; private set; }
        public SearchParameters CurrentSearchParameters { get; private set; } = new SearchParameters();
        public event EventHandler<SearchParameters>? CurrentSearchParametersChanged;

        public ImageService(IDerpiNetworkService networkService)
        {
            PagesVisible = new List<uint>() { 1 };

            CurrentImages = _currentImages.AsObservableCache();
            _tags.LimitSizeTo(200).Subscribe();
            Tags = _tags.AsObservableCache();
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

            return await SearchCore(parameters, InsertMode.Append, page, itemsPerPage);
        }

        public async Task<uint> AddPageToSearch(uint pageNumber, uint itemsPerPage = 15)
        {
            if (PagesVisible.Contains(pageNumber))
            {
                return 0;
            }

            InsertMode insertMode = InsertMode.Append;
            if (pageNumber < PagesVisible.Min())
            {
                insertMode = InsertMode.Prepend;
            }

            PagesVisible.AddSorted(pageNumber);
            return await SearchCore(CurrentSearchParameters, insertMode, pageNumber, itemsPerPage);
        }

        private async Task<uint> SearchCore(SearchParameters parameters, InsertMode mode, uint page = 1, uint itemsPerPage = 15)
        {
            var searchResult = await _networkService.SearchImages(parameters, page, itemsPerPage);
            if (searchResult?.Images == null)
            {
                return 0;
            }

            _currentImages.Edit(x =>
            {
                uint searchPage = page;
                if (((uint)(x.Count + searchResult.Images.Length)) > VisibleImages)
                {
                    uint numToRemove = (uint)(x.Count + searchResult.Images.Length) - VisibleImages;
                    List<ApiImage> toRemove = new List<ApiImage>();
                    int pageSearchDirection;
                    int removePage;

                    // Remove from the front, as we're adding to the end
                    if (mode == InsertMode.Append)
                    {
                        pageSearchDirection = 1;
                        removePage = (int)PagesVisible.Min();
                    }
                    // Remove from the end, as we're adding to the front
                    else
                    {
                        pageSearchDirection = -1;
                        removePage = (int)PagesVisible.Max();
                    }

                    while (toRemove.Count < numToRemove)
                    {
                        toRemove.Add(x.Items.Where(y => y.SearchPage == removePage).Take((int)numToRemove));
                        if (!PagesVisible.Contains((uint)(removePage + pageSearchDirection)))
                        {
                            break;
                        }
                        PagesVisible.Remove((uint)removePage);
                        removePage += pageSearchDirection;
                    }

                    // One last .Take() in case we wound up over numToRemove.
                    x.Remove(toRemove.Take((int)numToRemove));
                }
            });

            // Do this in a separate edit pass, so the UI updates. Otherwise, the CollectionView
            // won't realize it needs to keep the scoll item in view.
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


