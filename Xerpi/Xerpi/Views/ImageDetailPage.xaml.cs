using System.ComponentModel;
using Xamarin.Forms;
using Xerpi.ViewModels;

namespace Xerpi.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ImageDetailPage : ContentPage
    {
        ImageDetailViewModel _viewModel;

        public ImageDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = (ImageDetailViewModel)Startup.ServiceProvider.GetService(typeof(ImageDetailViewModel));
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