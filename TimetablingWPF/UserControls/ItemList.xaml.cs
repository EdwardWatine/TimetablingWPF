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
using static TimetablingWPF.GenericHelpers;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for ItemList.xaml
    /// </summary>
    public partial class ItemList : ItemsControl
    {
        public ItemList(CustomPropertyInfo prop) : this()
        {
            InitializeComponent();
            CustomPropertyInfo = prop;
        }
        public ItemList()
        {
            InitializeComponent();
            DeleteAction = o => ((IList)ItemsSource).Remove(o);
        }
        private new IEnumerable ItemsSource
        {
            get => base.ItemsSource;
            set
            {
                base.ItemsSource = value.Cast<object>().ToList();
                handler = GenerateLinkHandler((IList)base.ItemsSource);
            }
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
                main.GetMainPage().NewTab(DataHelpers.GenerateItemTab(obj, CommandType.edit), $"Edit {obj}");
                return;
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.GetMainPage().NewTab(DataHelpers.GenerateItemTab(obj, CommandType.edit), $"Edit {obj}");
                    mainWindow.Activate();
                    return;
                }
            }
        }
        private NotifyCollectionChangedEventHandler handler;
        public Action<object> DeleteAction { get; set; }
        private void RemoveItemClick(object sender, MouseButtonEventArgs e)
        {
            DeleteAction(((FrameworkElement)sender).Tag);
        }
        public void ListenToCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += handler;
        }
        public void UnlistenToCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= handler;
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
