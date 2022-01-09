using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace threesharkWpfLibrary.Behaviors
{
    public class DataGridSelectedItemsProxyBehavior : Behavior<DataGrid>
    {
        public IEnumerable SelectedItemsProxy
        {
            get { return (IEnumerable)GetValue(SelectedItemsProxyProperty); }
            set { SetValue(SelectedItemsProxyProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProxyProperty =
            DependencyProperty.Register(nameof(SelectedItemsProxy), 
                typeof(IEnumerable), 
                typeof(DataGridSelectedItemsProxyBehavior), 
                new FrameworkPropertyMetadata(Enumerable.Empty<object>(),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedCellsChanged += OnSelectedCellsChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectedCellsChanged -= OnSelectedCellsChanged;
        }

        private void OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            this.SetCurrentValue(SelectedItemsProxyProperty,
                AssociatedObject.SelectedCells
                    .Where(x => x.IsValid)
                    .Select(x => x.Item)
                    .Distinct());
        }
    }
}
