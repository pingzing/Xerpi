using Microsoft.Extensions.DependencyInjection;
using System;
using Xamarin.Forms;
using Xerpi.Messages;
using Xerpi.ViewModels;

namespace Xerpi.Views
{
    public partial class ImageGalleryPage : NavigablePage
    {
        // This will probably have to become a setting. Right now, it's hardcoded in various places around the app.
        private const int ItemsPerPage = 50;
        private readonly IMessagingCenter _messagingService;
        private ImageGalleryViewModel ViewModel => (ImageGalleryViewModel)_viewModel;

        public ImageGalleryPage() : base(typeof(ImageGalleryViewModel))
        {
            InitializeComponent();
            BindingContext = ViewModel;
            _messagingService = Startup.ServiceProvider.GetRequiredService<IMessagingCenter>();
        }

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            if (BottomPanel.IsOpen)
            {
                BottomPanel.IsOpen = false;
            }
            else
            {
                if (ViewModel?.FullSizeButtonCommand?.CanExecute(null) == true)
                {
                    ViewModel.FullSizeButtonCommand.Execute(null);
                }
            }
        }
    }
}