using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xerpi.Models;
using Xerpi.Views;
using Xerpi.ViewModels;
using Xerpi.Models.API;

namespace Xerpi.Views
{
    public partial class ImagesPage : ContentPage
    {
        ImagesViewModel _viewModel;

        public ImagesPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = (ImagesViewModel)Startup.ServiceProvider.GetService(typeof(ImagesViewModel));
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