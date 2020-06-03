using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for MultiComboBox.xaml
    /// </summary>
    public partial class MultiComboBox : UserControl
    {
        public static readonly DependencyProperty ItemStringProperty =
            DependencyProperty.Register("ItemString", typeof(string), typeof(MultiComboBox));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(MultiComboBox));

        public event SelectionChangedEventHandler SelectionChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set
            {
                if (!ReferenceEquals(ItemsSource, value))
                {
                    SetValue(ItemsSourceProperty, value);
                    ListCollectionView view_l = new ListCollectionView(value);
                    if (value is INotifyCollectionChanged collection)
                    {
                        collection.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e) { 
                            view_l.Refresh();
                            if (e.OldItems != null)
                            {
                                foreach (object item in e.OldItems)
                                {
                                    SelectedItems.Remove(item);
                                }
                            }
                        };
                    }
                    view = view_l;
                    selectedItems.Clear();
                    lbFiltered.ItemsSource = view;
                }
            }
        }
        public string ItemString
        {
            get => (string)GetValue(ItemStringProperty) ?? "NULL";
            set
            {
                if (ItemString != value)
                {
                    SetValue(ItemStringProperty, value);
                    if (!tbMain.IsKeyboardFocused)
                    {
                        SetClosedText();
                    }
                }
            }
        }
        public IList SelectedItems => selectedItems;
        private bool IgnoreSelection = false;
        private readonly ObservableCollection<object> selectedItems = new ObservableCollection<object>();
        private ListCollectionView view;
        private readonly MouseButtonEventHandler mouseCaptureHandler;
        private string lastString = null;
        private bool IgnoreStringUpdate = false;
        public SortingComparer SortingComparer { get; } = new SortingComparer();
        private void SetClosedText()
        {
            if (selectedItems.Count == 0)
            {
                SetText($"No {ItemString.Pluralize()} selected");
                return;
            }
            if (selectedItems.Count == 1)
            {
                SetText($"'{SelectedItems[0]}' selected");
                return;
            }
            SetText($"{selectedItems.Count} {ItemString.Pluralize()} selected");
        }
        private void SetText(string text)
        {
            IgnoreStringUpdate = true;
            tbMain.Text = text;
            IgnoreStringUpdate = false;
        }
        private void BoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreStringUpdate)
            {
                return;
            }
            popup.IsOpen = true;
            if (lastString == tbMain.Text)
            {
                return;
            }
            string target = tbMain.Text.RemoveWhitespace().ToUpperInvariant();
            lastString = tbMain.Text;
            if (string.IsNullOrWhiteSpace(target))
            {
                view.Filter = null;
                view.CustomSort = null;
                return;
            }
            view.Filter = DataHelpers.GenerateNameFilter(target, o => o.ToString());
            SortingComparer.Filter = target;
            view.CustomSort = SortingComparer;
            UpdateStatusBox();
        }
        private void UpdateStatusBox()
        {
            if (ItemsSource.Count == 0)
            {
                tbStatus.Text = $"No {ItemString.Pluralize()} to display";
                tbStatus.Visibility = Visibility.Visible;
            }
            else if (view.Count == 0)
            {
                tbStatus.Text = $"No matching {ItemString.Pluralize()} found";
                tbStatus.Visibility = Visibility.Visible;
            }
            else
            {
                tbStatus.Visibility = Visibility.Collapsed;
            }
        }
        private void ReleaseControl(object sender, MouseButtonEventArgs e)
        {
            SetClosedText();
            RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, mouseCaptureHandler);
            popup.IsOpen = false;
            Mouse.Capture(null);
            Keyboard.ClearFocus();
        }
        private void BoxFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsKeyboardFocusWithin && !popup.IsKeyboardFocusWithin)
            {
                ReleaseControl(null, null);
            }
            else
            {
                SetText(lastString);
                tbMain.SelectAll();
                AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, mouseCaptureHandler, true);
                BoxTextChanged(null, null);
                Mouse.Capture(this, CaptureMode.SubTree);
                UpdateStatusBox();
            }
            
        }
        private void Select(object sender, MouseButtonEventArgs e)
        {
            object item = lbFiltered.SelectedItem;
            Select(item);
            Keyboard.Focus(tbMain);
        }
        private void RaiseSelectionChanged(IList removed, IList added)
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent, removed, added)); 
        }
        private void Select(object item)
        {
            if (item != null && !IgnoreSelection)
            {
                IgnoreSelection = true;
                ItemsSource.Remove(item);
                selectedItems.Add(item);
                RaiseSelectionChanged(new List<object>(), new List<object>() { item });
                ClearSelections();
                IgnoreSelection = false;
            }
        }
        private void ClearSelections()
        {
            lbSelected.UnselectAll();
            lbFiltered.UnselectAll();
        }
        private void Deselect(object sender, MouseButtonEventArgs e)
        {
            object item = lbSelected.SelectedItem;
            Deselect(item);
            Keyboard.Focus(tbMain);
        }
        private void Deselect(object item)
        {
            if (item != null && !IgnoreSelection)
            {
                IgnoreSelection = true;
                ItemsSource.Add(item);
                selectedItems.Remove(item);
                RaiseSelectionChanged(new List<object>() { item }, new List<object>());
                ClearSelections();
                IgnoreSelection = false;
                UpdateStatusBox();
            }
        }
        private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!e.IsNotPropertyChanged())
            {
                return;
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    Deselect(item);
                }
            }
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    Select(item);
                }
            }
            BoxFocusChanged(null, new DependencyPropertyChangedEventArgs());
        }
        public void SetSelected(IEnumerable enumerable)
        {
            foreach (object item in enumerable)
            {
                Select(item);
            }
        }
        public MultiComboBox()
        {
            InitializeComponent();
            ItemsSource = new ObservableCollection<object>();
            lbSelected.ItemsSource = selectedItems;
            lbSelected.SelectedItem = null;
            mouseCaptureHandler = new MouseButtonEventHandler(ReleaseControl);
            SetClosedText();
            tbMain.TextChanged += BoxTextChanged;
            tbMain.IsKeyboardFocusedChanged += BoxFocusChanged;
            tbMain.PreviewMouseDown += delegate (object sender, MouseButtonEventArgs e)
            {
                Keyboard.Focus(tbMain);
                e.Handled = true;
            };
            lbFiltered.PreviewMouseUp += Select;
            lbSelected.PreviewMouseUp += Deselect;
            selectedItems.CollectionChanged += SelectedItemsChanged;
            ClearSelections();
        }

        public MultiComboBox(string itemString) : this()
        {
            ItemString = itemString;
        }

        private void FilterClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
