using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SA2MsgTextEditor.UI
{
    public class DataGridNumericColumn : DataGridTextColumn
    {
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

            if (!IsDataValid(data))
            {
                e.CancelCommand();
            }                
        }

        private void Edit_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
