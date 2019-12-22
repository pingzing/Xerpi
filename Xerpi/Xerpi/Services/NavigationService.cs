using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xerpi.ViewModels;

namespace Xerpi.Services
{
    public interface INavigationService
    {
        Task NavigateToViewModel<T>() where T : BasePageViewModel;
        Task NavigateToViewModel<TViewModel, TArgs>(TArgs args) where TViewModel : BasePageViewModel;
        void RegisterViewModel<TViewModel, TPage>(string routeUrl);
    }

    public class NavigationService : INavigationService
    {
        private Dictionary<Type, Type> _vmToPageMapping = new Dictionary<Type, Type>();

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
            Routing.RegisterRoute(routeUrl, typeof(TPage));
        }
    }
}
