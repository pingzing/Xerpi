using System.ComponentModel;
using Xamarin.Forms;
using Xerpi.ViewModels;

namespace Xerpi.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ImageGalleryPage : ContentPage
    {
        ImageGalleryViewModel _viewModel;

        public ImageGalleryPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = (ImageGalleryViewModel)Startup.ServiceProvider.GetService(typeof(ImageGalleryViewModel));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.NavigatedTo();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _viewModel.NavigatedFrom();
        }
    }
}