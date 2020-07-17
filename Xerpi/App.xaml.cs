using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Xerpi
{
    public partial class App : Application
    {

        public App()
        {
#if DEBUG // Dumb stupid workaround because dumb stupid framework doesn't report dumb stupid errors
            Log.Listeners.Add(new DelegateLogListener((arg1, arg2) => Debug.WriteLine(arg2)));
#endif

            InitializeComponent();
            Startup.ThemeHandler!.Initialize();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
