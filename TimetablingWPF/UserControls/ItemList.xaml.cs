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
    /// <summary>
    /// Interaction logic for ItemList.xaml
    /// </summary>
    public partial class ItemList : ItemsControl
    {
        public ItemList()
        {
            InitializeComponent();
        }
        private void RemoveItemClick(object sender, MouseButtonEventArgs e)
        {
            ((IList)ItemsSource).Remove(((FrameworkElement)sender).Tag);
        }
        public void ListenToCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += CollectionChanged;
        }
        public void UnlistenToCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= CollectionChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object obj in e.NewItems)
                {
                    ((IList)ItemsSource).Add(obj);
                }
            }
            if (e.OldItems != null)
            {
                foreach (object obj in e.NewItems)
                {
                    ((IList)ItemsSource).Remove(obj);
                }
            }
        }

        public static readonly DependencyProperty RepresentationPathProperty =
            DependencyProperty.Register("RepresentationPath", typeof(string), typeof(ItemList));
        public string RepresentationPath
        {
            get { return (string)GetValue(RepresentationPathProperty); }
            set { SetValue(RepresentationPathProperty, value); }
        }
    }
}
