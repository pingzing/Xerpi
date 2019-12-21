using System;
using System.Globalization;
using Xamarin.Forms;

namespace Xerpi.Converters
{
    public class DerpiCdnConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string url))
            {
                return null;
            }

            if (!url.StartsWith("//derpicdn.net"))
            {
                return null;
            }

            return url.Replace("//derpicdn.net", "https://derpicdn.net");
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string url))
            {
                return null;
            }

            if (!url.StartsWith("https://derpicdn.net"))
            {
                return null;
            }

            return url.Replace("https://derpicdn.net", "//derpicdn.net");
        }
    }
}
