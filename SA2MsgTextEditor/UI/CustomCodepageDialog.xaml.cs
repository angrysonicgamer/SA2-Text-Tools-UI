using System.Windows;
using System.Windows.Input;

namespace SA2MsgTextEditor.UI
{
    /// <summary>
    /// Interaction logic for InputCustomCodepage.xaml
    /// </summary>
    public partial class CustomCodepageDialog : Window
    {
        public CustomCodepageDialog()
        {
            InitializeComponent();
            DataContext = App.VM;
        }


        // Buttons

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
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
