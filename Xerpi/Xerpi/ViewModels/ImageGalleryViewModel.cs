using DynamicData;
using DynamicData.Binding;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            INavigationService navigationService)
        {
            _imageService = imageService;
            _navigationService = navigationService;
            _images = new ReadOnlyObservableCollection<DetailedImageViewModel>(new ObservableCollection<DetailedImageViewModel>());

            CurrentImageChangedCommand = new Command<DetailedImageViewModel>(CurrentImageChanged);
            SoftBackPressedCommand = new Command(SoftBackPressed);
            FullSizeButtonCommand = new Command(FullSizeButtonPressed);
        }

        private async void CurrentImageChanged(DetailedImageViewModel newImage)
        {
            await newImage.InitExternalData();
        }

        public override bool OnBack()
        {
            if (IsImageViewerOpen)
            {
                IsImageViewerOpen = false;
                return false;
            }

            return true;
        }

        private void FullSizeButtonPressed(object obj)
        {
            IsImageViewerOpen = true;
        }

        public override Task NavigatedTo()
        {
            var operation = _imageService.CurrentImages.Connect()
                .Filter(x => !x.Image.EndsWith(".webm"))
                .Sort(SortExpressionComparer<ApiImage>.Descending(x => x.Id))
                .Transform(x => new DetailedImageViewModel(x, _imageService))
                .Bind(out _images)
                .DisposeMany()
                .Subscribe();

            OnPropertyChanged(nameof(Images));

            if (NavigationParameter is ApiImage image)
            {
                var foundImage = Images.FirstOrDefault(x => x.BackingImage.Id == image.Id);
                if (foundImage != null)
                {
                    CurrentImage = foundImage;
                }
            }

            return Task.CompletedTask;
        }

        private void SoftBackPressed()
        {
            _navigationService.Back();
        }

        public override Task NavigatedFrom()
        {
            return base.NavigatedFrom();
        }
    }
}
