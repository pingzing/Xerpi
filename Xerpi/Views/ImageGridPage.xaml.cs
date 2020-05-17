using System.Linq;
using Xamarin.Forms;
using Xerpi.ViewModels;
using Xerpi.Models.API;
using Microsoft.Extensions.DependencyInjection;
using Xerpi.Messages;
using Xerpi.Models;

namespace Xerpi.Views
{
    public partial class ImageGridPage : NavigablePage
    {
        private readonly IMessagingCenter _messagingService;
        private ImageGridViewModel ViewModel => (ImageGridViewModel)_viewModel;

        public ImageGridPage() : base(typeof(ImageGridViewModel))
        {
            InitializeComponent();
            BindingContext = ViewModel;
            _messagingService = Startup.ServiceProvider.GetRequiredService<IMessagingCenter>();
            _messagingService.Subscribe<ImageGridViewModel, NavigatedBackToImageGridMessage>(this, "", OnNavigatedFromGallery);
        }

        private void OnNavigatedFromGallery(ImageGridViewModel _, NavigatedBackToImageGridMessage args)
        {
            ImageListCollectionView.ScrollTo(args.Image, position: ScrollToPosition.Start, animate: false);
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
            {
                return;
            }

            CollectionView? cv = sender as CollectionView;
            if (cv == null)
            {
                return;
            }
            cv.SelectedItem = null;
            ViewModel.ImageSelected((ApiImage)e.CurrentSelection.First());
        }

        private void TitleSearch_SearchSortOptionsChanged(object sender, SearchSortOptions newOptions)
        {
            if (ViewModel.SortOptionsChangedCommand.CanExecute(newOptions))
            {
                ViewModel.SortOptionsChangedCommand.Execute(newOptions);
            }
        }
    }
}