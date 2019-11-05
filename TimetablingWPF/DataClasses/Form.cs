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
    public class Form : BaseDataClass
    {
        public ObservableCollection<Lesson> Lessons { get; private set; } = new ObservableCollection<Lesson>();
        private int _year;
        public int YearGroup
        {
            get { return _year; }
            set
            {
                if (value != _year)
                {
                    _year = value;
                    NotifyPropertyChanged("YearGroup");
                }
            }
        }
        public const string ListName = "Forms";
        protected override string ListNameAbstract => ListName;
    }
}