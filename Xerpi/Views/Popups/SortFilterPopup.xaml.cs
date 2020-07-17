using Microsoft.Extensions.DependencyInjection;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Pages;


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
            _popupNavigation.RemovePageAsync(this);
        }
    }
}