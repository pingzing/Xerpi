using System;
using Xamarin.Essentials;

namespace Xerpi.Services
{
    public interface ISettingsService
    {
        event EventHandler<uint> FilterIdChanged;
        uint FilterId { get; set; }
    }

    public class SettingsService : ISettingsService
    {
        // Apparently 100073 is the "Default" filter. Huh.
        private const uint DefaultFilterId = 100_073;
        public event EventHandler<uint>? FilterIdChanged;
        public uint FilterId
        {
            get => (uint)Preferences.Get(nameof(FilterId), (int)DefaultFilterId);
            set
            {
                Preferences.Set(nameof(FilterId), (int)value);
                FilterIdChanged?.Invoke(this, value);
            }
        }

    }
}
