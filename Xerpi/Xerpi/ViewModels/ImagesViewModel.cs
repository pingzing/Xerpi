using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Xerpi.Models;
using Xerpi.Models.API;
using Xerpi.Services;
using Xerpi.Views;

namespace Xerpi.ViewModels
{
    public class ImagesViewModel : BaseViewModel
    {
        private readonly IDerpiNetworkService _derpiNetworkService;
        private readonly INavigationService _navigationService;
        private readonly IDerpiNetworkService _networkService;

        public override string Url => "images";

        public ObservableCollection<ApiImage> Images { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ImagesViewModel(IDerpiNetworkService derpiNetworkService,
            INavigationService navigationService)
        {
            Title = "Browse";
            Images = new ObservableCollection<ApiImage>();
            LoadItemsCommand = new Command(async () => await LoadMoreItems());
            _derpiNetworkService = derpiNetworkService;
            _navigationService = navigationService;
        }

        public override async Task NavigatedTo()
        {
            var first15Images = await _derpiNetworkService.GetImages();
            if (first15Images != null)
            {
                Images.Clear();
                foreach (var image in first15Images.Images)
                {
                    Images.Add(image);
                }
            }
        }

        async Task LoadMoreItems()
        {

        }

        public void ImageSelected(ApiImage selectedImage)
        {
            _navigationService.NavigateToViewModel<ImageDetailViewModel, ApiImage>(selectedImage);
        }
    }
}