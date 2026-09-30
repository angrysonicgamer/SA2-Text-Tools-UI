using System.ComponentModel;
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
        public SearchWindow()
        {
            InitializeComponent();
            DataContext = App.VM;
        }

        private void WindowSearch_Loaded(object sender, RoutedEventArgs e)
        {
            IgnoreCase.IsChecked = App.Config.Search.IgnoreCase;
        }

        private void WindowSearch_Closing(object sender, CancelEventArgs e)
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
            if (SearchResultsList.SelectedIndex == -1 || App.VM.SearchResults == null) return;            

            if (App.VM.EventFileLoaded)
            {
                int eventID = App.VM.SearchResults[SearchResultsList.SelectedIndex].EventID;
                var scene = App.VM.FindByEventID(eventID);

                if (scene != null)
                {
                    App.VM.SelectedScene = scene;
                    App.VM.SelectedMessageIndex = App.VM.SearchResults[SearchResultsList.SelectedIndex].MessageIndex;
                }                
            }
        }        
    }
}
