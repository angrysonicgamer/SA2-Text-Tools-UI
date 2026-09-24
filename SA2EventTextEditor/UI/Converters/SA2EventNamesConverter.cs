using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SA2EventTextEditor.UI.Converters
{
    [ValueConversion(sourceType: typeof(int), targetType: typeof(string))]
    public class SA2EventNamesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetString($"EV{(int)value:0000}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
