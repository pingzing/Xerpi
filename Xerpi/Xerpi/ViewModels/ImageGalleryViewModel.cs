using DynamicData;
using DynamicData.Binding;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
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
        private readonly SortExpressionComparer<ApiImage> _imageSorter = SortExpressionComparer<ApiImage>.Descending(x => x.Id);

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

        public Command<DetailedImageViewModel> CurrentImageChangedCommand { get; private set; }
        public Command SoftBackPressedCommand { get; private set; }
        public Command FullSizeButtonCommand { get; private set; }

        public ImageGalleryViewModel(IImageService imageService,
            INavigationService navigationService,
            IDerpiNetworkService networkSerivce,
            ISynchronizationContextService syncContextService)
        {
            _imageService = imageService;
            _navigationService = navigationService;
            _networkService = networkSerivce;
            _syncContextService = syncContextService;
            _images = new ReadOnlyObservableCollection<DetailedImageViewModel>(new ObservableCollection<DetailedImageViewModel>());

            CurrentImageChangedCommand = new Command<DetailedImageViewModel>(CurrentImageChanged);
            SoftBackPressedCommand = new Command(SoftBackPressed);
            FullSizeButtonCommand = new Command(FullSizeButtonPressed);
        }

        protected override Task NavigatedToOverride()
        {
            _backPayloadPrepared = false;
            _navParameterImage = NavigationParameter as ApiImage;

            var operation = _imageService.CurrentImages.Connect()
             .Filter(x => !x.MimeType.Contains("video")) // TODO: Make sure this only covers webm, and not other things we can actually handle
             .Sort(_imageSorter)
             .Transform(x => new DetailedImageViewModel(x, _imageService, _networkService, _syncContextService))
             .ObserveOn(_syncContextService.UIThread)
             .Bind(out _images, resetThreshold: 75)
             .DisposeMany()
             .Subscribe(x =>
             {
                 if (_navParameterImage != null)
                 {
                     var foundImage = Images.FirstOrDefault(x => x.BackingImage.Id == _navParameterImage.Id);
                     if (foundImage != null)
                     {
                         CurrentImage = foundImage;
                     }
                     _navParameterImage = null;
                 }
             });

            OnPropertyChanged(nameof(Images));

            return Task.CompletedTask;
        }

        private async void CurrentImageChanged(DetailedImageViewModel newImage)
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            await newImage.InitExternalData(_cts.Token);

            // TODO: Display some kind of indicator while this is happening
            // TODO: Prevent this from triggering more than once simultaneously
            if (newImage.BackingImage.Id == Images.Last().BackingImage.Id)
            {
                _ = await _imageService.AddPageToSearch(_imageService.PagesSeen.Max() + 1, 50);
            }
        }

        private bool _backPayloadPrepared = false;
        public override bool OnBack()
        {
            if (IsImageViewerOpen)
            {
                IsImageViewerOpen = false;
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
        }

        private void SoftBackPressed()
        {
            _navigationService.Back();
        }

        public override Task NavigatedFromOverride()
        {
            return base.NavigatedFromOverride();
        }
    }
}
