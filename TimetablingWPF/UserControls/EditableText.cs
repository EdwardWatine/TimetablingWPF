using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TimetablingWPF
{
    public partial class EditableText : TextBox
    {
        private string lastText;
        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                lastText = value;
                base.Text = value;
            }
        }
        public EditableText()
        {
            IsKeyboardFocusedChanged += delegate (object sender, DependencyPropertyChangedEventArgs e)
            {
                if ((bool)e.NewValue)
                {
                    SelectAll();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Text))
                    {
                        Text = lastText;
                    }
                    lastText = Text;
                }
            };
            PreviewMouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e)
            {
                if (!IsFocused)
                {
                    Focus();
                    e.Handled = true;
                }
            };
            BorderThickness = new Thickness(0);
            //HorizontalAlignment = HorizontalAlignment.Center;
        }
    }
}
