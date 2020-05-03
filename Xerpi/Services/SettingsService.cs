using Xamarin.Essentials;
using Xamarin.Forms;
using Xerpi.Messages;

namespace Xerpi.Services
{
    public interface ISettingsService
    {
        uint FilterId { get; set; }
        AppTheme SelectedTheme { get; set; }
    }

    public class SettingsService : ISettingsService
    {
        private readonly IMessagingCenter _messagingService;

        public SettingsService(IMessagingCenter messagingService)
        {
            _messagingService = messagingService;
        }

        // Apparently 100073 is the "Default" filter. Huh.
        private const uint DefaultFilterId = 100_073;
        public uint FilterId
        {
            get => (uint)Preferences.Get(nameof(FilterId), (int)DefaultFilterId);
            set
            {
                uint oldId = FilterId;
                Preferences.Set(nameof(FilterId), (int)value);
                _messagingService.Send(this, "", new FilterIdChangedMessage { OldId = oldId, NewId = value });
            }
        }

        // Stored as int.
        public AppTheme SelectedTheme
        {
            get => (AppTheme)Preferences.Get(nameof(SelectedTheme), (int)AppTheme.Unspecified);
            set
            {
                AppTheme oldTheme = SelectedTheme;
                Preferences.Set(nameof(SelectedTheme), (int)value);
                _messagingService.Send(this, "", new AppThemeChangedMessage { OldTheme = oldTheme, NewTheme = value });
            }
        }

    }
}
