using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SA2EventTextEditor.Common
{
    public class SA2Scene : PropertyChangedNotifier
    {
        private int _eventID;
        private ObservableCollection<SA2EventMessage> _messages;


        public int EventID
        {
            get => _eventID;
            set { _eventID = value; NotifyPropertyChanged(); }
        }
        public ObservableCollection<SA2EventMessage> Messages
        {
            get => _messages;
            set { _messages = value; NotifyPropertyChanged(); }
        }


        [JsonConstructor]
        public SA2Scene() { }

        public SA2Scene(int id, ObservableCollection<SA2EventMessage> messages)
        {
            EventID = id;
            Messages = messages;
        }
    }
}
