using SA2EventTextEditor.Common;
using SA2EventTextEditor.Extensions;
using SA2EventTextEditor.JSON;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SA2EventTextEditor.VM
{
    public class AppViewModel : PropertyChangedNotifier
    {
        #region Properties raising PropertyChanged

        private Encoding _selectedEncoding;
        private Endianness _selectedEndianness;
        private string? _fileName;
        private OpenFileMode _mode;
        private SA2EventFile? _eventFile;
        private SA2Scene? _selectedScene;
        private int _selectedMessageIndex;
        private ObservableCollection<SearchResult>? _searchResults;


        public Encoding SelectedEncoding
        {
            get => _selectedEncoding;
            set { _selectedEncoding = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(EncodingAsString)); }
        }
        public Endianness SelectedEndianness
        {
            get => _selectedEndianness;
            set { _selectedEndianness = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(EndiannessAsString)); }
        }
        public string? FileName
        {
            get => _fileName;
            set { _fileName = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(Title)); }
        }
        public OpenFileMode FileMode
        {
            get => _mode;
            set { _mode = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(FileModeAsString)); NotifyPropertyChanged(nameof(IsPRS)); }
        }
        public SA2EventFile? EventFile
        {
            get => _eventFile;
            set { _eventFile = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(EventFileLoaded)); }
        }
        public SA2Scene? SelectedScene
        {
            get => _selectedScene;
            set { _selectedScene = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(AnySceneIsSelected)); }
        }
        public int SelectedMessageIndex
        {
            get => _selectedMessageIndex;
            set { _selectedMessageIndex = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(AnyMessageIsSelected)); NotifyPropertyChanged(nameof(SelectedMessageIndexAsString)); }
        }        
        public ObservableCollection<SearchResult>? SearchResults
        {
            get => _searchResults;
            set { _searchResults = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region Other properties

        public string Title => EventFileLoaded ? $"{App.GetString("App.Title")} — {Path.GetFileName(FileName)}" : App.GetString("App.Title");
        public string EncodingAsString
        {
            get
            {
                string encoding = App.GetString(App.Config.Settings.Encoding.GetDisplayName());
                return App.Config.Settings.CustomCodepage.HasValue ? $"{encoding}: {App.Config.Settings.CustomCodepage.Value}" : encoding;
            }            
        }
        public string EndiannessAsString => App.GetString(SelectedEndianness.GetDisplayName());
        public bool EventFileLoaded => EventFile != null;
        public string FileModeAsString => FileMode != OpenFileMode.NoFile ? App.GetString(FileMode.GetDisplayName()) : "";
        public bool IsPRS => FileMode == OpenFileMode.OpenPRS;
        public bool AnySceneIsSelected => SelectedScene != null;
        public bool AnyMessageIsSelected => SelectedMessageIndex != -1;
        public string SelectedMessageIndexAsString => AnyMessageIsSelected ? SelectedMessageIndex.ToString() : App.GetString("Status.SelectedItem.None");
        public int Codepage { get; set; }
        public string? LastSearchString { get; set; }

        #endregion


        #region Methods

        #region Setup/update

        public void SetupConfigItems()
        {
            SelectedEncoding = App.Config.Settings.CustomCodepage.HasValue ? Encoding.GetEncoding(App.Config.Settings.CustomCodepage.Value) : Encoding.GetEncoding((int)App.Config.Settings.Encoding);
            SelectedEndianness = App.Config.Settings.Endianness;
        }

        public void UpdateLanguage()
        {
            NotifyPropertyChanged(nameof(Title));
            NotifyPropertyChanged(nameof(FileModeAsString));
            NotifyPropertyChanged(nameof(EncodingAsString));
            NotifyPropertyChanged(nameof(EndiannessAsString));
            NotifyPropertyChanged(nameof(SelectedMessageIndexAsString));

            if (EventFile == null) return;
            
            foreach (var scene in EventFile.Events)
            {
                scene.NotifyPropertyChanged(nameof(scene.EventID));

                foreach (var msg in scene.Messages)
                {
                    msg.NotifyPropertyChanged(nameof(msg.TextCentering));
                }
            }
        }

        #endregion

        #region File

        public void LoadEventFile(string fileName)
        {
            EventFile = new SA2EventFile(fileName);
            FileName = fileName;
            FileMode = OpenFileMode.OpenPRS;
        }

        public void ClearEventFile()
        {
            EventFile = null;
            FileName = null;
            FileMode = OpenFileMode.NoFile;
        }

        public Endianness DetectEndianness()
        {
            return EventFile.DetectEndianness();
        }

        public void ReadEventFile()
        {
            EventFile?.ReadEventData(SelectedEncoding, SelectedEndianness);
        }

        public void SaveEventFile()
        {
            if (FileName == null || FileMode != OpenFileMode.OpenPRS) return;

            EventFile?.Save(FileName, SelectedEncoding, SelectedEndianness);
        }

        public void ImportFromJson(string fileName)
        {
            EventFile = Json.Import<SA2EventFile>(fileName);
            FileName = fileName;
            FileMode = OpenFileMode.ImportJSON;
        }

        public void ExportJson()
        {
            if (FileName == null) return;
            
            Json.Export(EventFile, FileName);
        }        

        public void ReencodeEventFile(Encoding newEncoding)
        {
            if (EventFile == null) return;
            
            foreach (var scene in EventFile.Events)
            {
                foreach (var message in scene.Messages)
                {
                    message.Text = newEncoding.GetString(SelectedEncoding.GetBytes(message.Text));
                }
            }
        }

        #endregion

        #region Add/remove messages

        public void AddMessage()
        {
            SelectedScene?.Messages.Add(new SA2EventMessage());
        }

        public void RemoveLastMessage()
        {
            SelectedScene?.Messages.RemoveAt(SelectedScene.Messages.Count - 1);
        }

        public void InsertMessage()
        {
            if (SelectedMessageIndex + 1 < SelectedScene?.Messages.Count)
            {
                SelectedScene?.Messages.Insert(SelectedMessageIndex + 1, new SA2EventMessage());
            }
            else
            {
                SelectedScene?.Messages.Add(new SA2EventMessage());
            }
        }

        public void RemoveSelectedMessage()
        {
            SelectedScene?.Messages.RemoveAt(SelectedMessageIndex);
        }

        #endregion

        #region Search

        public void Search(string text, bool ignoreCase)
        {
            if (EventFile == null) return;
            
            SearchResults = new ObservableCollection<SearchResult>();
            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var scene in EventFile.Events)
            {
                for (int i = 0; i < scene.Messages.Count; i++)
                {
                    var message = scene.Messages[i];

                    if (message.Text != null && message.Text.Contains(text, comparison))
                    {
                        SearchResults.Add(new SearchResult(scene.EventID, i, message.Text));
                    }
                }
            }
        }

        public void ClearSearchResults()
        {
            SearchResults = null;
        }

        public SA2Scene? FindByEventID(int eventID)
        {
            return EventFile?.Events.FirstOrDefault(x => x.EventID == eventID);
        }

        #endregion

        #endregion
    }
}