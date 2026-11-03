using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2MsgTextEditor.JSON
{
    public class Json
    {
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };


        public static T? Import<T>(string fileName)
        {
            var json = JsonNode.Parse(File.ReadAllText(fileName));
            return JsonSerializer.Deserialize<T>(json);
        }

        public static void Export<T>(T contents, string fileName)
        {
            var json = JsonSerializer.Serialize(contents, _serializerOptions);
            File.WriteAllText(fileName, json);
        }
    }
}
