using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Navigates backward, calling any ViewModel OnBack overrides. Returns true if 
        /// back navigation completed normally, false if it was suppressed or did not occur.
        /// Sends an argument of type <see cref="{TArgs}"/> to the destination page's NavigationParameter property.
        /// </summary>
        /// <returns>False if back navigation was suppressed, or did not occur.</returns>
        bool Back<TArgs>(TArgs args);
        Task NavigateToViewModel<T>() where T : BasePageViewModel;
        Task NavigateToViewModel<TViewModel, TArgs>(TArgs args) where TViewModel : BasePageViewModel;
        Task HomeToViewModel<TViewModel, TArgs>(TArgs args) where TViewModel : BasePageViewModel;
        void RegisterViewModel<TViewModel, TPage>(string routeUrl);
    }

    public class NavigationService : INavigationService
    {
        private Dictionary<Type, Type> _vmToPageMapping = new Dictionary<Type, Type>();
        private Dictionary<Type, Type> _pageToVmMapping = new Dictionary<Type, Type>();

        // Return true if we went back, false otherwise.
        public bool Back()
        {
            // Check for VM back overrides
            bool shouldContinue = RunVmBackOverrides();
            if (!shouldContinue)
            {
                return false;
            }

            // Default behavior
            ShellSection? currentContent = Shell.Current.CurrentItem?.CurrentItem;
            if (currentContent == null || currentContent.Stack?.Count <= 1)
            {
                return false;
            }

            currentContent.Navigation.PopAsync();
            return true;
        }

        // Return true if we went back, false otherwise.
        public bool Back<TArgs>(TArgs args)
        {
            // Check for VM back overrides
            bool shouldContinue = RunVmBackOverrides();
            if (!shouldContinue)
            {
                return false;
            }

            // Default behavior, with added argumenty goodness
            ShellSection? currentContent = Shell.Current.CurrentItem?.CurrentItem;
            if (currentContent == null || currentContent.Stack?.Count <= 1)
            {
                return false;
            }

            int destinationIndex = currentContent.Navigation.NavigationStack.Count - 2; // -1 for previous, -1 to offset index vs .Count
            Page destinationPage;
            if (destinationIndex == 0)
            {
                destinationPage = ((IShellContentController)Shell.Current.CurrentItem.CurrentItem.CurrentItem).Page;
            }
            else
            {
                destinationPage = currentContent.Navigation.NavigationStack[destinationIndex];
            }
            Type destinationVmType = _pageToVmMapping[destinationPage.GetType()];
            BasePageViewModel destinationVmInstance = (BasePageViewModel)Startup.ServiceProvider.GetService(destinationVmType);
            destinationVmInstance.NavigationParameter = args;

            currentContent.Navigation.PopAsync();
            return true;
        }

        // Return true if we should continue, false if Back has been suppressed by the VM.
        private bool RunVmBackOverrides()
        {
            Page? visiblePage = (Shell.Current.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
            if (visiblePage == null)
            {
                return false;
            }

            Type vmType = _pageToVmMapping[visiblePage.GetType()];
            BasePageViewModel vmInstance = (BasePageViewModel)Startup.ServiceProvider.GetService(vmType);
            bool shouldContinue = vmInstance.OnBack();
            return shouldContinue;
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

        public async Task HomeToViewModel<TViewModel, TArgs>(TArgs args) where TViewModel : BasePageViewModel
        {
            if (!(Startup.ServiceProvider.GetService(typeof(TViewModel)) is TViewModel vmInstance))
            {
                throw new ArgumentException($"Could not find a ViewModel of type {typeof(TViewModel)}");
            }

            vmInstance.NavigationParameter = args;
            await Shell.Current.GoToAsync(new ShellNavigationState($"//{vmInstance.Url}"));
        }

        public void RegisterViewModel<TViewModel, TPage>(string routeUrl)
        {
            _vmToPageMapping.Add(typeof(TViewModel), typeof(TPage));
            _pageToVmMapping.Add(typeof(TPage), typeof(TViewModel));
            Routing.RegisterRoute(routeUrl, typeof(TPage));
        }
    }
}
