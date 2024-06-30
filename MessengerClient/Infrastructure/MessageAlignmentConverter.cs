using MessengerModels;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MessengerClient.Infrastructure
{
    internal class MessageAlignmentConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Message message
            ? message.Sender.Login == CurrentUser.User?.Login
            ? HorizontalAlignment.Right
            : HorizontalAlignment.Left
            : null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
