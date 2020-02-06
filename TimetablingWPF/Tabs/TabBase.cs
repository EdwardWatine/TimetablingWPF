using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimetablingWPF
{
    public class TabBase : TabItem
    {
        public MainPage MainPage => (MainPage)Window.GetWindow(this).Content;

        public virtual bool Cancel()
        {
            return MessageBox.Show("Are you sure you want to discard your changes?",
                "Discard changes?", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }
        public virtual void OnSelect() { }
    }
}
