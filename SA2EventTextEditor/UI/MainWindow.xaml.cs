using Microsoft.Win32;
using SA2EventTextEditor.Common;
using SA2EventTextEditor.Extensions;
using SA2EventTextEditor.JSON;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2EventTextEditor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _fileName;
        private bool _fileLoaded = false;
        private SA2Scene? _selectedScene;
        private int _selectedSceneIndex = -1;
        private Encoding _selectedEncoding;
        private Endianness _selectedEndianness;
        private OpenFileMode _mode;        


        public MainWindow()
        {
            InitializeComponent();            
            _selectedEncoding = App.Config.Settings.CustomCodepage.HasValue ? Encoding.GetEncoding(App.Config.Settings.CustomCodepage.Value) : Encoding.GetEncoding((int)App.Config.Settings.Encoding);
            _selectedEndianness = App.Config.Settings.Endianness;
        }

        private void WindowTextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Pointer.SetBaseAddress(_selectedEndianness);
            SetupMenusInitial();
            SetDefaults();
        }

        private void WindowTextEditor_Closing(object sender, CancelEventArgs e)
        {
            if (_fileLoaded)
            {
                var result = MessageBox.Show(App.GetString("Message.FileOpenOnClosing"), App.GetString("MainWindow.Title"), MessageBoxButton.OKCancel, MessageBoxImage.Information);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }


        #region Setting up the view

        private void SetupMenusInitial()
        {
            switch (App.Config.Settings.Encoding)
            {
                case Codepage.Windows1252:
                    Codepage1252.IsChecked = true;
                    break;
                case Codepage.Windows1251:
                    Codepage1251.IsChecked = true;
                    break;
                case Codepage.ShiftJIS:
                    CodepageSJIS.IsChecked = true;
                    break;
                case Codepage.Custom:
                    CodepageCustom.IsChecked = true;
                    break;
            }

            switch (App.Config.Settings.Endianness)
            {
                case Endianness.Auto:
                    AutoEndian.IsChecked = true;
                    break;
                case Endianness.BigEndian:
                    BigEndian.IsChecked = true;
                    break;
                case Endianness.LittleEndian:
                    LittleEndian.IsChecked = true;
                    break;
            }

            switch (App.Config.Settings.Language)
            {
                case Common.Language.English:
                    MenuEnglish.IsChecked = true;
                    break;
                case Common.Language.Russian:
                    MenuRussian.IsChecked = true;
                    break;
                case Common.Language.Japanese:
                    MenuJapanese.IsChecked = true;
                    break;
            }
        }
        
        private void SetDefaults()
        {
            _fileName = "";
            _fileLoaded = false;
            SetWindowTitle();
            _mode = OpenFileMode.OpenPRS;
            MenuSave.IsEnabled = false;
            MenuSaveAs.IsEnabled = false;
            MenuExportJson.IsEnabled = false;
            MenuSearch.IsEnabled = false;
            AutoEndian.IsEnabled = true;
            Events.Visibility = Visibility.Hidden;
            GridMessagesList.Visibility = Visibility.Hidden;
            UpdateStatusBar();
        }

        private void SetupViewOnFileLoading()
        {
            _fileLoaded = true;
            SetWindowTitle();
            MenuSave.IsEnabled = _mode == OpenFileMode.OpenPRS;
            MenuSaveAs.IsEnabled = true;
            MenuExportJson.IsEnabled = true;
            MenuSearch.IsEnabled = true;
            AutoEndian.IsEnabled = false;
            Events.ItemsSource = App.SA2Event?.Events;
            Events.Visibility = Visibility.Visible;
            Events.Items.SortDescriptions.Add(new SortDescription("EventID", ListSortDirection.Ascending));
            GridMessagesList.Visibility = Visibility.Hidden;
            UpdateStatusBar();
        }

        private void SetWindowTitle()
        {
            Title = _fileLoaded ? $"{App.GetString("MainWindow.Title")} — {Path.GetFileName(_fileName)}" : App.GetString("MainWindow.Title");
        }

        #endregion


        #region Menu

        #region Menu > File

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_fileLoaded)
            {
                var result = MessageBox.Show(App.GetString("Message.FileOpenOnOpeningNewFile"), App.GetString("MainWindow.Title"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No) return;
            }

            var openFileWindow = new OpenFileDialog() { Filter = App.GetString("Filters.PRS") };
            if (openFileWindow.ShowDialog() == false) return;

            _fileName = openFileWindow.FileName;
            App.SA2Event = new SA2EventFile(_fileName);
            var detectedEndianness = App.SA2Event.DetectEndianness();

            if (App.Config.Settings.Endianness != Endianness.Auto)
            {
                if (detectedEndianness != _selectedEndianness)
                {
                    MessageBox.Show(App.GetString("Message.WrongEndianness"), App.GetString("MainWindow.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    SetDefaults();
                    ResetStatusBar();
                    return;
                }                
            }
            else
            {
                _selectedEndianness = detectedEndianness;
                Pointer.SetBaseAddress(detectedEndianness);
            }

            _mode = OpenFileMode.OpenPRS;
            App.SA2Event.ReadEventData(_selectedEncoding, _selectedEndianness);
            SetupViewOnFileLoading();
            UpdateStatusBar();
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.SA2Event?.Save(_fileName, _selectedEncoding, _selectedEndianness);
        }

        private void CommandSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_mode == OpenFileMode.ImportJSON && App.Config.Settings.Endianness == Endianness.Auto)
            {
                MessageBox.Show(App.GetString("Message.AutoEndiannessSaveAs"), App.GetString("MainWindow.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var saveFileDialog = new SaveFileDialog() { DefaultExt = "prs", FileName = Path.GetFileNameWithoutExtension(_fileName), Filter = App.GetString("Filters.PRS") };
            if (saveFileDialog.ShowDialog() == false) return;

            _fileName = saveFileDialog.FileName;
            CommandSave_Executed(sender, e);
        }

        private void MenuImportJson_Click(object sender, RoutedEventArgs e)
        {
            if (_fileLoaded)
            {
                var result = MessageBox.Show(App.GetString("Message.FileOpenOnOpeningNewFile"), App.GetString("MainWindow.Title"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No) return;
            }

            var openFileWindow = new OpenFileDialog() { Filter = App.GetString("Filters.JSON") };
            if (openFileWindow.ShowDialog() == false) return;

            _fileName = openFileWindow.FileName;
            App.SA2Event = Json.Import<SA2EventFile>(_fileName);

            if (App.SA2Event?.Events != null)
            {
                _mode = OpenFileMode.ImportJSON;
                SetupViewOnFileLoading();
            }
            else
            {
                MessageBox.Show(App.GetString("Message.InvalidJson"), App.GetString("MainWindow.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                SetDefaults();
            }
        }

        private void MenuExportJson_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog() { DefaultExt = "json", FileName = Path.GetFileNameWithoutExtension(_fileName), Filter = App.GetString("Filters.JSON") };
            if (saveFileDialog.ShowDialog() == false) return;

            _fileName = saveFileDialog.FileName;
            Json.Export(App.SA2Event, _fileName);
        }

        private void CommandClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion


        #region Menu > Search

        private void CommandSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (App.SA2Event == null) return;

            var searchWindow = new SearchWindow() { Text = App.LastSearchText };
            searchWindow.Show();
        }

        #endregion


        #region Menu > Settings

        private void CheckCodepageMenuItem(MenuItem item)
        {
            Codepage1252.IsChecked = item == Codepage1252;
            Codepage1251.IsChecked = item == Codepage1251;
            CodepageSJIS.IsChecked = item == CodepageSJIS;
            CodepageCustom.IsChecked = item == CodepageCustom;
        }
        
        private void Codepage1251_Click(object sender, RoutedEventArgs e)
        {
            var newEncoding = Encoding.GetEncoding((int)Codepage.Windows1251);
            App.SA2Event?.Reencode(_selectedEncoding, newEncoding);
            _selectedEncoding = newEncoding;
            CheckCodepageMenuItem(Codepage1251);
            App.Config.SetEncoding(Codepage.Windows1251);
            App.Config.Save();
            UpdateStatusBar();
        }

        private void Codepage1252_Click(object sender, RoutedEventArgs e)
        {
            var newEncoding = Encoding.GetEncoding((int)Codepage.Windows1252);
            App.SA2Event?.Reencode(_selectedEncoding, newEncoding);
            _selectedEncoding = newEncoding;
            CheckCodepageMenuItem(Codepage1252);
            App.Config.SetEncoding(Codepage.Windows1252);
            App.Config.Save();
            UpdateStatusBar();
        }

        private void CodepageSJIS_Click(object sender, RoutedEventArgs e)
        {
            var newEncoding = Encoding.GetEncoding((int)Codepage.ShiftJIS);
            App.SA2Event?.Reencode(_selectedEncoding, newEncoding);
            _selectedEncoding = newEncoding;
            CheckCodepageMenuItem(CodepageSJIS);
            App.Config.SetEncoding(Codepage.ShiftJIS);
            App.Config.Save();
            UpdateStatusBar();
        }

        private void CodepageCustom_Click(object sender, RoutedEventArgs e)
        {
            var inputCustomCodepage = new InputCustomCodepage { Codepage = _selectedEncoding.CodePage };
            bool? result = inputCustomCodepage.ShowDialog();

            if (result == true)
            {
                int customCodepage = inputCustomCodepage.Codepage.Value;
                Encoding newEncoding;

                try
                {
                    newEncoding = Encoding.GetEncoding(customCodepage);
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show(App.GetString("Message.UnsupportedCodepage"), App.GetString("MainWindow.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    CodepageCustom.IsChecked = false;
                    return;
                }

                App.SA2Event?.Reencode(_selectedEncoding, newEncoding);
                _selectedEncoding = newEncoding;

                if (customCodepage == (int)Codepage.Windows1252)
                {
                    CheckCodepageMenuItem(Codepage1252);
                    App.Config.SetEncoding(Codepage.Windows1252);
                }
                else if (customCodepage == (int)Codepage.Windows1251)
                {
                    CheckCodepageMenuItem(Codepage1251);
                    App.Config.SetEncoding(Codepage.Windows1251);
                }
                else if (customCodepage == (int)Codepage.ShiftJIS)
                {
                    CheckCodepageMenuItem(CodepageSJIS);
                    App.Config.SetEncoding(Codepage.ShiftJIS);
                }
                else
                {
                    CheckCodepageMenuItem(CodepageCustom);
                    App.Config.SetEncoding(customCodepage);
                }

                App.Config.Save();
                UpdateStatusBar();
            }
            else
            {
                CodepageCustom.IsChecked = false;
            }
        }

        private void AutoEndian_Click(object sender, RoutedEventArgs e)
        {
            AutoEndian.IsChecked = true;
            BigEndian.IsChecked = false;
            LittleEndian.IsChecked = false;
            App.Config.Settings.Endianness = Endianness.Auto;
            App.Config.Save();
            UpdateStatusBar();
        }

        private void BigEndian_Click(object sender, RoutedEventArgs e)
        {
            BigEndian.IsChecked = true;
            LittleEndian.IsChecked = false;
            AutoEndian.IsChecked = false;
            _selectedEndianness = App.Config.Settings.Endianness = Endianness.BigEndian;
            Pointer.SetBaseAddress(_selectedEndianness);            
            App.Config.Save();
            UpdateStatusBar();
        }

        private void LittleEndian_Click(object sender, RoutedEventArgs e)
        {
            LittleEndian.IsChecked = true;
            BigEndian.IsChecked = false;
            AutoEndian.IsChecked = false;
            _selectedEndianness = App.Config.Settings.Endianness = Endianness.LittleEndian;
            Pointer.SetBaseAddress(_selectedEndianness);            
            App.Config.Save();
            UpdateStatusBar();
        }

        #endregion


        #region Menu > Language

        private void MenuEnglish_Click(object sender, RoutedEventArgs e)
        {
            App.SetLanguage(Common.Language.English);
            App.Config.Save();
            MenuEnglish.IsChecked = true;
            MenuRussian.IsChecked = false;
            MenuJapanese.IsChecked = false;
            SetWindowTitle();
            UpdateViewOnLanguageChange();
            UpdateStatusBar();
        }

        private void MenuRussian_Click(object sender, RoutedEventArgs e)
        {
            App.SetLanguage(Common.Language.Russian);
            App.Config.Save();
            MenuEnglish.IsChecked = false;
            MenuRussian.IsChecked = true;
            MenuJapanese.IsChecked = false;
            SetWindowTitle();
            UpdateViewOnLanguageChange();
            UpdateStatusBar();
        }

        private void MenuJapanese_Click(object sender, RoutedEventArgs e)
        {
            App.SetLanguage(Common.Language.Japanese);
            App.Config.Save();
            MenuEnglish.IsChecked = false;
            MenuRussian.IsChecked = false;
            MenuJapanese.IsChecked = true;
            SetWindowTitle();
            UpdateViewOnLanguageChange();
            UpdateStatusBar();
        }

        private void UpdateViewOnLanguageChange()
        {
            Events.ItemsSource = null;
            Events.ItemsSource = App.SA2Event?.Events;
            EventMessages.ItemsSource = null;
            EventMessages.ItemsSource = _selectedScene?.Messages;
        }

        #endregion        

        #endregion


        #region List box (Event IDs)

        private void Events_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Events.SelectedIndex != -1)
            {
                GridMessagesList.Visibility = Visibility.Visible;
                _selectedScene = Events.SelectedItem as SA2Scene;
                EventMessages.ItemsSource = _selectedScene?.Messages;
                _selectedSceneIndex = Events.SelectedIndex;
            }

            UpdateStatusBar();
        }

        #endregion


        #region Data grid (Messages list for selected event)

        private void EventMessages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonInsertAfter.IsEnabled = EventMessages.SelectedIndex != -1;
            ButtonRemoveSelected.IsEnabled = EventMessages.SelectedIndex != -1;

            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dataGrid.UpdateLayout();
                        dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                    }));
            }

            UpdateStatusBar();
        }

        #endregion


        #region Add/remove lines

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            _selectedScene?.Messages.Add(new SA2EventMessage());
            UpdateStatusBar();
        }

        private void ButtonRemoveLast_Click(object sender, RoutedEventArgs e)
        {
            _selectedScene?.Messages.RemoveAt(_selectedScene.Messages.Count - 1);
            UpdateStatusBar();
        }

        private void ButtonInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            if (EventMessages.SelectedIndex + 1 < _selectedScene?.Messages.Count)
            {
                _selectedScene?.Messages.Insert(EventMessages.SelectedIndex + 1, new SA2EventMessage());
            }
            else
            {
                _selectedScene?.Messages.Add(new SA2EventMessage());
            }
                        
            UpdateStatusBar();
        }

        private void ButtonRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            _selectedScene?.Messages.RemoveAt(EventMessages.SelectedIndex);
            UpdateStatusBar();
        }

        #endregion


        #region Status bar

        private void SetDetailsVisibility(Visibility visibility)
        {
            StatusEventID.Visibility = StatusSelectedItem.Visibility = StatusTotalItems.Visibility = visibility;
            StatusSeparator1.Visibility = StatusSeparator2.Visibility = StatusSeparator3.Visibility = visibility;
        }

        private void UpdateStatusBar()
        {
            string encoding = App.GetString(App.Config.Settings.Encoding.GetDisplayName());            
            StatusMode.Text = _fileLoaded ? App.GetString(_mode.GetDisplayName()) : "";
            StatusEncoding.Text = App.Config.Settings.CustomCodepage.HasValue ? $"{encoding}: {App.Config.Settings.CustomCodepage.Value}" : encoding;
            StatusEndianness.Text = App.GetString(_selectedEndianness.GetDisplayName());

            if (_selectedSceneIndex != -1 && _fileLoaded)
            {
                SetDetailsVisibility(Visibility.Visible);
                StatusEventID.Text = $"{App.GetString("Status.EventID")}: {_selectedScene?.EventID}";
                StatusSelectedItem.Text = EventMessages.SelectedIndex != -1 ? $"{App.GetString("Status.SelectedItem")}: {EventMessages.SelectedIndex}" : App.GetString("Status.SelectedItem.None");
                StatusTotalItems.Text = $"{App.GetString("Status.TotalItems")}: {_selectedScene?.Messages.Count}";
            }
            else
            {
                ResetStatusBar();
            }
        }

        private void ResetStatusBar()
        {
            SetDetailsVisibility(Visibility.Hidden);
        }

        #endregion
    }
}