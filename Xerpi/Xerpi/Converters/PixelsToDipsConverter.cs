using System;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Xerpi.Converters
{
    public class PixelsToDipsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double pixelValue = System.Convert.ToDouble(value);
            double screenDensity = 1;
            if (Device.RuntimePlatform == Device.iOS)
            {
                MainThread.BeginInvokeOnMainThread(() => screenDensity = DeviceDisplay.MainDisplayInfo.Density);
            }
            else
            {
                screenDensity = DeviceDisplay.MainDisplayInfo.Density;
            }

            return pixelValue / screenDensity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
