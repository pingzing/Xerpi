using System;
using System.Globalization;
using Xamarin.Forms;
using Xerpi.Models.API;

namespace Xerpi.Converters
{
    public class TagCategoryBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TagCategory tagCat))
            {
                return null;
            }

            return tagCat.BackgroundColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
