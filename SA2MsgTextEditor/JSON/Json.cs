using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2MsgTextEditor.JSON
{
    public class Json
    {
        public static T? Import<T>(string fileName)
        {
            var json = JsonNode.Parse(File.ReadAllText(fileName));
            return JsonSerializer.Deserialize<T>(json);
        }

        public static void Export<T>(T contents, string fileName)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(contents, options);
            File.WriteAllText(fileName, json);
        }
    }
}
