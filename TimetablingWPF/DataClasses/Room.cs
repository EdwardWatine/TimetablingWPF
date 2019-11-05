using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Diagnostics;
using System.Collections.Specialized;

namespace TimetablingWPF
{
    public class Room : BaseDataClass
    {
        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (value != _quantity)
                {
                    _quantity = value;
                    NotifyPropertyChanged("Quantity");
                }
            }
        }
        private bool _critical;
        public bool Critical
        {
            get { return _critical; }
            set
            {
                if (value != _critical)
                {
                    _critical = value;
                    NotifyPropertyChanged("Critical");
                }
            }
        }
        public RelationalCollection<Group> Groups { get; private set; } = new RelationalCollection<Group>("Rooms");

        public const string ListName = "Rooms";
        protected override string ListNameAbstract => ListName;
    }
}
