using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xerpi.ViewModels;

namespace Xerpi.Services
{
    public interface INavigationService
    {
        /// <summary>
        /// Navigates backward, calling any ViewModel OnBack overrides. Returns true if 
        /// back navigation completed normally, false if it was suppressed or did not occur.
        /// </summary>
        /// <returns>False if back navigation was suppressed, or did not occur.</returns>
        bool Back();
        Task NavigateToViewModel<T>() where T : BasePageViewModel;
        Task NavigateToViewModel<TViewModel, TArgs>(TArgs args) where TViewModel : BasePageViewModel;
        void RegisterViewModel<TViewModel, TPage>(string routeUrl);
    }

    public class NavigationService : INavigationService
    {
        private Dictionary<Type, Type> _vmToPageMapping = new Dictionary<Type, Type>();
        private Dictionary<Type, Type> _pageToVmMapping = new Dictionary<Type, Type>();

        public bool Back()
        {
            // Check for VM back overrides
            Page? visiblePage = (Shell.Current.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
            if (visiblePage != null)
            {
                Type vmType = _pageToVmMapping[visiblePage.GetType()];
                BasePageViewModel vmInstance = (BasePageViewModel)Startup.ServiceProvider.GetService(vmType);
                bool shouldContinue = vmInstance.OnBack();
                if (!shouldContinue)
                {
                    return false;
                }
            }

            // Default behavior
            ShellSection? currentContent = Shell.Current.CurrentItem?.CurrentItem;
            if (currentContent != null && currentContent.Stack.Count > 1)
            {
                currentContent.Navigation.PopAsync();
                return true;
            }
            return false;
        }

        public async Task NavigateToViewModel<T>() where T : BasePageViewModel
        {
            if (!(Startup.ServiceProvider.GetService(typeof(T)) is T vmInstance))
            {
                throw new ArgumentException($"Could not find a ViewModel of type {typeof(T)}");
            }

            await Shell.Current.GoToAsync(new ShellNavigationState(vmInstance.Url));
        }

        public async Task NavigateToViewModel<TViewModel, TArgs>(TArgs args) where TViewModel : BasePageViewModel
        {
            if (!(Startup.ServiceProvider.GetService(typeof(TViewModel)) is TViewModel vmInstance))
            {
                throw new ArgumentException($"Could not find a ViewModel of type {typeof(TViewModel)}");
            }

            vmInstance.NavigationParameter = args;
            await Shell.Current.GoToAsync(new ShellNavigationState(vmInstance.Url));
        }

        public void RegisterViewModel<TViewModel, TPage>(string routeUrl)
        {
            _vmToPageMapping.Add(typeof(TViewModel), typeof(TPage));
            _pageToVmMapping.Add(typeof(TPage), typeof(TViewModel));
            Routing.RegisterRoute(routeUrl, typeof(TPage));
        }
    }
}
