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
        static Room()
        {
            Type type = typeof(Room);
            RegisterProperty(type, nameof(Quantity));
            RegisterProperty(type, nameof(Critical));
            RegisterProperty(type, nameof(Groups));
        }
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value != _quantity)
                {
                    _quantity = value;
                    NotifyPropertyChanged(nameof(Quantity));
                }
            }
        }
        private bool _critical;
        public bool Critical
        {
            get => _critical;
            set
            {
                if (value != _critical)
                {
                    _critical = value;
                    NotifyPropertyChanged(nameof(Critical));
                }
            }
        }
        public RelationalCollection<Group, Room> Groups { get; private set; } = new RelationalCollection<Group, Room>(nameof(Group.Rooms));
        private readonly IList<ErrorContainer> errorValidations = new List<ErrorContainer>();
        public override IList<ErrorContainer> ErrorValidations => errorValidations;
    }
}
