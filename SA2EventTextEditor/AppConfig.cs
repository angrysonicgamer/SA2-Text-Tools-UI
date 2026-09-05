using SA2EventTextEditor.Common;
using SA2EventTextEditor.JSON;
using System.IO;
using System.Text.Json.Serialization;

namespace SA2EventTextEditor
{
    public class Settings
    {
        public Codepage Encoding { get; set; }
        public int? CustomCodepage { get; set; }
        public Endianness Endianness { get; set; }
        public Language Language { get; set; }


        [JsonConstructor]
        public Settings() { }
    }

    public class Search
    {
        public bool IgnoreCase { get; set; }


        [JsonConstructor]
        public Search() { }
    }
    
    public class AppConfig
    {
        private readonly string _configFile = "AppConfig.json";

        public Settings Settings { get; set; }
        public Search Search { get; set; }
        

        [JsonConstructor]
        public AppConfig() { }


        public void Read()
        {
            Settings = new();
            Search = new();

            if (File.Exists(_configFile))
            {
                var buffer = Json.Import<AppConfig>(_configFile);                
                Settings.Encoding = buffer.Settings.Encoding;
                Settings.CustomCodepage = buffer.Settings.CustomCodepage;
                Settings.Endianness = buffer.Settings.Endianness;
                Settings.Language = buffer.Settings.Language;
                Search.IgnoreCase = buffer.Search.IgnoreCase;
            }
            else
            {
                Settings.Encoding = Codepage.Windows1252;
                Settings.Endianness = Endianness.BigEndian;
                Settings.Language = Language.English;
                Search.IgnoreCase = false;
            }
        }

        public void SetEncoding(Codepage encoding)
        {
            Settings.Encoding = encoding;
            Settings.CustomCodepage = null;
        }

        public void SetEncoding(int customCodepage)
        {
            Settings.Encoding = Codepage.Custom;
            Settings.CustomCodepage = customCodepage;
        }

        public void Save()
        {
            Json.Export(this, _configFile);
        }
    }
}
