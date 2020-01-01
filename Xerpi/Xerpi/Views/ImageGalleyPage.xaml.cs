using System.ComponentModel;
using System.Diagnostics;
using FFImageLoading.Forms;
using Xamarin.Forms;
using Xerpi.ViewModels;

namespace Xerpi.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ImageGalleryPage : ContentPage
    {
        private ImageGalleryViewModel _viewModel;
        private bool _bottomPanelShown = true;

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

        private async void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            if (_bottomPanelShown)
            {
                double hiddenHeight = BottomPanel.Y + BottomPanel.Height;
                await BottomPanel.TranslateTo(BottomPanel.X, hiddenHeight, 333, Easing.CubicIn);
                BottomPanel.IsVisible = false;
                _bottomPanelShown = false;
            }
            else
            {
                BottomPanel.IsVisible = true;
                BottomPanel.TranslationY = BottomPanel.Y + BottomPanel.Height;
                await BottomPanel.TranslateTo(BottomPanel.X, 0, 333, Easing.CubicOut);
                _bottomPanelShown = true;
            }
        }
    }
}