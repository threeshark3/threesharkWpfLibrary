using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace threesharkWpfLibrary
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfCustomControlLibrary"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfCustomControlLibrary;assembly=WpfCustomControlLibrary"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class MultiSelectComboBox : ComboBox
    {
        private TextBox editableTextBox;
        private ItemsControl selectedItemsControl;
        private PropertyInfo displayMemberPropertyInfo;

        public static RoutedCommand RemoveSelectedItem { get; }
            = new RoutedCommand(nameof(RemoveSelectedItem), typeof(MultiSelectComboBox));

        public IEnumerable<object> Source
        {
            get { return (IEnumerable<object>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(IEnumerable<object>), typeof(MultiSelectComboBox), new PropertyMetadata(null, (d, e) =>
            {
                var box = (MultiSelectComboBox)d;

                box.UpdateFilteredSource();
            }));



        public IEnumerable<object> FilteredSource
        {
            get { return (IEnumerable<object>)GetValue(FilteredSourceProperty); }
            set { SetValue(FilteredSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilteredSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilteredSourceProperty =
            DependencyProperty.Register("FilteredSource", typeof(IEnumerable<object>), typeof(MultiSelectComboBox), new PropertyMetadata(null));

        public IEnumerable<object> SelectedItems
        {
            get { return (IEnumerable<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable<object>), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) =>
                {
                    var box = (MultiSelectComboBox)d;

                    box.UpdateFilteredSource();
                }));



        public static bool GetIsSelectedItemsItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedItemsItemProperty);
        }

        public static void SetIsSelectedItemsItem(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedItemsItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsSelectedItemsItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedItemsItemProperty =
            DependencyProperty.RegisterAttached("IsSelectedItemsItem", typeof(bool), typeof(MultiSelectComboBox), new PropertyMetadata(false));



        static MultiSelectComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(typeof(MultiSelectComboBox)));
        }

        public MultiSelectComboBox()
        {
            CommandBindings.Add(new CommandBinding(RemoveSelectedItem,
                (s, e) =>
                {
                    RemoveFromSelectedItems(e.Parameter);
                }));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            editableTextBox = GetTemplateChild("PART_EditableTextBox2") as TextBox;
            selectedItemsControl = GetTemplateChild("PART_SelectedItems") as ItemsControl;

            editableTextBox.TextChanged += EditableTextBox_TextChanged;

            SetBinding(ItemsSourceProperty, new Binding(nameof(FilteredSource))
            {
                Source = this,
                Mode = BindingMode.OneWay,
            });

            OnDisplayMemberPathChanged(DisplayMemberPath, DisplayMemberPath);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (!(e.OriginalSource is DependencyObject dObj)) { return; }

            if (e.OriginalSource == this)
            {
                // ポップアップ表示中にポップアップ外をクリックした時
                return;
            }

            editableTextBox.Focus();

            var boxItem = dObj.FindAncestor<ComboBoxItem>();
            if (boxItem != null)
            {
                AddToSelectedItems(boxItem.DataContext);
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (e.MiddleButton != MouseButtonState.Pressed) { return; }

            if (e.OriginalSource is DependencyObject dObj)
            {
                var selectedItemsItem = dObj.FindAncestors()
                    .FirstOrDefault(x => GetIsSelectedItemsItem(x));
                if (selectedItemsItem is FrameworkElement elem)
                {
                    RemoveFromSelectedItems(elem.DataContext);
                }
            }
        }

        private void AddToSelectedItems(object item)
        {
            if (item == null) { return; }
            var newSelectedItems = SelectedItems?.ToList() ?? new List<object>();
            newSelectedItems.Add(item);
            SetCurrentValue(SelectedItemsProperty, newSelectedItems);

            IsDropDownOpen = false;
            editableTextBox.Text = string.Empty;
            Dispatcher.BeginInvoke((Action)(() => editableTextBox.Focus()));
        }

        private void RemoveFromSelectedItems(object item)
        {
            if (SelectedItems == null || !SelectedItems.Contains(item))
            {
                return;
            }
            var newSelectedItems = SelectedItems.ToList();
            newSelectedItems.Remove(item);
            SetCurrentValue(SelectedItemsProperty, newSelectedItems);

            // 要素が消された時はドロップダウンを閉じておく
            IsDropDownOpen = false;
            Dispatcher.BeginInvoke((Action)(() => editableTextBox.Focus()));
        }

        private void EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(editableTextBox.Text))
            {
                IsDropDownOpen = true;
            }
            UpdateFilteredSource();
        }

        private void UpdateFilteredSource()
        {
            FilteredSource = Source?.Where(x =>
            {
                // 選択済みのアイテムは選択肢に出さない
                if (SelectedItems?.Contains(x) ?? false)
                {
                    return false;
                }

                var filterText = editableTextBox?.Text;
                if (string.IsNullOrEmpty(filterText))
                {
                    return true;
                }

                if (x is string str)
                {
                    return str.Contains(filterText);
                }

                // リフレクションを用いて、DisplayMemberPathで指定されたプロパティ
                // と比較を行う。DislayMemberPathがないなら、
                // ToStringした結果と比較を行う
                if (!string.IsNullOrWhiteSpace(DisplayMemberPath))
                {
                    displayMemberPropertyInfo = displayMemberPropertyInfo ?? x.GetType().GetProperty(DisplayMemberPath);
                    var value = displayMemberPropertyInfo.GetValue(x);
                    return value?.ToString().Contains(filterText) ?? false;
                }

                return x?.ToString().Contains(filterText) ?? false;
            }) ?? Enumerable.Empty<object>();
            
            if (FilteredSource.Any())
            {
                SetCurrentValue(SelectedItemProperty, !string.IsNullOrWhiteSpace(editableTextBox.Text)
                    ? FilteredSource.FirstOrDefault()
                    : null);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (TryChangeSelectedItem(e))
            {
                e.Handled = true;
                return;
            }

            if (IsDropDownOpen && e.Key == Key.Enter)
            {
                AddToSelectedItems(SelectedItem);
            }
            else if (!IsDropDownOpen && e.Key == Key.Back
                    && string.IsNullOrEmpty(editableTextBox.Text))
            {
                RemoveFromSelectedItems(SelectedItems.LastOrDefault());
            }
            else if (!IsDropDownOpen && e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                IsDropDownOpen = true;
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        private bool TryChangeSelectedItem(KeyEventArgs e)
        {
            if (!IsDropDownOpen) { return false; }

            if (Items.Count == 0) { return false; }

            switch (e.Key)
            {
                case Key.Up:
                    SetCurrentValue(SelectedIndexProperty, SelectedIndex <= 0 ? 0 : SelectedIndex - 1);
                    return true;
                case Key.Down:
                    SetCurrentValue(SelectedIndexProperty, SelectedIndex == Items.Count ? SelectedIndex : SelectedIndex + 1);
                    return true;
            }
            return false;
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);

            if (selectedItemsControl == null) { return; }

            // 選択中アイテムのテンプレートを作り直す
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
            border.SetValue(Border.BorderBrushProperty, Brushes.Black);
            border.SetValue(Border.MarginProperty, new Thickness(1));
            border.SetValue(IsSelectedItemsItemProperty, true);

            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            stackPanel.SetValue(StackPanel.HeightProperty, 16D);

            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(newDisplayMemberPath));
            stackPanel.AppendChild(textBlock);

            var button = new FrameworkElementFactory(typeof(Button));
            button.SetValue(Button.ContentProperty, "×");
            button.SetValue(Button.CommandProperty, RemoveSelectedItem);
            button.SetBinding(Button.CommandParameterProperty, new Binding());
            button.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            button.SetValue(Button.BackgroundProperty, Brushes.Transparent);
            button.SetValue(Button.IsTabStopProperty, false);
            button.SetBinding(Button.WidthProperty, new Binding(nameof(Height))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(StackPanel),
                }
            });
            button.SetBinding(Button.HeightProperty, new Binding(nameof(Height))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(StackPanel),
                }
            });
            stackPanel.AppendChild(button);

            border.AppendChild(stackPanel);

            var template = new DataTemplate();
            template.VisualTree = border;
            selectedItemsControl.ItemTemplate = template;

            displayMemberPropertyInfo = null;
        }
    }
}
