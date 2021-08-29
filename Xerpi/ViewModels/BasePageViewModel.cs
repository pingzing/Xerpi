using System;
using System.Threading.Tasks;

namespace Xerpi.ViewModels
{
    public abstract class BasePageViewModel : BaseViewModel
    {
        public virtual string Url => throw new ArgumentException("BaseViewModel cannot be navigated to.");

        public object? NavigationParameter { get; set; }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { Set(ref title, value); }
        }

        public async Task NavigatedTo()
        {
            await NavigatedToOverride();
            NavigationParameter = null;
        }
        protected virtual Task NavigatedToOverride() { return Task.CompletedTask; }

        public async Task NavigatedFrom()
        {
            await NavigatedFromOverride();
            NavigationParameter = null;
        }
        public virtual Task NavigatedFromOverride() { return Task.CompletedTask; }

        /// <summary>
        /// Function to return just before going back. Return true if back navigation should continue, false if it should be suppressed.
        /// </summary>
        /// <returns>True if back navigation should be continued, false if it should be suppressed.</returns>
        public virtual bool OnBack() { return false; }
    }
}
