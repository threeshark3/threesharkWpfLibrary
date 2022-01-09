using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace threesharkWpfLibrary.Controls
{
    public class GeneralEditor : ContentControl
    {
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            var type = newContent.GetType();
            var props = type.GetProperties(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public);
            var grid = new FrameworkElementFactory(typeof(Grid));
            var nameGridColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
            nameGridColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1.0, GridUnitType.Auto));
            grid.AppendChild(nameGridColumn);

            var gridSplitter = new FrameworkElementFactory(typeof(GridSplitter));
            gridSplitter.SetValue(WidthProperty, 4D);
            gridSplitter.SetValue(BackgroundProperty, SystemColors.GrayTextBrush);
            gridSplitter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Stretch);

            var editorGridColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
            grid.AppendChild(editorGridColumn);
            var count = 0;
            for (int i = 0; i < props.Length; i++)
            {
                var property = props[i];
                var propertyName = property.Name;

                var gridRow = new FrameworkElementFactory(typeof(RowDefinition));
                gridRow.SetValue(RowDefinition.MinHeightProperty, 20D);
                gridRow.SetValue(RowDefinition.HeightProperty, new GridLength(1.0, GridUnitType.Auto));
                grid.AppendChild(gridRow);

                var nameBlock = new FrameworkElementFactory(typeof(TextBlock));
                nameBlock.SetValue(TextBlock.TextProperty, propertyName);
                nameBlock.SetValue(Grid.ColumnProperty, 0);
                nameBlock.SetValue(Grid.RowProperty, i);
                grid.AppendChild(nameBlock);

                var editor = new FrameworkElementFactory(typeof(TextBox));
                var isReadOnly = !property.CanWrite;
                editor.SetBinding(TextBox.TextProperty, new Binding(propertyName)
                {
                    Mode = isReadOnly
                        ? BindingMode.OneWay
                        : BindingMode.TwoWay,
                });
                editor.SetValue(Grid.ColumnProperty, 1);
                editor.SetValue(Grid.RowProperty, i);
                editor.SetValue(TextBox.IsReadOnlyProperty, isReadOnly);
                grid.AppendChild(editor);

                count++;
            }

            gridSplitter.SetValue(Grid.RowSpanProperty, count);
            grid.AppendChild(gridSplitter);

            var template = new DataTemplate(typeof(GeneralEditor));
            template.VisualTree = grid;
            template.Seal();
            this.ContentTemplate = template;
        }
    }
}
