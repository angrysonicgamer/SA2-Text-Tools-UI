using SA2EventTextEditor.PRS;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace SA2EventTextEditor.Common
{
    public class SA2EventFile
    {
        private readonly string _fileName;

        public string Name { get; set; }
        public ObservableCollection<SA2Scene> Events { get; set; }

        
        [JsonConstructor]
        public SA2EventFile() { }

        public SA2EventFile(string fileName)
        {
            _fileName = fileName;
            Name = Path.GetFileNameWithoutExtension(_fileName);
            Events = new ObservableCollection<SA2Scene>();            
        }

        public Endianness DetectEndianness()
        {
            using PrsReader reader = new PrsReader(_fileName);
            return reader.DetectEndianness();
        }

        public void ReadEventData(Encoding encoding, Endianness endianness)
        {
            using PrsReader reader = new PrsReader(_fileName);
            Events = reader.ReadEventData(encoding, endianness);
        }

        public void Reencode(Encoding selectedEncoding, Encoding newEncoding)
        {
            foreach (var scene in Events)
            {
                foreach (var message in scene.Messages)
                {
                    message.Text = newEncoding.GetString(selectedEncoding.GetBytes(message.Text));
                }
            }
        }

        public void Save(string fileName, Encoding encoding, Endianness endianness)
        {
            using PrsWriter writer = new PrsWriter();
            writer.WriteToBuffer(Events, encoding, endianness);
            writer.WriteBufferToFile(fileName);
        }

        public List<SearchResult> Search(string text, bool ignoreCase)
        {
            var searchResults = new List<SearchResult>();
            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var scene in Events)
            {
                for (int i = 0; i < scene.Messages.Count; i++)
                {
                    var message = scene.Messages[i];

                    if (message.Text != null && message.Text.Contains(text, comparison))
                    {
                        searchResults.Add(new SearchResult(scene.EventID, i, message.Text));
                    }
                }
            }

            return searchResults;            
        }

        public SA2Scene? FindByEventID(int eventID)
        {
            return Events.FirstOrDefault(x => x.EventID == eventID);
        }
    }
}
