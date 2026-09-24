using SA2EventTextEditor.Common;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SA2EventTextEditor.UI.Converters
{
    [ValueConversion(sourceType: typeof(SearchResult), targetType: typeof(string))]
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
