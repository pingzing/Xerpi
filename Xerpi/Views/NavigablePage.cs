using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xerpi.ViewModels;

namespace Xerpi.Views
{
    public abstract class NavigablePage : ContentPage
    {
        protected BasePageViewModel _viewModel;

        public NavigablePage(Type viewModelType)
        {
            _viewModel = (BasePageViewModel)Startup.ServiceProvider.GetService(viewModelType);
        }

        protected sealed override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.NavigatedTo();
            await OnAppearingOverride();
        }
        protected virtual Task OnAppearingOverride() { return Task.CompletedTask; }

        protected sealed override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _viewModel.NavigatedFrom();
            await OnDisappearingOverride();
        }
        protected virtual Task OnDisappearingOverride() { return Task.CompletedTask; }
    }
}
