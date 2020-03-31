using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xerpi.Services;

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

        private void MenuItem_Clicked(object sender, EventArgs e)
        {
            FFImageLoading.ImageService.Instance.InvalidateMemoryCache();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }
    }
}
