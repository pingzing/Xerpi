using System;

namespace Xerpi.ViewModels.Popups
{
    public class BasePopupViewModel<T> : BaseViewModel
    {
        public event EventHandler? Closed;

        public T Result { get; protected set; }

        protected void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
