using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using SA2MsgTextEditor.Extensions;

namespace SA2MsgTextEditor.Common
{
    public class SA2Message : INotifyPropertyChanged
    {
        private string? _voice;
        private string? _framecount;
        private bool _is2p;
        private TextCentering _centering;
        private string? _text;

        public string? Voice
        {
            get { return _voice; }
            set { _voice = !string.IsNullOrEmpty(value) ? $"{int.Parse(value)}" : null; OnPropertyChanged(nameof(Voice)); }
        }
        public string? FrameCount
        {
            get { return _framecount; }
            set { _framecount = !string.IsNullOrEmpty(value) ? $"{int.Parse(value)}" : null; OnPropertyChanged(nameof(FrameCount)); }
        }
        public bool Is2PPiece
        {
            get { return _is2p; }
            set { _is2p = value; OnPropertyChanged(nameof(Is2PPiece)); }
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


        // actual methods

        private static TextCentering GetCenteringMethod(string text)
        {
            if (text.StartsWith('\a'))
                return TextCentering.Block;

            if (text.StartsWith('\t'))
                return TextCentering.All;

            return TextCentering.None;
        }

        public void Parse(string rawText, Encoding encoding)
        {
            rawText = rawText.ReplaceKeyboardButtons(encoding);

            if (rawText.StartsWith('\x0C'))
            {
                int indexSpace = rawText.IndexOf(' ');
                string controls = rawText.Substring(0, indexSpace);                
                int indexS = controls.IndexOf('s');
                int indexW = controls.IndexOf('w');
                Voice = indexS != -1 ? (indexW != -1 ? controls.Substring(indexS + 1, indexW - indexS - 1) : controls.Substring(indexS + 1, controls.Length - indexS - 1)) : null;
                FrameCount = indexW != -1 ? controls.Substring(indexW + 1) : null;
                Is2PPiece = controls.Contains('D');
                rawText = rawText.Substring(indexSpace + 1);
            }

            TextCentering = GetCenteringMethod(rawText);
            Text = TextCentering != TextCentering.None ? rawText.Substring(1) : rawText;
        }

        public string GetRawText(Encoding encoding)
        {
            var builder = new StringBuilder();
            bool hasControls = !string.IsNullOrEmpty(Voice) || !string.IsNullOrEmpty(FrameCount) || Is2PPiece;

            if (hasControls)
            {
                builder.Append('\x0C');

                if (!string.IsNullOrEmpty(Voice))
                {
                    builder.Append($"s{Voice}");
                }

                if (!string.IsNullOrEmpty(FrameCount))
                {
                    builder.Append($"w{FrameCount}");
                }

                if (Is2PPiece)
                {
                    builder.Append('D');
                }

                builder.Append(' ');
            }

            if (TextCentering != TextCentering.None)
            {
                builder.Append((char)TextCentering);
            }

            builder.Append(Text);
            return builder.ToString().ReplaceKeyboardButtons(encoding, TextConversionMode.Reversed);
        }

        public string GetRawChaoText(Encoding encoding)
        {
            bool useCyrillic = encoding == Encoding.GetEncoding((int)Codepage.Windows1251);
            var converter = new ChaoTextConverter(useCyrillic);
            return converter.ToRaw(Text);
        }
    }
}
