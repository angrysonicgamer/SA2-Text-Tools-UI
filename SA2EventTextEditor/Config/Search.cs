using System.Text.Json.Serialization;

namespace SA2EventTextEditor.Config
{
    public class Search
    {
        public bool IgnoreCase { get; set; }


        [JsonConstructor]
        public Search() { }
    }
}
