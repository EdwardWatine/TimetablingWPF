using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TimetablingWPF
{
    class VisualHelpers
    {
        public static Border SetInternalBorder(FrameworkElement element)
        {
            return new Border()
            {
                Child = element,
                Style = (Style)Application.Current.Resources["GridLineInternal"]
            };
        }
        public static StackPanel VerticalMenuItem(object @object, MouseButtonEventHandler @event = null, string repr = null)
        {
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            TextBlock tb = new TextBlock()
            {
                Style = (Style)Application.Current.Resources["DialogText"],
                Text = repr ?? @object.ToString(),
                Margin = new Thickness(0, 0, 5, 0),
                Height = 22,
                Padding = new Thickness(1)
            };
            Image img = new Image()
            {
                Source = (ImageSource)Application.Current.Resources["CrossIcon"],
                Height = 22,
                Tag = @object,
                Cursor = Cursors.Hand
            };
            if (@event != null)
            {
                img.MouseDown += @event;
            }
            sp.Children.Add(tb);
            sp.Children.Add(img);
            return sp;
        }
        public static void ShowErrorBox(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
