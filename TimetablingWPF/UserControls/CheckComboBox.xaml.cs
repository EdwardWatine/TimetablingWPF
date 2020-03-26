using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

namespace TimetablingWPF
{
    public partial class CheckComboBox : ComboBox
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(CheckComboBox));
        private readonly Dictionary<object, CheckBox> checkboxMapping = new Dictionary<object, CheckBox>();
        private const string topItem = "Toggle Select All";
        private int Itemscount => base.Items.Count;
        public new ItemCollection Items => throw new InvalidOperationException("Items cannot be accessed; use ItemsSource instead.");
        public IEnumerable SelectedItems
        {
            get => selection;
            set
            {
                selection.Clear();
                selection.Add(topItem);
                selection.AddRange(value.Cast<object>());
            }
        }
        private readonly InternalObservableCollection<object> selection = new InternalObservableCollection<object>();
        public new object SelectedItem => selection.FirstOrDefault();
        public new IEnumerable ItemsSource
        {
            get => base.ItemsSource;
            set
            {
                IList list = value.Cast<object>().ToList();
                list.Insert(0, topItem);
                base.ItemsSource = list;
            }
        }
        public CheckComboBox()
        {
            selection.CollectionChanged += SelectionItemsChanged;
            Text = "0 selected";
            IsEditable = true;
            IsReadOnly = true;
        }
        private void SelectionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Text = $"{selection.Count} selected";
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    checkboxMapping[item].IsChecked = true;
                }
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    checkboxMapping[item].IsChecked = false;
                }
            }
            checkboxMapping[topItem].IsChecked = selection.Count == Itemscount;
        }
        private void CheckboxClicked(object sender, RoutedEventArgs e)
        {
            CheckBox cbox = (CheckBox)sender;
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
            if (cbox.Tag == topItem)
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
            {
                int selcount = selection.Count;
                selection.Clear();
                if (selcount != Itemscount) {
                    selection.AddRange(ItemsSource.Cast<object>().Skip(1));
                }
            }
            else {
                if (cbox.IsChecked ?? false)
                {
                    selection.Add(cbox.Tag);
                }
                else
                {
                    selection.Remove(cbox.Tag);
                }
            }
        }
        private void CheckBoxLoaded(object sender, RoutedEventArgs e)
        {
            CheckBox cbox = (CheckBox)sender;
            checkboxMapping[cbox.Tag] = cbox;
        }
    }
}
