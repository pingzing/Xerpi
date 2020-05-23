using Microsoft.Extensions.DependencyInjection;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Pages;
using System;
using System.Diagnostics;

namespace Xerpi.Views.Popups
{
    public partial class SortFilterPopup : PopupPage
    {
        private readonly IPopupNavigation _popupNavigation;
        private readonly SortFilterPopupViewModel _viewModel;

        public SortFilterPopup()
        {
            InitializeComponent();
            _viewModel = Startup.ServiceProvider.GetRequiredService<SortFilterPopupViewModel>();
            BindingContext = _viewModel;
            _popupNavigation = Startup.ServiceProvider.GetRequiredService<IPopupNavigation>();

            _viewModel.Closed += ViewModel_Closed;
        }

        protected override void OnDisappearing()
        {
            _viewModel.Closed -= ViewModel_Closed;
            base.OnDisappearing();
        }

        private void ViewModel_Closed(object sender, System.EventArgs e)
        {
            try
            {
                _popupNavigation.RemovePageAsync(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OHSHI- Exception when closing SortFilterPopup and trying to remove page from navigation! {ex}");
            }

        }
    }
}