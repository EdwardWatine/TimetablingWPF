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
        public ItemList(CustomPropertyInfo prop)
        {
            InitializeComponent();
            CustomPropertyInfo = prop;
        }
        public ItemList()
        {
            InitializeComponent();
        }
        private void MouseMoveLink(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl)) ((Hyperlink)sender).TextDecorations = TextDecorations.Underline;
            else ((Hyperlink)sender).TextDecorations = null;
        }
        private void MouseLeaveLink(object sender, MouseEventArgs e)
        {
            ((Hyperlink)sender).TextDecorations = null;
        }
        private void LinkClick(object sender, RoutedEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;
            object obj = ((Hyperlink)sender).Tag;
            if (Window.GetWindow(this) is MainWindow main)
            {
                main.GetMainPage().NewTab(DataHelpers.GenerateItemTab(obj, CommandType.edit), $"Edit {obj.ToString()}");
                return;
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.GetMainPage().NewTab(DataHelpers.GenerateItemTab(obj, CommandType.edit), $"Edit {obj.ToString()}");
                    mainWindow.Activate();
                    return;
                }
            }
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

        public static readonly DependencyProperty CustomPropertyInfoProperty =
            DependencyProperty.Register("CustomPropertyInfo", typeof(CustomPropertyInfo), typeof(ItemList));
        public CustomPropertyInfo CustomPropertyInfo
        {
            get => (CustomPropertyInfo)GetValue(CustomPropertyInfoProperty);
            set => SetValue(CustomPropertyInfoProperty, value);
        }
    }
}
