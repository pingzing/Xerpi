using System;
using Xamarin.Forms;
using Xerpi.Messages;
using Xerpi.Models;
using Xerpi.Services;
using Xerpi.ViewModels;

namespace Xerpi
{
    public partial class AppShell : Shell
    {
        private readonly INavigationService _navigationService;

        public AppShell()
        {
            InitializeComponent();
            _navigationService = (INavigationService)Startup.ServiceProvider.GetService(typeof(INavigationService));
        }

        protected override bool OnBackButtonPressed()
        {
            _navigationService.Back();
            return true; // Lie to the Shell--Nav service does all the work now.
        }

        private async void Popular_Clicked(object sender, EventArgs e)
        {
            await _navigationService.HomeToViewModel<ImageGridViewModel, ImageGridWithQuery>(new ImageGridWithQuery
            {
                Query = "first_seen_at.gte: 3 days ago",
                SortProperty = SortProperties.WilsonScore,
                SortOrder = SortOrderKind.Descending,
            });
            this.FlyoutIsPresented = false;
        }
    }
}
