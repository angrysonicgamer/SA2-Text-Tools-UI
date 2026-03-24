using SA2MsgTextEditor.Common;
using SA2MsgTextEditor.JSON;
using System.IO;
using System.Text.Json.Serialization;

namespace SA2MsgTextEditor
{
    public class AppConfig
    {
        private readonly string configFile = "AppConfig.json";

        public Codepage Encoding { get; set; }
        public int? CustomCodepage { get; set; }
        public Endianness Endianness { get; set; }
        public Language Language { get; set; }

        [JsonConstructor]
        public AppConfig() { }


        public void Read()
        {
            if (File.Exists(configFile))
            {
                var buffer = Json.Import<AppConfig>(configFile);
                Encoding = buffer.Encoding;
                CustomCodepage = buffer.CustomCodepage;
                Endianness = buffer.Endianness;
                Language = buffer.Language;
            }
            else
            {
                Encoding = Codepage.Windows1252;
                Endianness = Endianness.BigEndian;
                Language = Language.English;
            }
        }

        public void SetEncoding(Codepage encoding)
        {
            Encoding = encoding;
            CustomCodepage = null;
        }

        public void SetEncoding(int customCodepage)
        {
            Encoding = Codepage.Custom;
            CustomCodepage = customCodepage;
        }

        public void Save()
        {
            Json.Export(this, configFile);
        }
    }
}
