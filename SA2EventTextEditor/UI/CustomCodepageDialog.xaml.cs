using System.Windows;
using System.Windows.Input;

namespace SA2EventTextEditor.UI
{
    /// <summary>
    /// Interaction logic for InputCustomCodepage.xaml
    /// </summary>
    public partial class InputCustomCodepage : Window
    {
        public int? Codepage { get; set; }
        
        public InputCustomCodepage()
        {
            InitializeComponent();            
        }

        private void WindowCustomCodepage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomCodepage.Text = Codepage.ToString();
        }

        
        // Making the text box accept only numbers
        
        private bool IsDataValid(object data)
        {
            try
            {
                Convert.ToInt32(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void CustomCodepage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDataValid(e.Text);
        }

        private void CustomCodepage_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var data = e.SourceDataObject.GetData(DataFormats.Text);

            if (!IsDataValid(data))
            {
                e.CancelCommand();
            }
        }


        // Buttons

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Codepage = int.Parse(CustomCodepage.Text);
            DialogResult = true;
            Close();
        }

        private void CustomCodepage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonOK_Click(sender, e);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
