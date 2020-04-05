using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xerpi.ViewModels;

namespace Xerpi.Views
{
    public partial class ImageGalleryPage : ContentPage
    {
        private ImageGalleryViewModel _viewModel;

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

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            if (BottomPanel.IsOpen)
            {
                BottomPanel.IsOpen = false;
            }
            else
            {
                if (_viewModel?.FullSizeButtonCommand?.CanExecute(null) == true)
                {
                    _viewModel.FullSizeButtonCommand.Execute(null);
                }
            }
        }
    }
}