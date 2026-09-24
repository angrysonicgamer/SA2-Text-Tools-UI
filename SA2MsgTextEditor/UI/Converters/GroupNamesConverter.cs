using SA2MsgTextEditor.Common;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SA2MsgTextEditor.UI.Converters
{
    [ValueConversion(sourceType: typeof(ObservableCollection<SA2Message>), targetType: typeof(string))]
    public class GroupNamesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var group = value as ObservableCollection<SA2Message>;
            return string.IsNullOrEmpty(group[0].Text) ? App.GetString("ListBox.EmptyString") : group[0].Text.Replace("\n", " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
