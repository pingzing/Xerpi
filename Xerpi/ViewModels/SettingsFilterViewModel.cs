using Xamarin.Forms;
using Xerpi.Messages;
using Xerpi.Models.API;
using Xerpi.Services;

namespace Xerpi.ViewModels
{
    public class SettingsFilterViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IMessagingCenter _messagingService;

        private bool _isInUse = false;
        public bool IsInUse
        {
            get => _isInUse;
            set
            {
                Set(ref _isInUse, value);
                OnPropertyChanged(nameof(UseText));
                UseFilterCommand.ChangeCanExecute();
            }
        }

        public string UseText => IsInUse ? "Selected" : "Use";

        public ApiFilter BackingFilter { get; set; }
        public Command UseFilterCommand { get; private set; }

        public SettingsFilterViewModel(ApiFilter backingFilter,
            ISettingsService settingsService,
            IMessagingCenter messagingService)
        {
            _settingsService = settingsService;
            _messagingService = messagingService;

            _messagingService.Subscribe<SettingsService, FilterIdChangedMessage>(this, "", FilterIdChanged);

            BackingFilter = backingFilter;
            UseFilterCommand = new Command(UseFilter, () => !IsInUse);

            IsInUse = _settingsService.FilterId == BackingFilter.Id;
        }

        private void FilterIdChanged(ISettingsService sender, FilterIdChangedMessage message)
        {
            IsInUse = message.NewId == BackingFilter.Id;
        }

        private void UseFilter()
        {
            _settingsService.FilterId = BackingFilter.Id;
        }
    }
}
