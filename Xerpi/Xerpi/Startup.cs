using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using Xamarin.Essentials;
using Xerpi.Services;
using Xerpi.ViewModels;
using System.Net.Http.Headers;
using Xerpi.Views;
using Xamarin.Forms;

namespace Xerpi
{
    public static class Startup
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static void Init(Action<HostBuilderContext, IServiceCollection> nativeConfigureServices)
        {
            var configFilePath = ExtractResource("Xerpi.appsettings.json", FileSystem.AppDataDirectory);

            var host = new HostBuilder()
                .ConfigureHostConfiguration(c =>
                {
                    c.AddCommandLine(new string[] { $"ContentRoot={FileSystem.AppDataDirectory}" });
                    c.AddJsonFile(configFilePath);
                }).ConfigureServices((c, x) =>
                {
                    nativeConfigureServices(c, x);
                    ConfigureServices(c, x);
                }).ConfigureLogging(l => l.AddConsole(o =>
                {
                    o.DisableColors = true;
                })).Build();

            RegisterNavigationServiceRoutes(host.Services);
            ServiceProvider = host.Services;
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                // mocks
            }
            else
            {
                // non-dev only
            }

            services.AddHttpClient<IDerpiNetworkService, DerpiNetworkService>(x =>
            {
                x.BaseAddress = new Uri("https://derpibooru.org");
                x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                x.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("pingzing-Xerpi", "1.0"));
            });

            // ViewModel singletons
            services.AddSingleton<INavigationService, NavigationService>()
                .AddSingleton<ISynchronizationContextService, SynchronizationContextService>()
                .AddTransient<ISettingsService, SettingsService>()
                .AddSingleton<IImageService, ImageService>()
                .AddSingleton<IMessagingCenter, MessagingCenter>(_ => (MessagingCenter)MessagingCenter.Instance)
                .AddSingleton<ImagesViewModel>()
                .AddSingleton<ImageGalleryViewModel>()
                .AddSingleton<AboutViewModel>()
                .AddSingleton<SettingsViewModel>();
        }

        private static void RegisterNavigationServiceRoutes(IServiceProvider services)
        {
            var navService = services.GetRequiredService<INavigationService>();
            navService.RegisterViewModel<ImagesViewModel, ImagesPage>("images");
            navService.RegisterViewModel<ImageGalleryViewModel, ImageGalleryPage>("imagegallery");
            navService.RegisterViewModel<AboutViewModel, AboutPage>("about");
            navService.RegisterViewModel<SettingsViewModel, SettingsPage>("settings");
        }

        private static string ExtractResource(string fileName, string location)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream resFileStream = a.GetManifestResourceStream(fileName))
            {
                if (resFileStream != null)
                {
                    string full = Path.Combine(location, fileName);
                    using (FileStream stream = File.Create(full))
                    {
                        resFileStream.CopyTo(stream);
                    }
                }
            }
            return Path.Combine(location, fileName);
        }
    }
}
