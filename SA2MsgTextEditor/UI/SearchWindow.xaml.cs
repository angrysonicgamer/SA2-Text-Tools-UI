using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2MsgTextEditor.UI
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
            DataContext = App.VM;
        }

        private void WindowSearch_Loaded(object sender, RoutedEventArgs e)
        {
            IgnoreCase.IsChecked = App.Config.Search.IgnoreCase;
        }

        private void WindowSearch_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.VM.ClearSearchResults();
        }


        // Buttons

        private void Search()
        {
            if (string.IsNullOrEmpty(SearchString.Text)) return;

            App.VM.Search(SearchString.Text, IgnoreCase.IsChecked == true);

            if (App.VM.SearchResults?.Count == 0)
            {
                MessageBox.Show(App.GetString("Message.NothingFound"), App.GetString("App.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void SearchString_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        // "Ignore case" checkbox

        private void SetIgnoreCase(bool value)
        {
            IgnoreCase.IsChecked = value;

            if (IgnoreCase.IsChecked != App.Config.Search.IgnoreCase)
            {
                App.Config.Search.IgnoreCase = IgnoreCase.IsChecked.Value;
                App.Config.Save();
            }
        }

        private void IgnoreCase_Checked(object sender, RoutedEventArgs e)
        {
            SetIgnoreCase(true);
        }

        private void IgnoreCase_Unchecked(object sender, RoutedEventArgs e)
        {
            SetIgnoreCase(false);
        }


        // Search results

        private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsList.SelectedIndex == -1 || App.VM.SearchResults == null) return;

            if (App.VM.MessageFileLoaded)
            {
                App.VM.SelectedGroupIndex = App.VM.SearchResults[SearchResultsList.SelectedIndex].GroupIndex;
                App.VM.SelectedMessageIndex = App.VM.SearchResults[SearchResultsList.SelectedIndex].MessageIndex;
            }
        }        
    }
}
