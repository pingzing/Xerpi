using System;
using System.Globalization;
using Xamarin.Forms;

namespace Xerpi.Converters
{
    public class DateTimeTimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTimeOffset dateTime))
            {
                throw new ArgumentException("Tried to pass something that isn't a DateTimeOffset to DateTimeTimeAgoConverter.");
            }

            var utcNow = DateTimeOffset.Now;
            TimeSpan timeAgo = utcNow - dateTime;
            if (timeAgo.TotalDays <= 1)
            {
                return GetTimeAgoString(timeAgo);
            }
            else
            {
                return $"{dateTime:d} {dateTime:t}"; // Short date, space, short time
            }
        }

        private string GetTimeAgoString(TimeSpan timeAgo)
        {
            if (timeAgo.TotalMinutes == 0) //0-59 seconds
            {
                return $"{timeAgo.TotalSeconds:#.} seconds ago";
            }
            else if (timeAgo.TotalMinutes > 0 && timeAgo.TotalHours < 1) // 60 seconds - 59 minutes
            {
                string secondsComponent = timeAgo.Seconds > 0 ? $", {timeAgo.Seconds} seconds" : string.Empty;
                return $"{timeAgo.TotalMinutes:#.} minutes{secondsComponent} ago";
            }
            else // if(timeAgo.TotalHours >= 1)
            {
                string minutesComponent = timeAgo.Minutes > 0 ? $", {timeAgo.Minutes} minutes" : string.Empty;
                return $"{timeAgo.TotalHours:#.} hours{minutesComponent} ago";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
