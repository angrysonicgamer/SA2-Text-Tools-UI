using SA2EventTextEditor.Extensions;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace SA2EventTextEditor.Common
{
    public class SA2EventMessage : INotifyPropertyChanged
    {
        private int? _character;
        private TextCentering _centering;
        private string? _text;
        
        public int? Character
        {
            get { return _character; }
            set { _character = value.HasValue ? value : -1; OnPropertyChanged(nameof(Character)); }
        }

        public TextCentering TextCentering
        {
            get { return _centering; }
            set { _centering = value; OnPropertyChanged(nameof(TextCentering)); }
        }
        public string? Text
        {
            get { return _text; }
            set { _text = value?.Replace(Environment.NewLine, "\n"); OnPropertyChanged(nameof(Text)); }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        [JsonConstructor]
        public SA2EventMessage() { }

        public SA2EventMessage(int character, TextCentering centering, string text)
        {
            Character = character;
            TextCentering = centering;
            Text = text;
        }

        public void Read(BinaryReader reader, Encoding encoding, Endianness endianness)
        {
            Character = reader.ReadInt32(endianness);
            uint textOffset = reader.ReadUInt32(endianness) - Pointer.BaseAddress;
            string text = reader.ReadAt(textOffset, x => x.ReadCString(encoding));
            TextCentering = GetCenteringMethod(text);
            Text = TextCentering != TextCentering.None ? text.Substring(1) : text;
        }

        public string GetRawString()
        {
            if (Text == null) return string.Empty;
            return TextCentering != TextCentering.None ? $"{(char)TextCentering}{Text}" : Text;
        }


        private static TextCentering GetCenteringMethod(string text)
        {
            if (text.StartsWith('\a'))
                return TextCentering.Block;

            if (text.StartsWith('\t'))
                return TextCentering.All;

            return TextCentering.None;
        }
    }
}
