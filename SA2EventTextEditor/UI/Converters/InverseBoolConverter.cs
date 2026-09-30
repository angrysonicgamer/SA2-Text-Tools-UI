using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SA2EventTextEditor.UI.Converters
{
    [ValueConversion(sourceType: typeof(bool), targetType: typeof(bool))]
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
