using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace SA2EventTextEditor.UI.Behaviors
{
    public class ListBoxScrollIntoView : Behavior<ListBox>
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
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                listBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    listBox.UpdateLayout();
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }));
            }
        }
    }
}
