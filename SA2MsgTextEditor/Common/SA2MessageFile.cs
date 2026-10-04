using SA2MsgTextEditor.PRS;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace SA2MsgTextEditor.Common
{
    public class SA2MessageFile : PropertyChangedNotifier
    {
        private string _fileName;
        private ObservableCollection<SA2MessageGroup>? _messageGroups;

        public string Name { get; set; }
        public MessageFileType Type { get; set; }
        public ObservableCollection<SA2MessageGroup>? MessageGroups
        {
            get => _messageGroups;
            set { _messageGroups = value; NotifyPropertyChanged(nameof(MessageGroups)); }
        }


        [JsonConstructor]
        public SA2MessageFile() { }

        public SA2MessageFile(string fileName)
        {
            _fileName = fileName;
            Name = Path.GetFileNameWithoutExtension(_fileName);
            Type = GetFileType(Name);
        }


        public void ReadMessages(Encoding encoding, Endianness endianness)
        {
            using PrsReader reader = new PrsReader(_fileName);
            var offsets = reader.ReadOffsets(endianness);

            switch(this.Type)
            {
                case MessageFileType.GameplayMessages:
                    MessageGroups = reader.ReadGameplayMessages(offsets, encoding);
                    break;
                case MessageFileType.HuntingHints:
                    MessageGroups = reader.ReadEmeraldHints(offsets, encoding);
                    break;
                case MessageFileType.SimpleTextArray:
                    MessageGroups = reader.ReadSimpleText(offsets, encoding);
                    break;
                case MessageFileType.ChaoNames:
                    MessageGroups = reader.ReadChaoNames(offsets, encoding);
                    break;
            }
        }        

        public void Save(string fileName, Encoding encoding, Endianness endianness)
        {
            var rawStrings = GetRawStrings(encoding);
            using PrsWriter writer = new PrsWriter();

            writer.WriteToBuffer(rawStrings, encoding, endianness);            
            writer.WriteBufferToFile(fileName);
        }

        public Endianness DetectEndianness()
        {
            using PrsReader reader = new PrsReader(_fileName);
            return reader.DetectEndianness();
        }


        private MessageFileType GetFileType(string fileName)
        {
            if (fileName.StartsWith("eh", StringComparison.OrdinalIgnoreCase))
            {
                return MessageFileType.HuntingHints;
            }
            else if (fileName.StartsWith("mh", StringComparison.OrdinalIgnoreCase))
            {
                return MessageFileType.GameplayMessages;
            }
            else if (fileName.StartsWith("msgalkinderfoname", StringComparison.OrdinalIgnoreCase))
            {
                return MessageFileType.ChaoNames;
            }
            else
            {
                return MessageFileType.SimpleTextArray;
            }
        }

        private List<string> GetRawStrings(Encoding encoding)
        {
            var rawStrings = new List<string>();

            if (Type == MessageFileType.GameplayMessages)
            {
                foreach (var group in MessageGroups)
                {
                    var builder = new StringBuilder();

                    foreach (var message in group.Group)
                    {
                        builder.Append(message.GetRawText(encoding));
                    }

                    string text = builder.ToString();
                    rawStrings.Add(text);
                }
            }
            else if (Type == MessageFileType.HuntingHints)
            {
                foreach (var group in MessageGroups)
                {
                    foreach (var message in group.Group)
                    {
                        rawStrings.Add(message.GetRawText(encoding));
                    }
                }
            }
            else if (Type == MessageFileType.SimpleTextArray)
            {
                foreach (var message in MessageGroups[0].Group)
                {
                    rawStrings.Add(message.GetRawText(encoding));
                }
            }
            else
            {
                foreach (var message in MessageGroups[0].Group)
                {
                    rawStrings.Add(message.GetRawChaoText(encoding));
                }
            }
            
            return rawStrings;
        }
    }
}
