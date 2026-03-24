using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SA2EventTextEditor.Common
{
    public class SA2Scene
    {
        public int EventID { get; set; }
        public ObservableCollection<SA2EventMessage> Messages { get; set; }


        [JsonConstructor]
        public SA2Scene() { }

        public SA2Scene(int id, ObservableCollection<SA2EventMessage> messages)
        {
            EventID = id;
            Messages = messages;
        }
    }
}
