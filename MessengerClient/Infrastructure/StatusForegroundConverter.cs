using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MessengerClient.Infrastructure
{
    class StatusForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value.ToString() == "Online"
            ? Brushes.Green
            : Brushes.Red;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
