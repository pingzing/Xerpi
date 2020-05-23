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

        public override string Url => "images";

        private ReadOnlyObservableCollection<ApiImage> _images;
        public ReadOnlyObservableCollection<ApiImage> Images
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
            SortOptionsChangedCommand = new Command<SearchSortOptions>(async x => await SortOptionsChanged(x));

            var operation = _imageService.CurrentImages.Connect()
                .Filter(x => !x.MimeType.Contains("video")) // TODO: Make sure this only covers webm, and not other things we can actually handle                
                .Bind(out _images, resetThreshold: 75)
                .DisposeMany()
                .Subscribe();

            _imageService.Search(SearchParameters.Default, itemsPerPage: 50);
        }

        protected override async Task NavigatedToOverride()
        {
            // TODO: If we're coming from the Flyout menu, clear _currentSearchQuery and set the imagelist to a front page default
            // Otherwise, do nothing
            var image = NavigationParameter as ApiImage;
            if (image != null)
            {
                _messagingService.Send(this, "", new NavigatedBackToImageGridMessage { Image = image });
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
            if (string.IsNullOrWhiteSpace(query))
            {
                Title = "Browse";
            }
            else
            {
                Title = query;
            }

            return TriggerSearch(new SearchParameters
            {
                SearchQuery = query,
                SortOrder = _imageService.CurrentSearchParameters.SortOrder,
                SortProperty = _imageService.CurrentSearchParameters.SortProperty
            });
        }

        private Task TriggerSearch(SearchParameters parameters)
        {
            return _imageService.Search(parameters, 1, 50);
        }

        private bool _gettingNextPage = false;
        private async Task GetNextPage()
        {
            if (_gettingNextPage)
            {
                return;
            }

            _gettingNextPage = true;
            await _imageService.AddPageToSearch(_imageService.PagesSeen.Max() + 1, 50);
            _gettingNextPage = false;
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