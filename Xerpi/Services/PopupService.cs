using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xerpi.ViewModels;
using Xerpi.ViewModels.Popups;

namespace Xerpi.Services
{
    public class PopupService : IPopupService
    {
        private readonly IPopupNavigation _popupNavigation;

        private Dictionary<Type, Type> _vmToPopupMapping = new Dictionary<Type, Type>();

        public PopupService(IPopupNavigation popupNavigation)
        {
            _popupNavigation = popupNavigation;
        }

        public void RegisterViewModel<TViewModel, TPopup>()
        {
            RegisterViewModel(typeof(TViewModel), typeof(TPopup));
        }

        public void RegisterViewModel(Type vmType, Type popupType)
        {
            _vmToPopupMapping.Add(vmType, popupType);
        }

        public async Task<TResult> ShowPopup<TResult>(BasePopupViewModel<TResult> popupVieWModel)
        {
            // Create new popup associated with VM of given type
            if (!_vmToPopupMapping.TryGetValue(popupVieWModel.GetType(), out Type popupType))
            {
                throw new ArgumentOutOfRangeException($"Seems like {popupVieWModel} hasn't been registerd with the PopupService. Try again!");
            }

            ConstructorInfo popupPageConstructor = popupType.GetConstructor(Type.EmptyTypes);
            var page = (PopupPage)popupPageConstructor.Invoke(null);

            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            page.Disappearing += (s, e) =>
            {
                var vm = (BasePopupViewModel<TResult>)page.BindingContext;
                tcs.SetResult(vm.Result);
            };

            // Navigate to it            
            await _popupNavigation.PushAsync(page);

            // Wait for it to close
            return await tcs.Task;
        }
    }
}
