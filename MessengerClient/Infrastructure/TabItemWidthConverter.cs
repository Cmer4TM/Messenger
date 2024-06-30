using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MessengerClient.Infrastructure
{
    class TabItemWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values[0] is TabControl tabControl
            ? tabControl.ActualWidth / tabControl.Items.Count - 2
            : throw new NotImplementedException();
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
