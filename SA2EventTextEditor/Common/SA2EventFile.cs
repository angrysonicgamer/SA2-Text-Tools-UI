using SA2EventTextEditor.PRS;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace SA2EventTextEditor.Common
{
    public class SA2EventFile : PropertyChangedNotifier
    {
        private readonly string _fileName;
        private ObservableCollection<SA2Scene> _events;


        public string Name { get; set; }
        public ObservableCollection<SA2Scene> Events
        {
            get => _events;
            set { _events = value; NotifyPropertyChanged(); }
        }

        
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

        public void Save(string fileName, Encoding encoding, Endianness endianness)
        {
            using PrsWriter writer = new PrsWriter();
            writer.WriteToBuffer(Events, encoding, endianness);
            writer.WriteBufferToFile(fileName);
        }
    }
}
