using SA2EventTextEditor.Common;
using System.Text;
using System.Windows;

namespace SA2EventTextEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AppConfig Config = new AppConfig();        

        public static string GetString(string key)
        {
            var res = Current.TryFindResource(key).ToString();

            if (res is string str)
            {
                return str;
            }

            return key;
        }

        public static void SetLanguage(Language language)
        {
            if (Current.Resources.MergedDictionaries.Count == 2) // Means that language other than English was set, removing dictionary for it
            {
                Current.Resources.MergedDictionaries.RemoveAt(1);
            }

            if (language != Language.English)
            {
                Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"Languages/{language}.xaml", UriKind.Relative) });
            }
            
            Config.Language = language;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Config.Read();
            SetLanguage(Config.Language);
            base.OnStartup(e);
        }
    }
}
