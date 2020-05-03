using Xamarin.Essentials;

namespace Xerpi.Messages
{

    public class FilterIdChangedMessage
    {
        public uint OldId { get; set; }
        public uint NewId { get; set; }
    }

    public class AppThemeChangedMessage
    {
        public AppTheme OldTheme { get; set; }
        public AppTheme NewTheme { get; set; }
    }
}
