using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xerpi.ViewModels;

namespace Xerpi.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ImageGalleryPage : ContentPage
    {
        private readonly IMessagingCenter _messenger;
        private ImageGalleryViewModel _viewModel;
        private bool _bottomPanelMaximized = true;

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