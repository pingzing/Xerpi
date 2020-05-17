using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xerpi.ViewModels;
using Xerpi.ViewModels.Popups;

namespace Xerpi.Services
{
    public interface IPopupService
    {
        void RegisterViewModel<TViewModel, TPopup>();
        void RegisterViewModel(Type vmType, Type popupType);
        Task<TResult> ShowPopup<TResult>(BasePopupViewModel<TResult> popupVieWModel);
    }
}
