using System.Text;
using SA2MsgTextEditor.Common;

namespace SA2MsgTextEditor.Extensions
{
    public static class ExString
    {
        private static readonly Dictionary<string, string> _buttonsMap = new()
        {
            { "±", "{A}" },
            { "¶", "{B}" },
            { "Ё", "{Y}" },
        };

        private static readonly Dictionary<string, string> _cyrillicButtonsMap = new()
        {
            { "±", "{A}" },
            { "¶", "{B}" },
            { "·", "{Y}" },
        };

        private static string Replace(string text, Dictionary<string, string> map, TextConversionMode mode = TextConversionMode.Default)
        {
            foreach (var pair in map)
            {
                if (mode == TextConversionMode.Default)
                    text = text.Replace(pair.Key, pair.Value);
                else
                    text = text.Replace(pair.Value, pair.Key);
            }

            return text;
        }


        public static string ReplaceKeyboardButtons(this string text, Encoding encoding, TextConversionMode mode = TextConversionMode.Default)
        {
            if (encoding.CodePage == (int)Codepage.Windows1251)
            {
                return Replace(text, _cyrillicButtonsMap, mode);
            }
            else
            {
                return Replace(text, _buttonsMap, mode);
            }
        }

        public static string ReplaceUrlSignature(this string text, Encoding encoding, TextConversionMode mode = TextConversionMode.Default)
        {
            Dictionary<string, string> urlTags = new()
            {
                { encoding.GetString([0x0E, 0xFF, 0x11]), "<url>" },
                { encoding.GetString([0xFF, 0x10, 0x0F]), "</url>" }
            };

            return Replace(text, urlTags, mode);
        }
    }
}
