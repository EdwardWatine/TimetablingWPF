using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;

namespace TimetablingWPF.Searching
{
    public abstract class SearchBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected SelectionChangedEventHandler GenerateSelectionChangedHandler(string prop)
        {
            return new SelectionChangedEventHandler((sender, e) => RaisePropertyChanged(prop));
        }
        protected void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public abstract UIElement GenerateUI();
        public abstract bool Search(object item);
    }
    public abstract class SearchFactory
    {
        public abstract SearchBase GenerateSearch();
    }
}
