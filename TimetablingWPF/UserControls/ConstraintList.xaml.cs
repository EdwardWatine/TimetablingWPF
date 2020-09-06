using ObservableComputations;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimetablingWPF
{
    /// <summary>
    /// Interaction logic for ConstraintList.xaml
    /// </summary>
    public partial class ConstraintList : ItemsControl
    {
        static ConstraintList()
        {
            ItemsSourceProperty.OverrideMetadata(typeof(ConstraintList),
                new FrameworkPropertyMetadata(null, null, new CoerceValueCallback(CoerceItemsSource)));
        }
        public ConstraintList()
        {
            InitializeComponent();
        }
        private static object CoerceItemsSource(DependencyObject d, object value)
        {
            if (d == null)
            {
                return d;
            }
            if (value is INotifyCollectionChanged changeable)
            {
                return new ObservableCollectionExtended<object>() { new UIHook() }.Concatenating(changeable);
            }
            LinkedList<object> ll = new LinkedList<object>(((IEnumerable)value).Cast<object>());
            ll.AddFirst(new UIHook());
            return ll;
        }
        private void DeleteItem(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            border.MouseLeftButtonDown -= DeleteItem;
            border.Style = null;
            border.Background = null;
            Storyboard sb = (Storyboard)border.Resources["sbClose"];
            sb.Completed += delegate (object sender_i, EventArgs e_i) { Delete(border.DataContext); };
            ((DoubleAnimation)sb.Children[1]).From = ((FrameworkElement)sb.Children[1].GetTarget(border)).ActualWidth;
            double ratio = ((FrameworkElement)sb.Children[1].GetTarget(border)).ActualWidth / ((FrameworkElement)sb.Children[0].GetTarget(border)).Width;
            sb.Children[1].Duration = sb.Children[1].Duration.TimeSpan.Scale(ratio);
            sb.Begin();
        }

        private void Delete(object item)
        {
            // delete item
        }
    }
}
