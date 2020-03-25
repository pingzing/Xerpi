using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Xamarin.Forms;
using Xerpi.Models.API;
using Xerpi.Services;

namespace Xerpi.ViewModels
{
    public class ImagesViewModel : BasePageViewModel
    {
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly SortExpressionComparer<ApiImage> _imageSorter = SortExpressionComparer<ApiImage>.Descending(x => x.Id);

        private uint _currentPage = 1;

        public override string Url => "images";

        private string? _currentSearchQuery = null;
        public string? CurrentSearchQuery
        {
            get => _currentSearchQuery;
            set => Set(ref _currentSearchQuery, value);
        }

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

        public Command RefreshCommand { get; }
        public Command<string> SearchTriggeredCommand { get; }
        public Command GetNextPageCommand { get; }

        public ImagesViewModel(IImageService imageService,
            INavigationService navigationService)
        {
            _imageService = imageService;
            _navigationService = navigationService;

            Title = "Browse";
            RefreshCommand = new Command(async () => await Refresh());
            SearchTriggeredCommand = new Command<string>(async x => await SearchTriggered(x));
            GetNextPageCommand = new Command(async x => await GetNextPage());

            var operation = _imageService.CurrentImages.Connect()
                .Filter(x => !x.MimeType.Contains("video")) // TODO: Make sure this only covers webm, and not other things we can actually handle
                .Sort(_imageSorter)
                .Bind(out _images, resetThreshold: 75)
                .DisposeMany()
                .Subscribe();

            _imageService.SetImagesToFrontPage(itemsPerPage: 50);
        }

        public override async Task NavigatedTo()
        {
            // TODO: If we're coming from the Flyout menu, clear _currentSearchQuery and set the imagelist to a front page default
            // Otherwise, do nothing
        }

        private async Task Refresh()
        {
            if (CurrentSearchQuery == null)
            {
                await _imageService.SetImagesToFrontPage();
            }
            else
            {
                await _imageService.SetImagesToSearch(CurrentSearchQuery, 1, 50);
            }
            IsRefreshing = false;
        }

        private async Task SearchTriggered(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
            {
                CurrentSearchQuery = null;
                Title = "Browse";
                await Refresh();
                return;
            }

            CurrentSearchQuery = query;
            Title = query;
            _currentPage = 1;
            await _imageService.SetImagesToSearch(query, 1, 50);
        }

        private bool _gettingNextPage = false;
        private async Task GetNextPage()
        {
            if (_gettingNextPage)
            {
                return;
            }

            _gettingNextPage = true;
            if (CurrentSearchQuery == null)
            {
                await _imageService.AddPageToFrontPage(_currentPage + 1, 50);
            }
            else
            {
                await _imageService.AddPageToSearch(CurrentSearchQuery, _currentPage + 1, 50);
            }
            _currentPage += 1;
            _gettingNextPage = false;
        }

        public void ImageSelected(ApiImage selectedImage)
        {
            _navigationService.NavigateToViewModel<ImageGalleryViewModel, ApiImage>(selectedImage);
        }
    }
}