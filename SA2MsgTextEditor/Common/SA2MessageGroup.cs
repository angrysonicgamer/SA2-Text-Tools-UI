using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SA2MsgTextEditor.Common
{
    public class SA2MessageGroup : PropertyChangedNotifier
    {
        private ObservableCollection<SA2Message> _value;

        public ObservableCollection<SA2Message> Group
        {
            get => _value;
            set { _value = value; NotifyPropertyChanged(nameof(Group), nameof(AsString)); }
        }

        [JsonIgnore]
        public string? AsString => Group.Count > 0 && !string.IsNullOrEmpty(Group[0].Text) ? Group[0].Text : App.GetString("ListBox.EmptyString");


        public SA2MessageGroup(ObservableCollection<SA2Message> group)
        {
            Group = group;
        }
    }
}
