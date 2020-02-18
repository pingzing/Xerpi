using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Android.Content;
using FFImageLoading;

namespace Xerpi.Droid
{
    [Activity(Label = "Xerpi", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Forms.SetFlags("CollectionView_Experimental", "UseLegacyRenderers", "CarouselView_Experimental"); // Using legacy renderers until the Label FastRenderer crashes are fixed.
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            Forms.Init(this, savedInstanceState);
            Startup.Init(ConfigureServices);
            LoadApplication(new App());
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Clear the FFImageLoading memory cache when we're low on memory.
        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            ImageService.Instance.InvalidateMemoryCache();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            base.OnTrimMemory(level);
        }
    }
}