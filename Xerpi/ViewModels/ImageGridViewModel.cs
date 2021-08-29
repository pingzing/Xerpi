using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Xamarin.Forms;
using Xerpi.Messages;
using Xerpi.Models;
using Xerpi.Models.API;
using Xerpi.Services;

namespace Xerpi.ViewModels
{
    public class ImageGridViewModel : BasePageViewModel
    {
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly IMessagingCenter _messagingService;
        private readonly SearchPageComparer _pageComparer = new SearchPageComparer();

        public override string Url => "images";

        private ObservableCollectionExtended<ApiImage> _images = new ObservableCollectionExtended<ApiImage>();
        public ObservableCollectionExtended<ApiImage> Images
        {
            get => _images;
            set => Set(ref _images, value);
        }

        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => Set(ref _isRefreshing, value);
        }

        public string CurrentSearchQuery => _imageService.CurrentSearchParameters.SearchQuery;
        private void ImageService_CurrentSearchQueryChanged(object _, SearchParameters __)
        {
            OnPropertyChanged(nameof(CurrentSearchQuery));
        }

        public Command RefreshCommand { get; }
        public Command<string> SearchTriggeredCommand { get; }
        public Command GetNextPageCommand { get; }
        public Command GetPreviousPageCommand { get; }
        public Command<SearchSortOptions> SortOptionsChangedCommand { get; }

        public ImageGridViewModel(IImageService imageService,
            INavigationService navigationService,
            IMessagingCenter messagingService)
        {
            _imageService = imageService;
            _imageService.CurrentSearchParametersChanged += ImageService_CurrentSearchQueryChanged;

            _navigationService = navigationService;
            _messagingService = messagingService;

            Title = "Browse";
            RefreshCommand = new Command(async () => await Refresh(CurrentSearchQuery));
            SearchTriggeredCommand = new Command<string>(async x => await SearchQueryChanged(x));
            GetNextPageCommand = new Command(async x => await GetNextPage());
            GetPreviousPageCommand = new Command(async x => await GetPreviousPage());
            SortOptionsChangedCommand = new Command<SearchSortOptions>(async x => await SortOptionsChanged(x));

            _imageService.CurrentImages.Connect()
                .Filter(x => !x.MimeType.Contains("video")) // TODO: Make sure this only covers webm, and not other things we can actually handle                
                .Sort(_pageComparer, SortOptimisations.ComparesImmutableValuesOnly)
                .Bind(_images)
                .DisposeMany()
                .Subscribe();

            _imageService.Search(SearchParameters.Default, itemsPerPage: 50);
        }

        protected override async Task NavigatedToOverride()
        {
            // TODO: If we're coming from the Flyout menu, clear _currentSearchQuery and set the imagelist to a front page default

            // If we have an ApiImage in our navParams, we're coming back from the gallery, and need to preserve our scroll location
            var image = NavigationParameter as ApiImage;
            if (image != null)
            {
                // Send a message to the code-behind, as it has direct access to UI stuff
                _messagingService.Send(this, "", new NavigatedBackToImageGridMessage { Image = image });
                return;
            }

            // If we have an ApiTag in our navParams, then the user tapped a tag in the bottomPanel and we need to start a new search
            var apiTag = NavigationParameter as ApiTag;
            if (apiTag != null)
            {
                await SearchQueryChanged(apiTag.Name);
            }

            var queryParam = NavigationParameter as ImageGridWithQuery;
            if (queryParam != null)
            {
                await TriggerSearch(new SearchParameters
                {
                    SearchQuery = queryParam.Query,
                    SortOrder = queryParam.SortOrder ?? SortOrderKind.Descending,
                    SortProperty = queryParam.SortProperty ?? SortProperties.InitialPostDate
                });
            }
        }

        private async Task Refresh(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await _imageService.Search(new SearchParameters
                {
                    SearchQuery = "*",
                    SortOrder = _imageService.CurrentSearchParameters.SortOrder,
                    SortProperty = _imageService.CurrentSearchParameters.SortProperty,
                }, 1, 50);
            }
            else
            {
                await _imageService.Search(_imageService.CurrentSearchParameters, 1, 50);
            }
            IsRefreshing = false;
        }

        private Task SearchQueryChanged(string query)
        {
            return TriggerSearch(new SearchParameters
            {
                SearchQuery = query,
                SortOrder = _imageService.CurrentSearchParameters.SortOrder,
                SortProperty = _imageService.CurrentSearchParameters.SortProperty
            });
        }

        private Task TriggerSearch(SearchParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchQuery) || parameters.SearchQuery == "*")
            {
                Title = "Browse";
            }
            else
            {
                Title = parameters.SearchQuery;
            }

            return _imageService.Search(parameters, 1, 50);
        }

        private bool _gettingPage = false;
        private async Task GetPreviousPage()
        {
            if (_gettingPage)
            {
                return;
            }

            // Can't get anything below page 1.
            uint pageToGet = Math.Max(1, _imageService.PagesVisible.Min() - 1);

            _gettingPage = true;
            await _imageService.AddPageToSearch(pageToGet, 50);
            _gettingPage = false;
        }

        private async Task GetNextPage()
        {
            if (_gettingPage)
            {
                return;
            }

            _gettingPage = true;
            await _imageService.AddPageToSearch(_imageService.PagesVisible.Max() + 1, 50);
            _gettingPage = false;
        }

        public void ImageSelected(ApiImage selectedImage)
        {
            _navigationService.NavigateToViewModel<ImageGalleryViewModel, ApiImage>(selectedImage);
        }

        private async Task SortOptionsChanged(SearchSortOptions sortOptions)
        {
            // Only search if the sort options are actually different
            if (sortOptions.SortByProperty != _imageService.CurrentSearchParameters.SortProperty
                || sortOptions.SortOrder != _imageService.CurrentSearchParameters.SortOrder)
            {
                await TriggerSearch(new SearchParameters
                {
                    SearchQuery = CurrentSearchQuery,
                    SortOrder = sortOptions.SortOrder,
                    SortProperty = sortOptions.SortByProperty
                });
            }
        }
    }
}