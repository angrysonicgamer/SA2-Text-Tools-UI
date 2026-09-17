using SA2EventTextEditor.Extensions;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SA2EventTextEditor.Common
{
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

    public class SearchResultsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value as SearchResult;
            return $"{App.GetString("SearchResult.EventID")}: {result.EventID}, {App.GetString("SearchResult.MessageIndex")}: {result.MessageIndex}\n{result.Text}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
