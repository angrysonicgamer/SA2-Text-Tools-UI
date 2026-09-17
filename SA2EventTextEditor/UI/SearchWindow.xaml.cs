using SA2EventTextEditor.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2EventTextEditor.UI
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private List<SearchResult>? _searchResults;  
        
        public string? Text { get; set; }


        public SearchWindow()
        {
            InitializeComponent();            
        }

        private void WindowSearch_Loaded(object sender, RoutedEventArgs e)
        {
            SearchString.Text = Text;
            IgnoreCase.IsChecked = App.Config.Search.IgnoreCase;
        }

        private void WindowSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void SearchString_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonFind_Click(sender, e);
            }
        }


        // Buttons

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchString.Text)) return;

            _searchResults = App.SA2Event?.Search(SearchString.Text, IgnoreCase.IsChecked == true);
            ResultsCountNumber.Text = _searchResults?.Count.ToString();
            SearchResultsList.ItemsSource = _searchResults;
            App.LastSearchText = SearchString.Text;

            if (_searchResults?.Count == 0)
            {
                MessageBox.Show(App.GetString("Message.NothingFound"), App.GetString("MainWindow.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        // "Ignore case" checkbox

        private void IgnoreCase_Checked(object sender, RoutedEventArgs e)
        {
            IgnoreCase.IsChecked = App.Config.Search.IgnoreCase = true;
            App.Config.Save();
        }

        private void IgnoreCase_Unchecked(object sender, RoutedEventArgs e)
        {
            IgnoreCase.IsChecked = App.Config.Search.IgnoreCase = false;
            App.Config.Save();
        }


        // Search results

        private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsList.SelectedIndex == -1 || _searchResults == null) return;
            
            var mainWindow = Application.Current.MainWindow as MainWindow;
            int eventID = _searchResults[SearchResultsList.SelectedIndex].EventID;

            if (mainWindow != null && App.SA2Event != null)
            {
                var selectedScene = App.SA2Event.FindByEventID(eventID);

                if (selectedScene != null)
                {
                    mainWindow.Events.SelectedItem = selectedScene;
                    mainWindow.EventMessages.SelectedIndex = _searchResults[SearchResultsList.SelectedIndex].MessageIndex;
                }                
            }
        }        
    }
}
