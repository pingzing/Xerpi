using Xerpi.ViewModels;

namespace Xerpi.Views
{
    public partial class SettingsPage : NavigablePage
    {
        private SettingsViewModel ViewModel => (SettingsViewModel)_viewModel;

        public SettingsPage() : base(typeof(SettingsViewModel))
        {
            InitializeComponent();
            BindingContext = ViewModel;
        }
    }
}