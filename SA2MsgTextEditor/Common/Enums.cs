using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SA2MsgTextEditor.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TextCentering : short
    {
        [Display(Name = "TextCentering.None", Description = "TextCentering.None.Description")]
        None,

        [Display(Name = "TextCentering.Block", Description = "TextCentering.Block.Description")]
        Block = 7,  // \a

        [Display(Name = "TextCentering.All", Description = "TextCentering.All.Description")]
        All = 9     // \t
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Endianness
    {
        [Display(Name = "Status.Endianness.Auto")]
        Auto,

        [Display(Name = "Status.Endianness.BigEndian")]
        BigEndian,

        [Display(Name = "Status.Endianness.LittleEndian")]
        LittleEndian
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Codepage
    {
        [Display(Name = "Codepage.Cyrillic")]
        Windows1251 = 1251,

        [Display(Name = "Codepage.Latin")]
        Windows1252 = 1252,

        [Display(Name = "Codepage.Japanese")]
        ShiftJIS = 932,

        [Display(Name = "Status.CustomCodepage")]
        Custom = 0
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageFileType
    {
        [Display(Name = "Status.FileType.HuntingHints")]
        HuntingHints,

        [Display(Name = "Status.FileType.GameplayMessages")]
        GameplayMessages,

        [Display(Name = "Status.FileType.SimpleText")]
        SimpleTextArray,

        [Display(Name = "Status.FileType.ChaoNames")]
        ChaoNames
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Language
    {
        English,
        Russian,
        Japanese
    }

    public enum TextConversionMode
    {
        Default,
        Reversed,
    }

    public enum OpenFileMode
    {
        OpenPRS,
        ImportJSON
    }
}
