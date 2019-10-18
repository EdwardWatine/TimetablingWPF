using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
                Margin = new Thickness(0)
            };
            sp.Children.Add(tb);

            if (@event != null)
            {
                Binding binding = new Binding("ActualHeight") { Source = tb };
                Image img = new Image()
                {
                    Source = (ImageSource)Application.Current.Resources["CrossIcon"],
                    Tag = @object,
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(-5,0,0,0)
                };
                img.SetBinding(Image.HeightProperty, binding);
                img.MouseDown += @event;
                sp.Children.Add(img);
            }
            return sp;
        }
        public static void ShowErrorBox(string msg, string title = "Error")
        {
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static MessageBoxResult ShowWarningBox(string msg, string title = "Warning")
        {
            return MessageBox.Show(msg, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
        }
    }
}
