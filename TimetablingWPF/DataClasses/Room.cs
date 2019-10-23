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
        public Room(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
        private int _Quantity;
        public int Quantity
        {
            get { return _Quantity; }
            set
            {
                if (value != _Quantity)
                {
                    _Quantity = value;
                    NotifyPropertyChanged("Quantity");
                }
            }
        }
        public RelationalCollection<Group> Groups { get; private set; } = new RelationalCollection<Group>("Rooms");

        public const string ListName = "Rooms";
        protected override string ListNameAbstract => ListName;
    }
}
