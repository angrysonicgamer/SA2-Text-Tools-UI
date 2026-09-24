using SA2EventTextEditor.Common;
using System.Text.Json.Serialization;

namespace SA2EventTextEditor.Config
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
}
