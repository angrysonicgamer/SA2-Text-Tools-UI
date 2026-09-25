using System.Windows;
using System.Windows.Input;

namespace SA2EventTextEditor.UI
{
    /// <summary>
    /// Interaction logic for InputCustomCodepage.xaml
    /// </summary>
    public partial class CustomCodepageDialog : Window
    {
        public int? Codepage { get; set; }
        
        public CustomCodepageDialog()
        {
            InitializeComponent();            
        }

        private void WindowCustomCodepage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomCodepage.Text = Codepage.ToString();
        }


        // Buttons

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Codepage = int.Parse(CustomCodepage.Text);
            DialogResult = true;
            Close();
        }        

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CustomCodepage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonOK_Click(sender, e);
            }
        }
    }
}
