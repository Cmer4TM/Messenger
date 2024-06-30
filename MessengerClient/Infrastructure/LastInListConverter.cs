using MessengerModels;
using System.Globalization;
using System.Windows.Data;

namespace MessengerClient.Infrastructure
{
    internal class LastInListConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Chat chat
            && chat.Messages.Count > 0
            ? chat.Messages[^1].Content
            : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
