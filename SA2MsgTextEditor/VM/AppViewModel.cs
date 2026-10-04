using SA2MsgTextEditor.Common;
using SA2MsgTextEditor.Extensions;
using SA2MsgTextEditor.JSON;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SA2MsgTextEditor.VM
{
    public class AppViewModel : PropertyChangedNotifier
    {
        #region Properties raising PropertyChanged

        private Encoding _selectedEncoding;
        private Endianness _selectedEndianness;
        private string? _fileName;
        private OpenFileMode _mode;
        private SA2MessageFile? _messageFile;
        private MessageFileType _fileType;
        private SA2MessageGroup? _selectedGroup;
        private int _selectedGroupIndex;
        private int _selectedMessageIndex;
        private ObservableCollection<SearchResult>? _searchResults;


        public Encoding SelectedEncoding
        {
            get => _selectedEncoding;
            set { _selectedEncoding = value; NotifyPropertyChanged(nameof(SelectedEncoding), nameof(EncodingAsString)); }
        }
        public Endianness SelectedEndianness
        {
            get => _selectedEndianness;
            set { _selectedEndianness = value; NotifyPropertyChanged(nameof(SelectedEndianness), nameof(EndiannessAsString)); }
        }
        public string? FileName
        {
            get => _fileName;
            set { _fileName = value; NotifyPropertyChanged(nameof(FileName), nameof(Title)); }
        }
        public OpenFileMode FileMode
        {
            get => _mode;
            set { _mode = value; NotifyPropertyChanged(nameof(FileMode), nameof(IsPRS)); }
        }
        public SA2MessageFile? MessageFile
        {
            get => _messageFile;
            set { _messageFile = value; NotifyPropertyChanged(nameof(MessageFile), nameof(MessageFileLoaded)); }
        }
        public MessageFileType FileType
        {
            get => _fileType;
            set
            {
                _fileType = value;
                NotifyPropertyChanged(nameof(FileType), nameof(FileTypeAsString),
                    nameof(IsHuntingOrGameplayFile), nameof(IsHuntingFile), nameof(IsGameplayFile), nameof(IsChaoNamesFile), nameof(IsNotChaoNamesFile),
                    nameof(MessageGroupsTitle), nameof(MessageListTitle));
            }
        }
        public SA2MessageGroup? SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; NotifyPropertyChanged(nameof(SelectedGroup), nameof(AnyGroupIsSelected)); }
        }
        public int SelectedGroupIndex
        {
            get => _selectedGroupIndex;
            set { _selectedGroupIndex = value; NotifyPropertyChanged(nameof(SelectedGroupIndex), nameof(SelectedGroupIndexAsString)); }
        }
        public int SelectedMessageIndex
        {
            get => _selectedMessageIndex;
            set { _selectedMessageIndex = value; NotifyPropertyChanged(nameof(SelectedMessageIndex), nameof(AnyMessageIsSelected), nameof(SelectedMessageIndexAsString)); }
        }
        public ObservableCollection<SearchResult>? SearchResults
        {
            get => _searchResults;
            set { _searchResults = value; NotifyPropertyChanged(nameof(SearchResults)); }
        }

        #endregion


        #region Other properties

        public string Title => MessageFileLoaded ? $"{App.GetString("App.Title")} — {Path.GetFileName(FileName)}" : App.GetString("App.Title");
        public string EncodingAsString
        {
            get
            {
                string encoding = App.GetString(App.Config.Settings.Encoding.GetDisplayName());
                return App.Config.Settings.CustomCodepage.HasValue ? $"{encoding}: {App.Config.Settings.CustomCodepage.Value}" : encoding;
            }
        }
        public string EndiannessAsString => App.GetString(SelectedEndianness.GetDisplayName());
        public bool MessageFileLoaded => MessageFile != null;
        public bool IsPRS => FileMode == OpenFileMode.OpenPRS;
        public string FileTypeAsString => FileType != MessageFileType.NotLoaded ? App.GetString(FileType.GetDisplayName()) : "";        
        public bool AnyGroupIsSelected => SelectedGroup != null;
        public bool AnyMessageIsSelected => SelectedMessageIndex != -1;
        public string SelectedGroupIndexAsString => AnyGroupIsSelected ? SelectedGroupIndex.ToString() : App.GetString("Status.SelectedItem.None");
        public string SelectedMessageIndexAsString => AnyMessageIsSelected ? SelectedMessageIndex.ToString() : App.GetString("Status.SelectedItem.None");
        public int Codepage { get; set; }
        public string? LastSearchString { get; set; }
        public bool IsHuntingOrGameplayFile => FileType == MessageFileType.HuntingHints || FileType == MessageFileType.GameplayMessages;
        public bool IsHuntingFile => FileType == MessageFileType.HuntingHints;
        public bool IsGameplayFile => FileType == MessageFileType.GameplayMessages;
        public bool IsChaoNamesFile => FileType == MessageFileType.ChaoNames;
        public bool IsNotChaoNamesFile => FileType != MessageFileType.ChaoNames;
        public string MessageGroupsTitle => IsHuntingFile ? App.GetString("Editor.EmeraldsList") : App.GetString("Editor.GroupList");
        public string MessageListTitle => IsHuntingFile ? App.GetString("Editor.EmeraldHintsList") : App.GetString("Editor.MessageList");

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
            NotifyPropertyChanged(nameof(Title), nameof(FileTypeAsString), nameof(EncodingAsString), nameof(EndiannessAsString),
                nameof(SelectedGroupIndexAsString), nameof(SelectedMessageIndexAsString), nameof(MessageGroupsTitle), nameof(MessageListTitle));

            if (MessageFile == null || MessageFile.MessageGroups ==  null) return;

            foreach (var group in MessageFile.MessageGroups)
            {
                group.NotifyPropertyChanged(nameof(group.AsString));
                
                foreach (var msg in group.Group)
                {
                    msg.NotifyPropertyChanged(nameof(msg.TextCentering));
                }
            }
        }

        #endregion

        #region File

        public void LoadMessageFile(string fileName)
        {
            MessageFile = new SA2MessageFile(fileName);
            FileName = fileName;
            FileMode = OpenFileMode.OpenPRS;
        }

        public Endianness DetectEndianness()
        {
            return MessageFile.DetectEndianness();
        }

        public void ReadMessages()
        {
            MessageFile?.ReadMessages(SelectedEncoding, SelectedEndianness);
            FileType = MessageFile.Type;
            SelectedGroupIndex = 0;
        }

        public void ClearMessageFile()
        {
            MessageFile = null;
            FileName = null;
            FileMode = OpenFileMode.NoFile;
            FileType = MessageFileType.NotLoaded;
        }

        public void ImportFromJson(string fileName)
        {
            MessageFile = Json.Import<SA2MessageFile>(fileName);
            FileName = fileName;
            FileMode = OpenFileMode.ImportJSON;
            FileType = MessageFile.Type;
            SelectedGroupIndex = 0;
        }        

        public void SaveMessageFile()
        {
            if (FileName == null) return;
            
            MessageFile?.Save(FileName, SelectedEncoding, SelectedEndianness);
        }

        public void ExportJSON()
        {
            if (FileName == null) return;
            
            Json.Export(MessageFile, FileName);
        }

        public void ReencodeMessageFile(Encoding newEncoding)
        {
            if (MessageFile == null) return;
            if (MessageFile.MessageGroups == null) return;
            if (MessageFile.Type == MessageFileType.ChaoNames) return;

            foreach (var group in MessageFile.MessageGroups)
            {
                foreach (var message in group.Group)
                {
                    message.Text = newEncoding.GetString(SelectedEncoding.GetBytes(message.Text));
                }

                group.NotifyPropertyChanged(nameof(group.AsString));
            }
        }

        #endregion

        #region Add/remove items

        public void AddMessage()
        {
            SelectedGroup?.Group.Add(new SA2Message());
        }

        public void RemoveLastMessage()
        {
            SelectedGroup?.Group.RemoveAt(SelectedGroup.Group.Count - 1);
        }

        public void InsertMessage()
        {
            if (SelectedMessageIndex + 1 < SelectedGroup?.Group.Count)
            {
                SelectedGroup?.Group.Insert(SelectedMessageIndex + 1, new SA2Message());
            }
            else
            {
                SelectedGroup?.Group.Add(new SA2Message());
            }
        }

        public void RemoveSelectedMessage()
        {
            SelectedGroup?.Group.RemoveAt(SelectedMessageIndex);
        }

        #endregion

        #region Search

        public void Search(string text, bool ignoreCase)
        {
            if (MessageFile == null) return;

            SearchResults = new ObservableCollection<SearchResult>();
            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            for (int group = 0; group < MessageFile.MessageGroups?.Count; group++)
            {
                for (int msg = 0; msg < MessageFile.MessageGroups[group].Group.Count; msg++)
                {
                    var message = MessageFile.MessageGroups[group].Group[msg];

                    if (message.Text != null && message.Text.Contains(text, comparison))
                    {
                        SearchResults.Add(new SearchResult(group, msg, message.Text));
                    }
                }
            }
        }

        public void ClearSearchResults()
        {
            SearchResults = null;
        }

        #endregion

        #endregion
    }
}
