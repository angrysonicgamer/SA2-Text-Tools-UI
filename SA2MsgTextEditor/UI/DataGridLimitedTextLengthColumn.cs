using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2MsgTextEditor.UI
{
    public class DataGridLimitedTextLengthColumn : DataGridTextColumn
    {
        private static readonly int _maxLength = 7;
        
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            var edit = editingElement as TextBox;
            edit.PreviewTextInput += Edit_PreviewTextInput;
            DataObject.AddPastingHandler(edit, OnPaste);
            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var data = e.SourceDataObject.GetData(DataFormats.Text);

            if (sender is TextBox textBox && data is string name && textBox.Text.Length + name.Length > _maxLength)
            {
                e.CancelCommand();
            }
        }

        private void Edit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                e.Handled = textBox.Text.Length >= _maxLength;
            }            
        }
    }
}
