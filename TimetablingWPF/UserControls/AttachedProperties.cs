using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TimetablingWPF
{
    public class ButtonProperties : DependencyObject
    {

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text",
            typeof(string), typeof(ButtonProperties), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ImageProperty = DependencyProperty.RegisterAttached("Image",
            typeof(ImageSource), typeof(ButtonProperties), new FrameworkPropertyMetadata(null));

        public static string GetText(DependencyObject d)
        {
            return (string)d.GetValue(TextProperty);
        }
        public static ImageSource GetImage(DependencyObject d)
        {
            return (ImageSource)d.GetValue(ImageProperty);
        }
        public static void SetText(DependencyObject d, string value)
        {
            d.SetValue(TextProperty, value);
        }
        public static void SetImage(DependencyObject d, ImageSource value)
        {
            d.SetValue(ImageProperty, value);
        }
    }
    public class Editable : DependencyObject
    {
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.RegisterAttached("IsReadOnly", typeof(bool), typeof(Editable));
        public static bool GetIsReadOnly(DependencyObject d)
        {
            return (bool)d.GetValue(IsReadOnlyProperty);
        }
        public static void SetIsReadOnly(DependencyObject d, bool value)
        {
            d.SetValue(IsReadOnlyProperty, value);
        }
    }
}
