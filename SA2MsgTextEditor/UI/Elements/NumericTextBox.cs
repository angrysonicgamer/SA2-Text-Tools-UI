using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2MsgTextEditor.UI.Elements
{
    public class NumericTextBox : TextBox
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DataObject.AddPastingHandler(this, OnPaste);
            PreviewTextInput += OnPreviewTextInput;
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
