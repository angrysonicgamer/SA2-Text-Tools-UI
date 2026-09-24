using SA2EventTextEditor.Common;
using SA2EventTextEditor.Extensions;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SA2EventTextEditor.UI.Converters
{
    [ValueConversion(sourceType: typeof(TextCentering), targetType: typeof(string))]
    public class TextCenteringNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetString(((TextCentering)value).GetDisplayName());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
