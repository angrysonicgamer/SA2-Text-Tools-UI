using SA2MsgTextEditor.Common;
using SA2MsgTextEditor.Extensions;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SA2MsgTextEditor.UI.Converters
{
    [ValueConversion(sourceType: typeof(TextCentering), targetType: typeof(string))]
    public class TextCenteringDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetString(((TextCentering)value).GetDescription());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
