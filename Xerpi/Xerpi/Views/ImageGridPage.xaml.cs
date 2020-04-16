using System.Linq;
using Xamarin.Forms;
using Xerpi.ViewModels;
using Xerpi.Models.API;

namespace Xerpi.Views
{
    public partial class ImageGridPage : ContentPage
    {
        ImageGridViewModel _viewModel;

        public ImageGridPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = (ImageGridViewModel)Startup.ServiceProvider.GetService(typeof(ImageGridViewModel));
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
            _viewModel.ImageSelected((ApiImage)e.CurrentSelection.First());
        }
    }
}