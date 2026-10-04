using Microsoft.Win32;
using SA2MsgTextEditor.Common;
using System.ComponentModel;
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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.VM;
        }

        private void WindowTextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            App.VM.SetupConfigItems();
            SetupMenusInitial();
        }

        private void WindowTextEditor_Closing(object sender, CancelEventArgs e)
        {
            if (App.VM.MessageFileLoaded)
            {
                var result = MessageBox.Show(App.GetString("Message.FileOpenOnClosing"), App.GetString("App.Title"), MessageBoxButton.OKCancel, MessageBoxImage.Information);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
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

        private void SetupViewOnFileLoading(MessageFileType type)
        {
            if (type == MessageFileType.GameplayMessages)
            {
                GroupsList.Width = new GridLength(1, GridUnitType.Star);
                Buttons.Height = GridLength.Auto;
            }
            else if (type == MessageFileType.HuntingHints)
            {
                GroupsList.Width = new GridLength(1, GridUnitType.Star);
                Buttons.Height = new GridLength(0);
            }
            else if (type == MessageFileType.SimpleTextArray)
            {
                GroupsList.Width = new GridLength(0);
                Buttons.Height = new GridLength(0);
            }
            else if (type == MessageFileType.ChaoNames)
            {
                GroupsList.Width = new GridLength(0);
                Buttons.Height = GridLength.Auto;
            }
        }

        #endregion


        #region Menu > File

        // Helpers

        private bool UserDeclinedOpeningAnotherFile()
        {
            var result = MessageBox.Show(App.GetString("Message.FileOpenOnOpeningNewFile"), App.GetString("App.Title"), MessageBoxButton.YesNo, MessageBoxImage.Information);
            return result == MessageBoxResult.No;
        }

        private bool ShowOpenFileDialog(out string fileName, params string[] filterResourceKeys)
        {
            fileName = "";

            var openFileWindow = new OpenFileDialog() { Filter = App.GetCombinedString('|', filterResourceKeys) };
            if (openFileWindow.ShowDialog() == false) return false;

            fileName = openFileWindow.FileName;
            return true;
        }

        private bool ShowSaveFileDialog(out string fileName, string defaultExtension, params string[] filterResourceKeys)
        {
            fileName = "";

            var saveFileDialog = new SaveFileDialog() { DefaultExt = defaultExtension, FileName = App.VM.MessageFile != null ? App.VM.MessageFile.Name : "", Filter = App.GetCombinedString('|', filterResourceKeys) };
            if (saveFileDialog.ShowDialog() == false) return false;

            fileName = saveFileDialog.FileName;
            return true;
        }


        // Menu commands

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (App.VM.MessageFileLoaded && UserDeclinedOpeningAnotherFile()) return;
            if (!ShowOpenFileDialog(out string fileName, "Filters.PRS.Supported", "Filters.PRS.Hunting", "Filters.PRS.Gameplay", "Filters.PRS.Other", "Filters.PRS.Chao", "Filters.PRS.All")) return;

            App.VM.LoadMessageFile(fileName);            
            var detectedEndianness = App.VM.DetectEndianness();

            if (App.Config.Settings.Endianness != Endianness.Auto)
            {
                if (detectedEndianness != App.VM.SelectedEndianness)
                {
                    App.VM.ClearMessageFile();
                    MessageBox.Show(App.GetString("Message.WrongEndianness"), App.GetString("App.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                App.VM.SelectedEndianness = detectedEndianness;
            }

            App.VM.ReadMessages();
            SetupViewOnFileLoading(App.VM.FileType);
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            App.VM.SaveMessageFile();
        }

        private void CommandSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (App.VM.FileMode == OpenFileMode.ImportJSON && App.Config.Settings.Endianness == Endianness.Auto)
            {
                MessageBox.Show(App.GetString("Message.AutoEndiannessSaveAs"), App.GetString("App.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ShowSaveFileDialog(out string fileName, "prs", "Filters.PRS.Supported", "Filters.PRS.Hunting", "Filters.PRS.Gameplay", "Filters.PRS.Other", "Filters.PRS.Chao", "Filters.PRS.All")) return;

            App.VM.FileName = fileName;
            CommandSave_Executed(sender, e);
        }

        private void MenuImportJson_Click(object sender, RoutedEventArgs e)
        {
            if (App.VM.MessageFileLoaded && UserDeclinedOpeningAnotherFile()) return;
            if (!ShowOpenFileDialog(out string fileName, "Filters.JSON")) return;

            App.VM.ImportFromJson(fileName);          

            if (App.VM.MessageFile?.MessageGroups != null)
            {
                SetupViewOnFileLoading(App.VM.FileType);
            }
            else
            {
                MessageBox.Show(App.GetString("Message.InvalidJson"), App.GetString("App.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuExportJson_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowSaveFileDialog(out string fileName, "json", "Filters.JSON")) return;

            App.VM.FileName = fileName;
            App.VM.ExportJSON();
        }

        private void CommandClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Menu > Search

        private void CommandSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!App.VM.MessageFileLoaded) return;

            var searchWindow = new SearchWindow();
            searchWindow.Show();
        }

        #endregion

        #region Menu > Settings

        // Encoding
        
        private void CheckCodepageMenuItem(MenuItem item)
        {
            Codepage1252.IsChecked = item == Codepage1252;
            Codepage1251.IsChecked = item == Codepage1251;
            CodepageSJIS.IsChecked = item == CodepageSJIS;
            CodepageCustom.IsChecked = item == CodepageCustom;
        }

        private void ReencodeEventFile(Codepage newCodepage)
        {
            App.Config.SetEncoding(newCodepage);
            var newEncoding = Encoding.GetEncoding((int)newCodepage);            
            App.VM.ReencodeMessageFile(newEncoding);            
            App.VM.SelectedEncoding = newEncoding;
            App.Config.Save();
        }

        private void Codepage1251_Click(object sender, RoutedEventArgs e)
        {
            CheckCodepageMenuItem(Codepage1251);
            ReencodeEventFile(Codepage.Windows1251);
        }

        private void Codepage1252_Click(object sender, RoutedEventArgs e)
        {
            CheckCodepageMenuItem(Codepage1252);
            ReencodeEventFile(Codepage.Windows1252);
        }

        private void CodepageSJIS_Click(object sender, RoutedEventArgs e)
        {
            CheckCodepageMenuItem(CodepageSJIS);
            ReencodeEventFile(Codepage.ShiftJIS);
        }

        private void CodepageCustom_Click(object sender, RoutedEventArgs e)
        {
            App.VM.Codepage = App.VM.SelectedEncoding.CodePage;
            var inputCustomCodepage = new CustomCodepageDialog();
            bool? result = inputCustomCodepage.ShowDialog();

            if (result == true)
            {
                int customCodepage = App.VM.Codepage;
                Encoding newEncoding;

                try
                {
                    newEncoding = Encoding.GetEncoding(customCodepage);
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show(App.GetString("Message.UnsupportedCodepage"), App.GetString("App.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    CodepageCustom.IsChecked = false;
                    return;
                }                

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

                App.VM.ReencodeMessageFile(newEncoding);
                App.VM.SelectedEncoding = newEncoding;
                App.Config.Save();
            }
            else
            {
                CodepageCustom.IsChecked = false;
            }
        }


        // Endianness

        private void AutoEndian_Click(object sender, RoutedEventArgs e)
        {
            AutoEndian.IsChecked = true;
            BigEndian.IsChecked = false;
            LittleEndian.IsChecked = false;
            App.VM.SelectedEndianness = App.Config.Settings.Endianness = Endianness.Auto;
            App.Config.Save();
        }

        private void BigEndian_Click(object sender, RoutedEventArgs e)
        {
            BigEndian.IsChecked = true;
            LittleEndian.IsChecked = false;
            AutoEndian.IsChecked = false;
            App.VM.SelectedEndianness = App.Config.Settings.Endianness = Endianness.BigEndian;
            App.Config.Save();
        }

        private void LittleEndian_Click(object sender, RoutedEventArgs e)
        {
            LittleEndian.IsChecked = true;
            BigEndian.IsChecked = false;
            AutoEndian.IsChecked = false;
            App.VM.SelectedEndianness = App.Config.Settings.Endianness = Endianness.LittleEndian;
            App.Config.Save();
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
            App.VM.UpdateLanguage();
        }

        private void MenuRussian_Click(object sender, RoutedEventArgs e)
        {
            App.SetLanguage(Common.Language.Russian);
            App.Config.Save();
            MenuEnglish.IsChecked = false;
            MenuRussian.IsChecked = true;
            MenuJapanese.IsChecked = false;
            App.VM.UpdateLanguage();
        }

        private void MenuJapanese_Click(object sender, RoutedEventArgs e)
        {
            App.SetLanguage(Common.Language.Japanese);
            App.Config.Save();
            MenuEnglish.IsChecked = false;
            MenuRussian.IsChecked = false;
            MenuJapanese.IsChecked = true;
            App.VM.UpdateLanguage();
        }

        #endregion


        #region Add/remove lines (messages list)

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            App.VM.AddMessage();
        }

        private void ButtonRemoveLast_Click(object sender, RoutedEventArgs e)
        {
            App.VM.RemoveLastMessage();
        }

        private void ButtonInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            App.VM.InsertMessage();
        }

        private void ButtonRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            App.VM.RemoveSelectedMessage();
        }

        #endregion
    }
}