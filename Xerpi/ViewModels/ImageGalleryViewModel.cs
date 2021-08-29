using DynamicData;
using DynamicData.Binding;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xerpi.Models;
using Xerpi.Models.API;
using Xerpi.Services;

namespace Xerpi.ViewModels
{
    public class ImageGalleryViewModel : BasePageViewModel
    {
        public override string Url => "imagegallery";
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly IDerpiNetworkService _networkService;
        private readonly ISynchronizationContextService _syncContextService;
        private readonly IMessagingCenter _messagingService;
        private readonly SearchPageComparer _pageComparer = new SearchPageComparer();

        private ApiImage? _navParameterImage;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private DetailedImageViewModel? _currentImage;
        public DetailedImageViewModel CurrentImage
        {
            get => _currentImage;
            set => Set(ref _currentImage, value);
        }

        private ReadOnlyObservableCollection<DetailedImageViewModel> _images;
        public ReadOnlyObservableCollection<DetailedImageViewModel> Images
        {
            get => _images;
            set => Set(ref _images, value);
        }

        private bool _isImageViewerOpen = false;
        public bool IsImageViewerOpen
        {
            get => _isImageViewerOpen;
            set => Set(ref _isImageViewerOpen, value);
        }

        private int _currentImageNumber = 0;
        public int CurrentImageNumber
        {
            get => _currentImageNumber;
            set => Set(ref _currentImageNumber, value);
        }

        public uint CurrentTotalImages => _imageService.CurrentTotalImages;

        public Command<DetailedImageViewModel> CurrentImageChangedCommand { get; private set; }
        public Command SoftBackPressedCommand { get; private set; }
        public Command FullSizeButtonCommand { get; private set; }
        public Command ThresholdReachedCommand { get; private set; }
        public Command OpenInBrowserCommand { get; private set; }
        public Command<ApiTag> TagTappedCommand { get; private set; }

        public ImageGalleryViewModel(IImageService imageService,
            INavigationService navigationService,
            IDerpiNetworkService networkSerivce,
            ISynchronizationContextService syncContextService,
            IMessagingCenter messagingService)
        {
            _imageService = imageService;
            _navigationService = navigationService;
            _networkService = networkSerivce;
            _syncContextService = syncContextService;
            _messagingService = messagingService;
            _images = new ReadOnlyObservableCollection<DetailedImageViewModel>(new ObservableCollectionExtended<DetailedImageViewModel>());

            CurrentImageChangedCommand = new Command<DetailedImageViewModel>(CurrentImageChanged);
            SoftBackPressedCommand = new Command(SoftBackPressed);
            FullSizeButtonCommand = new Command(FullSizeButtonPressed);
            ThresholdReachedCommand = new Command(ThresholdReached);
            OpenInBrowserCommand = new Command(OpenInBrowserPressed);
            TagTappedCommand = new Command<ApiTag>(TagTapped);

            _imageService.CurrentImages.Connect()
                 .Filter(x => !x.MimeType.Contains("video")) // TODO: Make sure this only covers webm, and not other things we can actually handle
                 .Sort(_pageComparer, SortOptimisations.ComparesImmutableValuesOnly)
                 .Transform(x => new DetailedImageViewModel(x, _imageService, _networkService))
                 .ObserveOn(_syncContextService.UIThread)
                 .Bind(out _images, resetThreshold: 75)
                 .DisposeMany()
                 .Subscribe(x =>
                 {
                     if (_navParameterImage != null)
                     {
                         NavigateToSelectedImage();
                     }
                 });
        }

        private async void OpenInBrowserPressed()
        {
            await Browser.OpenAsync($"{_networkService.BaseUri}images/{CurrentImage.BackingImage.Id}", BrowserLaunchMode.External);
        }

        private bool _gettingPage = false;
        private async void ThresholdReached()
        {
            if (_gettingPage)
            {
                return;
            }

            _gettingPage = true;
            await _imageService.AddPageToSearch(_imageService.PagesVisible.Max() + 1, 50);
            _gettingPage = false;
        }

        private void TagTapped(ApiTag tag)
        {
            _backPayloadPrepared = true;
            _navigationService.Back(tag);
            _backPayloadPrepared = false;
        }

        private void NavigateToSelectedImage()
        {
            var foundImage = Images.FirstOrDefault(x => x.BackingImage.Id == _navParameterImage?.Id);
            if (foundImage != null)
            {
                CurrentImage = foundImage;
                _navParameterImage = null;
            }
        }

        private async void CurrentImageChanged(DetailedImageViewModel newImage)
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            Title = $"{newImage.BackingImage.Id}";
            CurrentImageNumber = CurrentImageNumber = Images.IndexOf(CurrentImage) + 1;
            await newImage.InitExternalData(_cts.Token);
        }

        private bool _backPayloadPrepared = false;
        public override bool OnBack()
        {
            if (IsImageViewerOpen)
            {
                IsImageViewerOpen = false;
                Title = $"{CurrentImage.BackingImage.Id}";
                return false;
            }

            if (!_backPayloadPrepared)
            {
                _backPayloadPrepared = true;
                _navigationService.Back(CurrentImage.BackingImage);
                return false;
            }

            return true;
        }

        private void FullSizeButtonPressed()
        {
            IsImageViewerOpen = true;
            Title = $"{CurrentImage.BackingImage.Id}: Large";
        }

        private void SoftBackPressed()
        {
            _navigationService.Back();
        }

        protected override Task NavigatedToOverride()
        {
            _backPayloadPrepared = false;
            _navParameterImage = NavigationParameter as ApiImage;
            NavigateToSelectedImage();
            OnPropertyChanged(nameof(CurrentTotalImages));

            return Task.CompletedTask;
        }

        public override Task NavigatedFromOverride()
        {
            return base.NavigatedFromOverride();
        }
    }
}
