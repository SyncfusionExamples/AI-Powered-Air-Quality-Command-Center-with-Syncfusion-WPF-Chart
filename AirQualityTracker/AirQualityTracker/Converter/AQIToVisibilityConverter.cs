using Syncfusion.UI.Xaml.Charts;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace AirQualityTracker
{
    public class AQIToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChartAdornment adornment && adornment.Item is AirQualityInfo airQualityInfo)
            {
                return airQualityInfo.PollutionIndex <= 50 ? Visibility.Visible : Visibility.Hidden;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}