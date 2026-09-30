using SA2EventTextEditor.Config;
using SA2EventTextEditor.Common;
using System.Text;
using System.Windows;
using SA2EventTextEditor.VM;

namespace SA2EventTextEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AppConfig Config { get; set; } = new AppConfig();
        public static AppViewModel VM { get; set; } = new AppViewModel();

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
            
            Config.Settings.Language = language;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            Config.Read();
            SetLanguage(Config.Settings.Language);
            base.OnStartup(e);
        }
    }
}
