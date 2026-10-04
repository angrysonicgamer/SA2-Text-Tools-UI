using System.ComponentModel;

namespace SA2MsgTextEditor.Common
{
    public class PropertyChangedNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var property in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
