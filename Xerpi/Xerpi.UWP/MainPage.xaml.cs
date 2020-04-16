using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Windows.UI.ViewManagement;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using Xerpi.Messages;

namespace Xerpi.UWP
{
    public sealed partial class MainPage : WindowsPage
    {
        private UISettings uiSettings;

        public MainPage()
        {
            InitializeComponent();
            Startup.Init(ConfigureServices);
            MR.Gestures.UWP.Settings.LicenseKey = "ALZ9-BPVU-XQ35-CEBG-5ZRR-URJQ-ED5U-TSY8-6THP-3GVU-JW8Z-RZGE-CQW6";
            LoadApplication(new Xerpi.App());

            uiSettings = new UISettings();
            uiSettings.ColorValuesChanged += UISettings_ColorValuesChanged;
        }

        private void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var messagingService = Startup.ServiceProvider.GetRequiredService<IMessagingCenter>();
                messagingService.Send(new object(), SimpleMessages.SystemThemeChanged);
            });
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }
    }
}
