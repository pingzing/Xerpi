using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xamarin.Forms.Platform.UWP;

namespace Xerpi.UWP
{
    public sealed partial class MainPage : WindowsPage
    {
        public MainPage()
        {
            InitializeComponent();
            Startup.Init(ConfigureServices);
            LoadApplication(new Xerpi.App());
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }
    }
}
