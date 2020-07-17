using Microsoft.Extensions.DependencyInjection;
using System;
using Xamarin.Forms;
using Xerpi.Messages;
using Xerpi.Models;
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
            _messagingService.Subscribe<NewPageWorkaroundMessage>(this, "", PrepareNewPageWorkaround);
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

        // This entire workaround is stupid, but necessary. It exists beacuse the CarouselView
        // seems to track its scroll position by using currentIndex. When we delete items
        // from the front of the collection, while simultaneously adding to the end of the
        // collection, it thinks we're still at index (end), instead of maintaining our
        // scroll position.
        // This doesn't happen until the next time the user tries to update their scroll position.
        // So instead, we hijack the scroll event, and forcibly send it to where the user expects to go.        
        private int _savedIndex;
        private InsertMode _direction;
        private void PrepareNewPageWorkaround(NewPageWorkaroundMessage args)
        {
            ImagesCarousel.Scrolled += ItemChangedWorkaround;
            _savedIndex = ViewModel.Images.IndexOf(ViewModel.CurrentImage);
            _direction = args.Direction;
        }

        private void ItemChangedWorkaround(object sender, ItemsViewScrolledEventArgs e)
        {
            ImagesCarousel.Scrolled -= ItemChangedWorkaround;
            if (_direction == InsertMode.Append)
            {
                // User is at or near the end of the gallery
                int adjustedIndex = _savedIndex - ItemsPerPage;
                ImagesCarousel.ScrollTo(adjustedIndex, animate: false);
            }
        }
    }
}