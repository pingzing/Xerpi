using FFImageLoading.Forms;
using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xerpi.UWP
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {

                rootFrame = new Frame();

                Xamarin.Forms.Forms.SetFlags("Shell_UWP_Experimental", "CollectionView_Experimental", "CarouselView_Experimental");
                FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

                var assembliesToInclude = new List<Assembly>
                {
                    typeof(CachedImage).GetTypeInfo().Assembly,
                    typeof(FFImageLoading.Forms.Platform.CachedImageRenderer).GetTypeInfo().Assembly
                };

                Xamarin.Forms.Forms.Init(e, assembliesToInclude);

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }
    }
}
