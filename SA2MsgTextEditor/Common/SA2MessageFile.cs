using SA2MsgTextEditor.PRS;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace SA2MsgTextEditor.Common
{
    public class SA2MessageFile
    {
        private string _fileName;

        public string Name { get; set; }
        public MessageFileType Type { get; set; }
        public ObservableCollection<ObservableCollection<SA2Message>>? Messages { get; set; }


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
                    Messages = reader.ReadGameplayMessages(offsets, encoding);
                    break;
                case MessageFileType.HuntingHints:
                    Messages = reader.ReadEmeraldHints(offsets, encoding);
                    break;
                case MessageFileType.SimpleTextArray:
                    Messages = reader.ReadSimpleText(offsets, encoding);
                    break;
                case MessageFileType.ChaoNames:
                    Messages = reader.ReadChaoNames(offsets, encoding);
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

        public void Reencode(Encoding selectedEncoding, Encoding newEncoding)
        {
            if (Messages == null) return;
            if (Type == MessageFileType.ChaoNames) return;
            
            foreach (var group in Messages)
            {
                foreach (var message in group)
                {
                    message.Text = newEncoding.GetString(selectedEncoding.GetBytes(message.Text));
                }
            }
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

            if (Type == MessageFileType.GameplayMessages || Type == MessageFileType.HuntingHints)
            {
                foreach (var group in Messages)
                {
                    var builder = new StringBuilder();

                    foreach (var message in group)
                    {
                        builder.Append(message.GetRawText(encoding));
                    }

                    string text = builder.ToString();
                    rawStrings.Add(text);
                }
            }
            else if (Type == MessageFileType.SimpleTextArray)
            {
                foreach (var message in Messages[0])
                {
                    rawStrings.Add(message.GetRawText(encoding));
                }
            }
            else
            {
                foreach (var message in Messages[0])
                {
                    rawStrings.Add(message.GetRawChaoText(encoding));
                }
            }
            
            return rawStrings;
        }
    }
}
