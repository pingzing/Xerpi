using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Xamarin.Forms;

using Xerpi.Models;
using Xerpi.Models.API;
using Xerpi.Services;
using Xerpi.Views;

namespace Xerpi.ViewModels
{
    public class ImagesViewModel : BasePageViewModel
    {
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;

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

        public Command RefreshCommand { get; set; }

        public ImagesViewModel(IImageService imageService,
            INavigationService navigationService)
        {
            Title = "Browse";
            RefreshCommand = new Command(async () => await Refresh());
            _imageService = imageService;
            _navigationService = navigationService;

            var operation = _imageService.CurrentImages.Connect()
                .Sort(SortExpressionComparer<ApiImage>.Descending(x => x.Id))
                .Bind(out _images)
                .DisposeMany()
                .Subscribe();

            _imageService.UpdateFrontPage();
        }

        public override async Task NavigatedTo()
        {

        }

        private async Task Refresh()
        {
            await _imageService.UpdateFrontPage();
            IsRefreshing = false;
        }

        public void ImageSelected(ApiImage selectedImage)
        {
            _navigationService.NavigateToViewModel<ImageGalleryViewModel, ApiImage>(selectedImage);
        }
    }
}