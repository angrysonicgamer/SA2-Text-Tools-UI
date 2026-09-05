using Microsoft.Win32;
using SA2MsgTextEditor.Common;
using SA2MsgTextEditor.Extensions;
using SA2MsgTextEditor.JSON;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2MsgTextEditor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _fileName;
        private bool _fileLoaded = false;
        private ObservableCollection<SA2Message>? _selectedGroup;
        private int _selectedGroupIndex = -1;
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
            SetupMenusInitial();
            SetDefaults();
        }

        private void WindowTextEditor_Closing(object sender, CancelEventArgs e)
        {
            if (_fileLoaded)
            {
                var result = MessageBox.Show(App.GetString("Message.FileOpenOnClosing"), App.GetString("MainWindow.Title"), MessageBoxButton.OKCancel);

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
            MessagesList.Visibility = Visibility.Hidden;
            ListGroupedMessages.Visibility = Visibility.Hidden;
            ButtonAdd.Visibility = Visibility.Hidden;
            ButtonInsertAfter.Visibility = Visibility.Hidden;
            ButtonRemoveLast.Visibility = Visibility.Hidden;
            ButtonRemoveSelected.Visibility = Visibility.Hidden;
            UpdateStatusBar();
        }

        private void SetupViewOnFileLoading(MessageFileType type)
        {
            _fileLoaded = true;
            SetWindowTitle();
            MenuSave.IsEnabled = _mode == OpenFileMode.OpenPRS;
            MenuSaveAs.IsEnabled = true;
            MenuExportJson.IsEnabled = true;
            MenuSearch.IsEnabled = true;
            AutoEndian.IsEnabled = false;
            ListGroupedMessages.ItemsSource = App.SA2Msg?.Messages;            

            if (type == MessageFileType.GameplayMessages)
            {
                GroupsList.Width = GridLength.Auto;
                Buttons.Height = GridLength.Auto;
                ListGroupedMessages.Visibility = Visibility.Visible;
                MessagesList.Visibility = Visibility.Hidden;
                ColumnVoice.Visibility = Visibility.Visible;
                ColumnFrameCount.Visibility = Visibility.Visible;
                ColumnIs2P.Visibility = Visibility.Hidden;
                ColumnTextCentering.Visibility = Visibility.Visible;
                ColumnText.Visibility = Visibility.Visible;
                ColumnChaoNames.Visibility = Visibility.Hidden;
                ButtonAdd.Visibility = Visibility.Hidden;
                ButtonInsertAfter.Visibility = Visibility.Hidden;
                ButtonRemoveLast.Visibility = Visibility.Hidden;
                ButtonRemoveSelected.Visibility = Visibility.Hidden;
            }
            else if (type == MessageFileType.HuntingHints)
            {
                GroupsList.Width = GridLength.Auto;
                Buttons.Height = new GridLength(0);
                ListGroupedMessages.Visibility = Visibility.Visible;
                MessagesList.Visibility = Visibility.Hidden;             
                ColumnVoice.Visibility = Visibility.Hidden;
                ColumnFrameCount.Visibility = Visibility.Hidden;
                ColumnIs2P.Visibility = Visibility.Visible;
                ColumnTextCentering.Visibility = Visibility.Visible;
                ColumnText.Visibility = Visibility.Visible;
                ColumnChaoNames.Visibility = Visibility.Hidden;
            }
            else if (type == MessageFileType.SimpleTextArray)
            {
                GroupsList.Width = new GridLength(0);
                Buttons.Height = new GridLength(0);
                ListGroupedMessages.Visibility = Visibility.Hidden;
                ListGroupedMessages.SelectedIndex = 0;
                MessagesList.Visibility = Visibility.Visible;
                ColumnVoice.Visibility = Visibility.Hidden;
                ColumnFrameCount.Visibility = Visibility.Hidden;
                ColumnIs2P.Visibility = Visibility.Hidden;
                ColumnTextCentering.Visibility = Visibility.Visible;
                ColumnText.Visibility = Visibility.Visible;
                ColumnChaoNames.Visibility = Visibility.Hidden;
            }
            else // Chao names
            {
                GroupsList.Width = new GridLength(0);
                Buttons.Height = GridLength.Auto;
                ListGroupedMessages.Visibility = Visibility.Hidden;
                ListGroupedMessages.SelectedIndex = 0;
                MessagesList.Visibility = Visibility.Visible;
                ColumnVoice.Visibility = Visibility.Hidden;
                ColumnFrameCount.Visibility = Visibility.Hidden;
                ColumnIs2P.Visibility = Visibility.Hidden;
                ColumnTextCentering.Visibility = Visibility.Hidden;
                ColumnText.Visibility = Visibility.Hidden;
                ColumnChaoNames.Visibility = Visibility.Visible;
                ButtonAdd.Visibility = Visibility.Visible;
                ButtonInsertAfter.Visibility = Visibility.Visible;
                ButtonRemoveLast.Visibility = Visibility.Visible;
                ButtonRemoveSelected.Visibility = Visibility.Visible;
            }

            UpdateStatusBar();
        }

        private void SetWindowTitle()
        {
            Title = _fileLoaded ? $"{App.GetString("MainWindow.Title")} — {Path.GetFileName(_fileName)}" : App.GetString("MainWindow.Title");
        }

        #endregion


        #region Menu > File

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_fileLoaded)
            {
                var result = MessageBox.Show(App.GetString("Message.FileOpenOnOpeningNewFile"), App.GetString("MainWindow.Title"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }

            var openFileWindow = new OpenFileDialog() { Filter = App.GetString("Filters.PRS") };
            if (openFileWindow.ShowDialog() == false) return;

            _fileName = openFileWindow.FileName;
            App.SA2Msg = new SA2MessageFile(_fileName);
            var detectedEndianness = App.SA2Msg.DetectEndianness();

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
            }

            _mode = OpenFileMode.OpenPRS;
            App.SA2Msg.ReadMessages(_selectedEncoding, _selectedEndianness);            
            SetupViewOnFileLoading(App.SA2Msg.Type);
            UpdateStatusBar();
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.SA2Msg?.Save(_fileName, _selectedEncoding, _selectedEndianness);
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
                var result = MessageBox.Show(App.GetString("Message.FileOpenOnOpeningNewFile"), App.GetString("MainWindow.Title"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }

            var openFileWindow = new OpenFileDialog() { Filter = App.GetString("Filters.JSON") };
            if (openFileWindow.ShowDialog() == false) return;

            _fileName = openFileWindow.FileName;
            App.SA2Msg = Json.Import<SA2MessageFile>(_fileName);            

            if (App.SA2Msg?.Messages != null)
            {
                _mode = OpenFileMode.ImportJSON;
                SetupViewOnFileLoading(App.SA2Msg.Type);
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
            Json.Export(App.SA2Msg, _fileName);
        }

        private void CommandClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Menu > Search

        private void CommandSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (App.SA2Msg == null) return;

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

        private void UpdateViewOnCodepageChange()
        {
            if (App.SA2Msg?.Type == MessageFileType.GameplayMessages || App.SA2Msg?.Type == MessageFileType.HuntingHints)
            {
                ListGroupedMessages.ItemsSource = null;
                ListGroupedMessages.ItemsSource = App.SA2Msg?.Messages;
            }
        }

        private void Codepage1251_Click(object sender, RoutedEventArgs e)
        {
            var newEncoding = Encoding.GetEncoding((int)Codepage.Windows1251);
            App.SA2Msg?.Reencode(_selectedEncoding, newEncoding);
            _selectedEncoding = newEncoding;            
            CheckCodepageMenuItem(Codepage1251);
            UpdateViewOnCodepageChange();
            App.Config.SetEncoding(Codepage.Windows1251);
            App.Config.Save();
            UpdateStatusBar();
        }

        private void Codepage1252_Click(object sender, RoutedEventArgs e)
        {
            var newEncoding = Encoding.GetEncoding((int)Codepage.Windows1252);
            App.SA2Msg?.Reencode(_selectedEncoding, newEncoding);
            _selectedEncoding = newEncoding;
            CheckCodepageMenuItem(Codepage1252);
            UpdateViewOnCodepageChange();
            App.Config.SetEncoding(Codepage.Windows1252);
            App.Config.Save();
            UpdateStatusBar();
        }

        private void CodepageSJIS_Click(object sender, RoutedEventArgs e)
        {
            var newEncoding = Encoding.GetEncoding((int)Codepage.ShiftJIS);
            App.SA2Msg?.Reencode(_selectedEncoding, newEncoding);
            _selectedEncoding = newEncoding;
            CheckCodepageMenuItem(CodepageSJIS);
            UpdateViewOnCodepageChange();
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

                App.SA2Msg?.Reencode(_selectedEncoding, newEncoding);
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
                UpdateViewOnCodepageChange();
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
            App.Config.Save();
            UpdateStatusBar();
        }

        private void LittleEndian_Click(object sender, RoutedEventArgs e)
        {
            LittleEndian.IsChecked = true;
            BigEndian.IsChecked = false;
            AutoEndian.IsChecked = false;
            _selectedEndianness = App.Config.Settings.Endianness = Endianness.LittleEndian;
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
            ListGroupedMessages.ItemsSource = null;
            ListGroupedMessages.ItemsSource = App.SA2Msg?.Messages;
            MessagesList.ItemsSource = null;
            MessagesList.ItemsSource = _selectedGroup;
        }

        #endregion


        #region List box (groups)

        private void ListGroupedMessages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListGroupedMessages.SelectedItem is ObservableCollection<SA2Message> msgList)
            {
                MessagesList.Visibility = Visibility.Visible;
                ButtonAdd.Visibility = Visibility.Visible;
                ButtonRemoveLast.Visibility = Visibility.Visible;
                ButtonInsertAfter.Visibility = Visibility.Visible;
                ButtonRemoveSelected.Visibility = Visibility.Visible;
                MessagesList.ItemsSource = msgList;
                _selectedGroup = msgList;
                _selectedGroupIndex = ListGroupedMessages.SelectedIndex;
            }

            UpdateStatusBar();
        }

        #endregion


        #region Data grid (messages list)

        private void MessagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonInsertAfter.IsEnabled = MessagesList.SelectedIndex != -1;
            ButtonRemoveSelected.IsEnabled = MessagesList.SelectedIndex != -1;

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


        #region Add/remove lines (messages list)

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup?.Add(new SA2Message());
            UpdateStatusBar();
        }

        private void ButtonRemoveLast_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup?.RemoveAt(_selectedGroup.Count - 1);
            UpdateStatusBar();
        }

        private void ButtonInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            if (MessagesList.SelectedIndex + 1 < _selectedGroup?.Count)
            {
                _selectedGroup.Insert(MessagesList.SelectedIndex + 1, new SA2Message());
            }
            else
            {
                _selectedGroup?.Add(new SA2Message());
            }

            UpdateStatusBar();
        }

        private void ButtonRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            _selectedGroup?.RemoveAt(MessagesList.SelectedIndex);
            UpdateStatusBar();
        }

        #endregion


        #region Status bar

        private void SetDetailsVisibility(Visibility visibility)
        {
            StatusSelectedGroup.Visibility = StatusSelectedItem.Visibility = StatusTotalItems.Visibility = visibility;
            StatusSeparator1.Visibility = StatusSeparator2.Visibility = StatusSeparator3.Visibility = visibility;
        }

        private void UpdateStatusBar()
        {
            string encoding = App.GetString(App.Config.Settings.Encoding.GetDisplayName());
            StatusFileType.Text = _fileLoaded && App.SA2Msg != null ? App.GetString(App.SA2Msg.Type.GetDisplayName()) : "";
            StatusEncoding.Text = App.Config.Settings.CustomCodepage.HasValue ? $"{encoding}: {App.Config.Settings.CustomCodepage.Value}" : encoding;
            StatusEndianness.Text = App.GetString(_selectedEndianness.GetDisplayName());
            
            if (_selectedGroupIndex != -1 && _fileLoaded)
            {
                SetDetailsVisibility(Visibility.Visible);
                StatusSelectedGroup.Text = $"{App.GetString("Status.SelectedGroup")}: {_selectedGroupIndex + 1}";
                StatusSelectedItem.Text = MessagesList.SelectedIndex != -1 ? $"{App.GetString("Status.SelectedItem")}: {MessagesList.SelectedIndex + 1}" : App.GetString("Status.SelectedItem.None");
                StatusTotalItems.Text = $"{App.GetString("Status.TotalItems")}: {_selectedGroup?.Count}";
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