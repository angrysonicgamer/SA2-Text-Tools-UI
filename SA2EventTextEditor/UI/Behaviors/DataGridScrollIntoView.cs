using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace SA2EventTextEditor.UI.Behaviors
{
    public class DataGridScrollIntoView : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dataGrid.UpdateLayout();
                    dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                }));
            }
        }
    }
}
