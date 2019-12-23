using System;
using Xamarin.Forms;
using Xerpi.Models.API;
using Xerpi.Services;

namespace Xerpi.ViewModels
{
    public class SettingsFilterViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;

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

        public SettingsFilterViewModel(ApiFilter backingFilter, ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _settingsService.FilterIdChanged += _settingsService_FilterIdChanged;

            BackingFilter = backingFilter;
            UseFilterCommand = new Command(UseFilter, () => !IsInUse);

            IsInUse = _settingsService.FilterId == BackingFilter.Id;
        }

        private void _settingsService_FilterIdChanged(object sender, uint newFilter)
        {
            IsInUse = newFilter == BackingFilter.Id;
        }

        private void UseFilter()
        {
            _settingsService.FilterId = BackingFilter.Id;
        }
    }
}
