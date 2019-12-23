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

        public Command<DetailedImageViewModel> CurrentImageChangedCommand { get; private set; }

        public ImageGalleryViewModel(IImageService imageService)
        {
            _imageService = imageService;
            _images = new ReadOnlyObservableCollection<DetailedImageViewModel>(new ObservableCollection<DetailedImageViewModel>());

            CurrentImageChangedCommand = new Command<DetailedImageViewModel>(CurrentImageChanged);
        }

        private async void CurrentImageChanged(DetailedImageViewModel newImage)
        {
            await newImage.InitExternalData();
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

        public override Task NavigatedFrom()
        {
            return base.NavigatedFrom();
        }
    }
}
