using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2EventTextEditor.UI.Elements
{
    public class NumericTextBox : TextBox
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DataObject.AddPastingHandler(this, OnPaste);
            PreviewTextInput += OnPreviewTextInput;
            KeyUp += OnKeyUp;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var data = e.SourceDataObject.GetData(DataFormats.Text);

            if (!IsDataValid(data))
            {
                e.CancelCommand();
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDataValid(e.Text);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            int number = int.Parse(Text);

            if (e.Key == Key.Up)
            {                
                Text = (number + 1).ToString();
            }
            else if (e.Key == Key.Down)
            {
                Text = (number - 1).ToString();
            }
        }

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
    }
}
