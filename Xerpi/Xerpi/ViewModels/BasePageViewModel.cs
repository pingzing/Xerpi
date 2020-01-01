using System;
using System.Threading.Tasks;

namespace Xerpi.ViewModels
{
    public class BasePageViewModel : BaseViewModel
    {
        public virtual string Url => throw new ArgumentException("BaseViewModel cannot be navigated to.");

        public object? NavigationParameter { get; set; }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { Set(ref title, value); }
        }

        public virtual Task NavigatedTo() { return Task.CompletedTask; }
        public virtual Task NavigatedFrom() { return Task.CompletedTask; }

        /// <summary>
        /// Function to return just before going back. Return false if back navigation should be suppressed.
        /// </summary>
        /// <returns>True if back navigation should be suppressed, false otherwise.</returns>
        public virtual bool OnBack() { return false; }
    }
}
